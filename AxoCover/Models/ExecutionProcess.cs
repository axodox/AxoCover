using AxoCover.Common.Events;
using AxoCover.Common.Extensions;
using AxoCover.Common.ProcessHost;
using AxoCover.Common.Runner;
using AxoCover.Common.Settings;
using AxoCover.Models.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;

namespace AxoCover.Models
{
  public class ExecutionProcess : ServiceProcess, ITestExecutionMonitor
  {
    private const int _reconnectTimeout = 100;
    private readonly ManualResetEvent _serviceStartedEvent = new ManualResetEvent(false);
    private readonly Timer _reconnectTimer;
    private int _executionProcessId;
    private ITestExecutionService _testExecutionService;

    public event EventHandler<EventArgs<string>> MessageReceived;
    public event EventHandler<EventArgs<TestCase>> TestStarted;
    public event EventHandler<EventArgs<TestCase>> TestEnded;
    public event EventHandler<EventArgs<TestResult>> TestResult;
    public event EventHandler TestsFinished;
    public event EventHandler DebuggerAttached;

    private ExecutionProcess(IHostProcessInfo hostProcess) :
      base(hostProcess.Embed(new ServiceProcessInfo(RunnerMode.Execution, AdapterExtensions.GetTestPlatformPath())))
    {
      _reconnectTimer = new Timer(OnReconnect, null, Timeout.Infinite, Timeout.Infinite);
      _serviceStartedEvent.WaitOne();
    }

    private void OnReconnect(object state)
    {
      _reconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
      try
      {
        ConnectToService(ServiceUri);
      }
      catch
      {
        _reconnectTimer.Change(_reconnectTimeout, _reconnectTimeout);
      }
    }

    public static ExecutionProcess Create(IHostProcessInfo hostProcess = null, TestPlatform testPlatform = TestPlatform.x86)
    {
      hostProcess = hostProcess.Embed(new PlatformProcessInfo(testPlatform)) as IHostProcessInfo;

      var executionProcess = new ExecutionProcess(hostProcess);

      if (executionProcess._testExecutionService == null)
      {
        throw new Exception("Could not create service.");
      }
      else
      {
        return executionProcess;
      }
    }

    protected override void OnServiceStarted()
    {
      ConnectToService(ServiceUri);
      _serviceStartedEvent.Set();
    }

    private void ConnectToService(Uri address)
    {
      var channelFactory = new DuplexChannelFactory<ITestExecutionService>(this, NetworkingExtensions.GetServiceBinding());
      _testExecutionService = channelFactory.CreateChannel(new EndpointAddress(address));
      var executionObject = _testExecutionService as ICommunicationObject;
      executionObject.Faulted += OnExecutionServiceFaulted;
      _executionProcessId = _testExecutionService.Initialize();
    }

    private void OnExecutionServiceFaulted(object sender, EventArgs e)
    {
      if (!HasExited)
      {
        _reconnectTimer.Change(_reconnectTimeout, _reconnectTimeout);
      }
    }

    protected override void OnServiceFailed()
    {
      _reconnectTimer.Dispose();
      _serviceStartedEvent.Set();
    }

    void ITestExecutionMonitor.RecordMessage(TestMessageLevel testMessageLevel, string message)
    {
      var text = testMessageLevel.GetShortName() + " " + message;
      MessageReceived?.Invoke(this, new EventArgs<string>(text));
    }

    void ITestExecutionMonitor.RecordStart(TestCase testCase)
    {
      TestStarted?.Invoke(this, new EventArgs<TestCase>(testCase));
    }

    void ITestExecutionMonitor.RecordEnd(TestCase testCase, TestOutcome outcome)
    {
      TestEnded?.Invoke(this, new EventArgs<TestCase>(testCase));
    }

    void ITestExecutionMonitor.RecordResult(TestResult testResult)
    {
      TestResult?.Invoke(this, new EventArgs<TestResult>(testResult));
    }

    void ITestExecutionMonitor.RecordFinish()
    {
      TestsFinished?.Invoke(this, EventArgs.Empty);
    }

    void ITestExecutionMonitor.RecordDebuggerStatus(bool isAttached)
    {
      if (isAttached)
      {
        DebuggerAttached?.Invoke(this, EventArgs.Empty);
      }
    }

    public void RunTestsAsync(IEnumerable<TestCase> testCases, TestExecutionOptions options)
    {
      _testExecutionService.RunTestsAsync(testCases, options);
    }

    public bool TryShutdown()
    {
      try
      {
        _testExecutionService.Shutdown();
        return true;
      }
      catch
      {
        try
        {
          Process.GetProcessById(_executionProcessId).KillWithChildren();
          return true;
        }
        catch
        {
          return false;
        }
      }
    }
  }
}
