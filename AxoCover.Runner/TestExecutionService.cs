using AxoCover.Common.Extensions;
using AxoCover.Common.Runner;
using AxoCover.Common.Settings;
using AxoCover.Runner.Settings;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading;

namespace AxoCover.Runner
{
  [ServiceBehavior(
    InstanceContextMode = InstanceContextMode.Single,
    ConcurrencyMode = ConcurrencyMode.Multiple,
    AddressFilterMode = AddressFilterMode.Any,
    IncludeExceptionDetailInFaults = true)]
  public class TestExecutionService : ITestExecutionService
  {
    private const int _debuggerTimeout = 100;
    private Dictionary<string, ITestExecutor> _testExecutors = new Dictionary<string, ITestExecutor>(StringComparer.OrdinalIgnoreCase);
    private ITestExecutionMonitor _monitor;
    private bool _exitOnSessionEnd = false;

    public TestExecutionService()
    {
      _monitor = InvocationBuffer.Create<ITestExecutionMonitor>(OnMonitorException);
    }

    private bool OnMonitorException(Exception obj)
    {
      return false;
    }

    public void Initialize()
    {
      var monitor = OperationContext.Current.GetCallbackChannel<ITestExecutionMonitor>();
      (_monitor as IInvocationBuffer<ITestExecutionMonitor>).Target = monitor;
      var monitorObject = monitor as ICommunicationObject;
      monitorObject.Closing += OnMonitorShutdown;
      monitorObject.Faulted += OnMonitorShutdown;
    }

    private void OnMonitorShutdown(object sender, EventArgs e)
    {
      if (_exitOnSessionEnd)
      {
        Program.Exit();
      }
      else
      {
        (_monitor as IInvocationBuffer<ITestExecutionMonitor>).Target = null;
      }
    }

    public string[] TryLoadAdaptersFromAssembly(string filePath)
    {
      try
      {
        _monitor.SendMessage(TestMessageLevel.Informational, $"Loading assembly from {filePath}...");
        var discovererNames = new List<string>();
        var assembly = Assembly.LoadFrom(filePath);
        var implementers = assembly.FindImplementers<ITestExecutor>();
        foreach (var implementer in implementers)
        {
          try
          {
            var uriAttribute = implementer.GetCustomAttribute<ExtensionUriAttribute>();
            if (uriAttribute == null) continue;

            var testExecutor = Activator.CreateInstance(implementer) as ITestExecutor;
            discovererNames.Add(testExecutor.GetType().FullName);

            _testExecutors[uriAttribute.ExtensionUri] = testExecutor;
          }
          catch (Exception e)
          {
            _monitor.SendMessage(TestMessageLevel.Warning, $"Could not instantiate executor {implementer.FullName}.\r\n{e.GetDescription()}");
          }
        }

        _monitor.SendMessage(TestMessageLevel.Informational, $"Assembly loaded.");
        return discovererNames.ToArray();
      }
      catch (Exception e)
      {
        _monitor.SendMessage(TestMessageLevel.Error, $"Could not load assembly {filePath}.\r\n{e.GetDescription()}");
        return new string[0];
      }
    }

    public void RunTests(IEnumerable<TestCase> testCases, string runSettingsPath, TestApartmentState apartmentState)
    {
      var thread = new Thread(() => RunTestsInternal(testCases, runSettingsPath));
      thread.SetApartmentState(apartmentState.ToApartmentState());
      thread.Start();
    }

    private void RunTestsInternal(IEnumerable<TestCase> testCases, string runSettingsPath)
    {
      _monitor.SendMessage(TestMessageLevel.Informational, $"Executing tests...");
      if (runSettingsPath != null)
      {
        _monitor.SendMessage(TestMessageLevel.Informational, $"Using run settings  {runSettingsPath}.");
      }

      try
      {
        var runSettings = new RunSettings(runSettingsPath == null ? null : File.ReadAllText(runSettingsPath));
        var context = new TestExecutionContext(_monitor, runSettings);

        var testCaseGroups = testCases.GroupBy(p => p.ExecutorUri.ToString());
        foreach (var testCaseGroup in testCaseGroups)
        {
          ITestExecutor testExecutor;

          if (_testExecutors.TryGetValue(testCaseGroup.Key.TrimEnd('/'), out testExecutor))
          {
            _monitor.SendMessage(TestMessageLevel.Informational, $"Running executor: {testExecutor.GetType().FullName}...");
            testExecutor.RunTests(testCaseGroup, context, context);
            _monitor.SendMessage(TestMessageLevel.Informational, $"Executor finished.");
          }
          else
          {
            foreach (var testCase in testCaseGroup)
            {
              var testResult = new TestResult(testCase)
              {
                ErrorMessage = "Test executor is not loaded.",
                Outcome = TestOutcome.Skipped
              };
              _monitor.RecordResult(testResult);
            }
          }
        }
        _monitor.SendMessage(TestMessageLevel.Informational, $"Test execution finished.");
      }
      catch (Exception e)
      {
        _monitor.SendMessage(TestMessageLevel.Error, $"Could not execute tests.\r\n{e.GetDescription()}");
      }
      _monitor.RecordFinish();
    }

    public void Shutdown()
    {
      (_monitor as IInvocationBuffer<ITestExecutionMonitor>).Dispose();
      _exitOnSessionEnd = true;
    }

    public bool WaitForDebugger(int timeout)
    {
      var time = 0;
      while (time < timeout && !Debugger.IsAttached)
      {
        time += _debuggerTimeout;
        Thread.Sleep(_debuggerTimeout);
      }

      return Debugger.IsAttached;
    }
  }
}
