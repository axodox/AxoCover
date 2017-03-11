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
    private bool _isShuttingDown = false;

    public TestExecutionService()
    {
      _monitor = InvocationBuffer.Create<ITestExecutionMonitor>(OnMonitorException);
      new Thread(MonitorDebugger).Start();
    }

    private void MonitorDebugger()
    {
      Thread.CurrentThread.Name = "Debugger monitor";
      Thread.CurrentThread.IsBackground = true;

      var isDebuggerAttached = false;
      while (!_isShuttingDown)
      {
        if (isDebuggerAttached != Debugger.IsAttached)
        {
          isDebuggerAttached = Debugger.IsAttached;
          _monitor.RecordDebuggerStatus(isDebuggerAttached);
        }
        Thread.Sleep(100);
      }
    }

    private bool OnMonitorException(Exception obj)
    {
      return false;
    }

    public int Initialize()
    {
      var monitor = OperationContext.Current.GetCallbackChannel<ITestExecutionMonitor>();
      (_monitor as IInvocationBuffer<ITestExecutionMonitor>).Target = monitor;
      var monitorObject = monitor as ICommunicationObject;
      monitorObject.Closing += OnMonitorShutdown;
      monitorObject.Faulted += OnMonitorShutdown;
      return Process.GetCurrentProcess().Id;
    }

    private void OnMonitorShutdown(object sender, EventArgs e)
    {
      if (_isShuttingDown)
      {
        Program.Exit();
      }
      else
      {
        (_monitor as IInvocationBuffer<ITestExecutionMonitor>).Target = null;
      }
    }

    private void LoadExecutors(string adapterSource)
    {
      try
      {
        _monitor.RecordMessage(TestMessageLevel.Informational, $"Loading assembly from {adapterSource}...");
        var assembly = Assembly.LoadFrom(adapterSource);
        var implementers = assembly.FindImplementers<ITestExecutor>();
        foreach (var implementer in implementers)
        {
          try
          {
            var uriAttribute = implementer.GetCustomAttribute<ExtensionUriAttribute>();
            if (uriAttribute == null || _testExecutors.ContainsKey(uriAttribute.ExtensionUri)) continue;

            var testExecutor = Activator.CreateInstance(implementer) as ITestExecutor;
            _testExecutors[uriAttribute.ExtensionUri] = testExecutor;
          }
          catch (Exception e)
          {
            _monitor.RecordMessage(TestMessageLevel.Warning, $"Could not instantiate executor {implementer.FullName}.\r\n{e.GetDescription()}");
          }
        }

        _monitor.RecordMessage(TestMessageLevel.Informational, $"Assembly loaded.");
      }
      catch (Exception e)
      {
        _monitor.RecordMessage(TestMessageLevel.Error, $"Could not load assembly {adapterSource}.\r\n{e.GetDescription()}");
      }
    }

    public void RunTestsAsync(string[] adapterSources, IEnumerable<TestCase> testCases, string runSettingsPath, TestApartmentState apartmentState)
    {
      var thread = new Thread(() => RunTests(adapterSources, testCases, runSettingsPath));
      thread.SetApartmentState(apartmentState.ToApartmentState());
      thread.Start();
    }

    private void RunTests(string[] adapterSources, IEnumerable<TestCase> testCases, string runSettingsPath)
    {
      Thread.CurrentThread.Name = "Test executor";
      Thread.CurrentThread.IsBackground = true;

      foreach (var adapterSource in adapterSources)
      {
        LoadExecutors(adapterSource);
      }

      _monitor.RecordMessage(TestMessageLevel.Informational, $"Executing tests...");
      if (runSettingsPath != null)
      {
        _monitor.RecordMessage(TestMessageLevel.Informational, $"Using run settings  {runSettingsPath}.");
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
            _monitor.RecordMessage(TestMessageLevel.Informational, $"Running executor: {testExecutor.GetType().FullName}...");
            testExecutor.RunTests(testCaseGroup, context, context);
            _monitor.RecordMessage(TestMessageLevel.Informational, $"Executor finished.");
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
        _monitor.RecordMessage(TestMessageLevel.Informational, $"Test execution finished.");
      }
      catch (Exception e)
      {
        _monitor.RecordMessage(TestMessageLevel.Error, $"Could not execute tests.\r\n{e.GetDescription()}");
      }
      _monitor.RecordFinish();
    }

    public void Shutdown()
    {
      _isShuttingDown = true;
      (_monitor as IInvocationBuffer<ITestExecutionMonitor>).Dispose();
    }
  }
}
