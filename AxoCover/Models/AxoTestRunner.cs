using AxoCover.Common.Extensions;
using AxoCover.Common.ProcessHost;
using AxoCover.Common.Runner;
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
    private readonly TimeSpan _debuggerTimeout = TimeSpan.FromSeconds(10);

    public AxoTestRunner(IEditorContext editorContext, IStorageController storageController, IOptions options)
    {
      _editorContext = editorContext;
      _storageController = storageController;
      _options = options;
    }

    protected override TestReport RunTests(TestItem testItem, bool isCovering, bool isDebugging)
    {
      List<TestResult> testResults = new List<TestResult>();
      try
      {
        var outputDirectory = _storageController.CreateTestRunDirectory();

        var testMethods = testItem
          .Flatten(p => p.Children)
          .OfType<TestMethod>()
          .Where(p => p.Case != null)
          .ToArray();
        var testCases = testMethods
          .Select(p => p.Case)
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

        var finishEvent = new ManualResetEvent(false);
        _executionProcess = ExecutionProcess.Create(hostProcessInfo, _options.TestPlatform);
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
            var testResult = e.Value.ToTestResult(testMethod);
            testResults.Add(testResult);
            OnTestExecuted(testResult);
          }
        };
        _executionProcess.OutputReceived += (o, e) => OnTestLogAdded(e.Value);
        _executionProcess.TestsFinished += (o, e) => finishEvent.Set();
        _executionProcess.Exited += (o, e) => finishEvent.Set();
        _executionProcess.DebuggerDetachmentRequested += (o, e) => _editorContext.DetachFromProcess(e.Value);

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
          AdapterSources = AdapterExtensions.GetAdapters(),
          RunSettingsPath = _options.TestSettings,
          ApartmentState = _options.TestApartmentState,
          OutputPath = outputDirectory,
          SolutionPath = solution.FilePath
        };
        _executionProcess.RunTestsAsync(testCases, options);

        finishEvent.WaitOne();
        if (!_isAborting)
        {
          OnTestLogAdded(Resources.ShuttingDown);
          if (!_executionProcess.TryShutdown())
          {
            OnTestLogAdded(Resources.ShutdownFailed);
          }
        }

        if (isCovering)
        {
          OnTestLogAdded(Resources.GeneratingCoverageReport);
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
