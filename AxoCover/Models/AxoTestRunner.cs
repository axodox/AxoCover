using AxoCover.Common.Extensions;
using AxoCover.Common.ProcessHost;
using AxoCover.Common.Runner;
using AxoCover.Models.Adapters;
using AxoCover.Models.Data;
using AxoCover.Models.Data.CoverageReport;
using AxoCover.Models.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace AxoCover.Models
{
  public class AxoTestRunner : TestRunner
  {
    private ExecutionProcess _executionProcess;
    private readonly IEditorContext _editorContext;
    private readonly IStorageController _storageController;
    private readonly IOptions _options;
    private readonly ITelemetryManager _telemetryManager;
    private readonly ITestAdapterRepository _testAdapterRepository;
    private readonly IIoProvider _ioProvider;
    private readonly TimeSpan _debuggerTimeout = TimeSpan.FromSeconds(10);
    private int _sessionId = 0;

    public AxoTestRunner(IEditorContext editorContext, IStorageController storageController, IOptions options, ITelemetryManager telemetryManager, ITestAdapterRepository testAdapterRepository, IIoProvider ioProvider)
    {
      _editorContext = editorContext;
      _storageController = storageController;
      _options = options;
      _telemetryManager = telemetryManager;
      _testAdapterRepository = testAdapterRepository;
      _ioProvider = ioProvider;
    }

    protected override TestReport RunTests(TestItem testItem, bool isCovering, bool isDebugging)
    {
      List<TestResult> testResults = new List<TestResult>();
      try
      {
        var sessionId = _sessionId++;
        var outputDirectory = _storageController.CreateTestRunDirectory();

        var testMethods = testItem
          .Flatten(p => p.Children)
          .OfType<TestMethod>()
          .Where(p => p.Case != null)
          .ToArray();

        var testExecutionTasks = testMethods
          .GroupBy(p => p.TestAdapterName)
          .Distinct()
          .Select(p => new TestExecutionTask()
          {
            TestCases = p.Where(q => q.Case != null).Select(q => q.Case).ToArray(),
            TestAdapterOptions = _testAdapterRepository.Adapters[p.Key]
              .GetLoadingOptions()
              .Do(q => q.IsRedirectingAssemblies = _options.IsRedirectingFrameworkAssemblies)
          })
          .ToArray();
                
        var testMethodsById = testMethods.ToDictionary(p => p.Case.Id);
        
        IHostProcessInfo hostProcessInfo = null;

        var solution = testItem.GetParent<TestSolution>();
        if (isCovering)
        {
          var openCoverOptions = new OpenCoverOptions()
          {
            CodeAssemblies = solution.CodeAssemblies,
            TestAssemblies = solution.TestAssemblies,
            CoverageReportPath = Path.Combine(outputDirectory, "coverageReport.xml"),
            IsCoveringByTest = _options.IsCoveringByTest,
            IsIncludingSolutionAssemblies = _options.IsIncludingSolutionAssemblies,
            IsExcludingTestAssemblies = _options.IsExcludingTestAssemblies,
            IsMergingByHash = _options.IsMergingByHash,
            IsSkippingAutoProps = _options.IsSkippingAutoProps,
            ExcludeAttributes = _options.ExcludeAttributes,
            ExcludeDirectories = _options.ExcludeDirectories,
            ExcludeFiles = _options.ExcludeFiles,
            Filters = _options.Filters
          };
          hostProcessInfo = new OpenCoverProcessInfo(openCoverOptions);
        }

        _executionProcess = ExecutionProcess.Create(AdapterExtensions.GetTestPlatformAssemblyPaths(_options.TestAdapterMode), hostProcessInfo, _options.TestPlatform, _options.TestProtocol);
        _executionProcess.MessageReceived += (o, e) => OnTestLogAdded(e.Value);
        _executionProcess.TestStarted += (o, e) =>
        {
          var testMethod = testMethodsById.TryGetValue(e.Value.Id);
          if (testMethod != null)
          {
            OnTestStarted(testMethod);
          }
        };
        _executionProcess.TestResult += (o, e) =>
        {
          var testMethod = testMethodsById.TryGetValue(e.Value.TestCase.Id);
          if (testMethod != null)
          {
            var testResult = e.Value.ToTestResult(testMethod, sessionId);
            testResults.Add(testResult);
            OnTestExecuted(testResult);
          }
        };
        _executionProcess.OutputReceived += (o, e) => OnTestLogAdded(e.Value);

        if (isDebugging)
        {
          var debuggerAttachedEvent = new ManualResetEvent(false);
          _executionProcess.DebuggerAttached += (o, e) => debuggerAttachedEvent.Set();

          OnTestLogAdded(Resources.DebuggerAttaching);
          if (_editorContext.AttachToProcess(_executionProcess.ProcessId) &&
            debuggerAttachedEvent.WaitOne(_debuggerTimeout))
          {
            OnTestLogAdded(Resources.DebuggerAttached);
            OnDebuggingStarted();
          }
          else
          {
            OnTestLogAdded(Resources.DebuggerFailedToAttach);
          }
        }

        var options = new TestExecutionOptions()
        {
          RunSettingsPath = _ioProvider.GetAbsolutePath(_options.TestSettings),
          ApartmentState = _options.TestApartmentState,
          OutputPath = outputDirectory,
          SolutionPath = solution.FilePath
        };

        _executionProcess.RunTests(testExecutionTasks, options);

        if (!_isAborting)
        {
          if (isDebugging)
          {
            _editorContext.DetachFromProcess(_executionProcess.ProcessId);
          }

          OnTestLogAdded(Resources.ShuttingDown);
          if (!_executionProcess.TryShutdown())
          {
            OnTestLogAdded(Resources.ShutdownFailed);
          }
          if (isCovering)
          {
            OnTestLogAdded(Resources.GeneratingCoverageReport);
          }
        }

        _executionProcess.WaitForExit();

        if (_isAborting) return null;

        if (isCovering)
        {
          var coverageReportPath = (hostProcessInfo as OpenCoverProcessInfo).CoverageReportPath;
          if (System.IO.File.Exists(coverageReportPath))
          {
            var coverageReport = GenericExtensions.ParseXml<CoverageSession>(coverageReportPath);
            return new TestReport(testResults, coverageReport);
          }
        }
        else
        {
          return new TestReport(testResults, null);
        }
      }
      catch (RemoteException exception) when (exception.RemoteReason != null)
      {
        _telemetryManager.UploadExceptionAsync(exception.RemoteReason);
        throw;
      }
      finally
      {
        if (_executionProcess != null)
        {
          _executionProcess.Dispose();
          _executionProcess = null;
        }
      }

      return null;
    }

    protected override void AbortTests()
    {
      if (_executionProcess != null)
      {
        _executionProcess.Dispose();
      }
    }
  }
}
