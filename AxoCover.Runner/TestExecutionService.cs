using AxoCover.Common.Extensions;
using AxoCover.Common.Models;
using AxoCover.Common.ProcessHost;
using AxoCover.Common.Runner;
using AxoCover.Common.Settings;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public void RunTestsAsync(IEnumerable<Common.Models.TestCase> testCases, TestExecutionOptions options)
    {
      var thread = new Thread(() => RunTests(testCases, options));
      thread.SetApartmentState(options.ApartmentState.ToApartmentState());
      thread.Start();
    }

    private void RunTests(IEnumerable<Common.Models.TestCase> testCases, TestExecutionOptions options)
    {
      Thread.CurrentThread.Name = "Test executor";
      Thread.CurrentThread.IsBackground = true;

      foreach (var adapterSource in options.AdapterSources)
      {
        LoadExecutors(adapterSource);
      }

      _monitor.RecordMessage(TestMessageLevel.Informational, $"Executing tests...");
      if (!string.IsNullOrEmpty(options.RunSettingsPath))
      {
        _monitor.RecordMessage(TestMessageLevel.Informational, $"Using run settings  {options.RunSettingsPath}.");
      }

      try
      {
        var context = new TestExecutionContext(_monitor, options);
        var testCaseGroups = testCases.GroupBy(p => p.ExecutorUri.ToString().ToLower());
        foreach (var testCaseGroup in testCaseGroups)
        {
          ITestExecutor testExecutor;

          if (_testExecutors.TryGetValue(testCaseGroup.Key.TrimEnd('/'), out testExecutor))
          {
            _monitor.RecordMessage(TestMessageLevel.Informational, $"Running executor: {testExecutor.GetType().FullName}...");
            testExecutor.RunTests(testCaseGroup.Convert(), context, context);
            _monitor.RecordMessage(TestMessageLevel.Informational, $"Executor finished.");
          }
          else
          {
            foreach (var testCase in testCaseGroup)
            {
              var testResult = new Common.Models.TestResult()
              {
                TestCase = testCase,
                ErrorMessage = "Test executor is not loaded.",
                Outcome = Common.Models.TestOutcome.Skipped
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

      if (Debugger.IsAttached)
      {
        ServiceProcess.PrintDebuggerDetachRequest(Process.GetCurrentProcess().Id);
      }
    }

    public void Shutdown()
    {
      _isShuttingDown = true;
      (_monitor as IInvocationBuffer<ITestExecutionMonitor>).Dispose();
    }
  }
}
