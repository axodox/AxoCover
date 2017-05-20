using AxoCover.Common.Events;
using AxoCover.Common.Extensions;
using AxoCover.Common.Models;
using AxoCover.Common.ProcessHost;
using AxoCover.Common.Runner;
using AxoCover.Common.Settings;
using AxoCover.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;

namespace AxoCover.Models
{
  public class ExecutionProcess : ServiceProcess, ITestExecutionMonitor
  {
    private readonly ManualResetEvent _serviceStartedEvent = new ManualResetEvent(false);
    private int _executionProcessId;
    private ITestExecutionService _testExecutionService;

    public event EventHandler<EventArgs<string>> MessageReceived;
    public event EventHandler<EventArgs<TestCase>> TestStarted;
    public event EventHandler<EventArgs<TestCase>> TestEnded;
    public event EventHandler<EventArgs<TestResult>> TestResult;
    public event EventHandler DebuggerAttached;

    private ExecutionProcess(IHostProcessInfo hostProcess, string[] testPlatformAssemblies) :
      base(hostProcess.Embed(new ServiceProcessInfo(RunnerMode.Execution, testPlatformAssemblies)))
    {
      Exited += OnExited;
      _serviceStartedEvent.WaitOne();
    }

    private void OnExited(object sender, EventArgs e)
    {
      if(_testExecutionService != null)
      {
        (_testExecutionService as ICommunicationObject).Abort();
      }
    }

    public static ExecutionProcess Create(string[] testPlatformAssemblies, IHostProcessInfo hostProcess = null, TestPlatform testPlatform = TestPlatform.x86)
    {
      hostProcess = hostProcess.Embed(new PlatformProcessInfo(testPlatform)) as IHostProcessInfo;

      var executionProcess = new ExecutionProcess(hostProcess, testPlatformAssemblies);

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
      _executionProcessId = _testExecutionService.Initialize();
    }

    protected override void OnServiceFailed(SerializableException exception)
    {
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

    void ITestExecutionMonitor.RecordDebuggerStatus(bool isAttached)
    {
      if (isAttached)
      {
        DebuggerAttached?.Invoke(this, EventArgs.Empty);
      }
    }

    public void RunTests(IEnumerable<TestCase> testCases, TestExecutionOptions options)
    {
      _testExecutionService.RunTests(testCases, options);
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
