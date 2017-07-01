using AxoCover.Common.Extensions;
using AxoCover.Common.Models;
using AxoCover.Common.Runner;
using AxoCover.Common.Settings;
using AxoCover.Runner.Properties;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Diagnostics;
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
    private bool _isFinalizing = false;
    private ITestExecutionMonitor _monitor;

    private void MonitorDebugger()
    {
      Thread.CurrentThread.Name = "Debugger monitor";
      Thread.CurrentThread.IsBackground = true;

      var isDebuggerAttached = false;
      while (!_isFinalizing)
      {
        if (isDebuggerAttached != Debugger.IsAttached)
        {
          isDebuggerAttached = Debugger.IsAttached;
          _monitor.RecordDebuggerStatus(isDebuggerAttached);
        }
        Thread.Sleep(100);
      }
    }

    public int Initialize()
    {
      _monitor = OperationContext.Current.GetCallbackChannel<ITestExecutionMonitor>();      
      new Thread(MonitorDebugger).Start();
      return Process.GetCurrentProcess().Id;
    }

    private bool TryLoadExecutor(string adapterSource, string extensionUri, out ITestExecutor testExecutor)
    {
      testExecutor = null;
      try
      {
        _monitor.RecordMessage(TestMessageLevel.Informational, $">> Loading assembly from {adapterSource}...");
        var assembly = Assembly.LoadFrom(adapterSource);
        var implementers = assembly.FindImplementers<ITestExecutor>();
        foreach (var implementer in implementers)
        {
          try
          {
            var uriAttribute = implementer.GetCustomAttribute<ExtensionUriAttribute>();
            if (uriAttribute == null || !StringComparer.OrdinalIgnoreCase.Equals(uriAttribute.ExtensionUri, extensionUri)) continue;

            testExecutor = Activator.CreateInstance(implementer) as ITestExecutor;
            _monitor.RecordMessage(TestMessageLevel.Informational, $"|| Loaded executor: {implementer.FullName}");
            break;
          }
          catch (Exception e)
          {
            _monitor.RecordMessage(TestMessageLevel.Warning, $"|| Could not load executor {implementer.FullName}.\r\n{e.GetDescription()}");
          }
        }

        _monitor.RecordMessage(TestMessageLevel.Informational, $"<< Assembly loaded.");
      }
      catch (Exception e)
      {
        _monitor.RecordMessage(TestMessageLevel.Error, $"<< Could not load assembly {adapterSource}!\r\n{e.GetDescription()}");
      }
      return testExecutor != null;
    }

    public void RunTests(TestExecutionTask[] executionTasks, TestExecutionOptions options)
    {
      _monitor.RecordMessage(TestMessageLevel.Informational, Resources.Branding);

      var thread = new Thread(() => RunTestsWorker(executionTasks, options));
      thread.SetApartmentState(options.ApartmentState.ToApartmentState());
      thread.Start();
      thread.Join();
    }

    private void RunTestsWorker(TestExecutionTask[] executionTasks, TestExecutionOptions options)
    {
      Thread.CurrentThread.Name = "Test executor";
      Thread.CurrentThread.IsBackground = true;
      
      _monitor.RecordMessage(TestMessageLevel.Informational, $"> Executing tests...");
      if (!string.IsNullOrEmpty(options.RunSettingsPath))
      {
        _monitor.RecordMessage(TestMessageLevel.Informational, $"| Using run settings  {options.RunSettingsPath}.");
      }

      try
      {
        var context = new TestExecutionContext(_monitor, options);
        foreach (var executionTask in executionTasks)
        {
          Program.ExecuteWithFileRedirection(executionTask.TestAdapterOptions, () =>
          {
            if (TryLoadExecutor(executionTask.TestAdapterOptions.AssemblyPath, executionTask.TestAdapterOptions.ExtensionUri, out var testExecutor))
            {
              _monitor.RecordMessage(TestMessageLevel.Informational, $">> Running executor: {testExecutor.GetType().FullName}...");
              testExecutor.RunTests(executionTask.TestCases.Convert(), context, context);
              _monitor.RecordMessage(TestMessageLevel.Informational, $"<< Executor finished.");
            }
            else
            {
              foreach (var testCase in executionTask.TestCases)
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
          }, (level, message) => _monitor.RecordMessage(level, "| " + message));
        }
        _monitor.RecordMessage(TestMessageLevel.Informational, $"< Test execution finished.");
      }
      catch (Exception e)
      {
        _monitor.RecordMessage(TestMessageLevel.Error, $"< Could not execute tests.\r\n{e.GetDescription().PadLinesLeft("< ")}");
      }
    }

    public void Shutdown()
    {
      Program.Exit();
    }

    ~TestExecutionService()
    {
      _isFinalizing = true;
    }
  }
}
