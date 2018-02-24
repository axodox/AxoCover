using AxoCover.Common.Events;
using AxoCover.Common.Extensions;
using AxoCover.Common.Models;
using AxoCover.Common.ProcessHost;
using AxoCover.Common.Runner;
using AxoCover.Models.Storage;
using System;

namespace AxoCover.Models.Testing.Execution
{
  public class ExecutionProcess : TestProcess<ITestExecutionService>, ITestExecutionMonitor
  {
    public event EventHandler<EventArgs<string>> MessageReceived;
    public event EventHandler<EventArgs<TestCase>> TestStarted;
    public event EventHandler<EventArgs<TestCase>> TestEnded;
    public event EventHandler<EventArgs<TestResult>> TestResult;
    public event EventHandler DebuggerAttached;

    private ExecutionProcess(IHostProcessInfo hostProcess, string[] testPlatformAssemblies, IOptions options) :
      base(hostProcess.Embed(new ServiceProcessInfo(RunnerMode.Execution, options.TestProtocol, options.IsDebugModeEnabled, testPlatformAssemblies)), options) { }
    
    public static ExecutionProcess Create(string[] testPlatformAssemblies, IHostProcessInfo hostProcess, IOptions options)
    {
      hostProcess = hostProcess.Embed(new PlatformProcessInfo(options.TestPlatform)) as IHostProcessInfo;

      return new ExecutionProcess(hostProcess, testPlatformAssemblies, options);
    }
    
    void ITestExecutionMonitor.RecordMessage(TestMessageLevel testMessageLevel, string message)
    {
      MessageReceived?.Invoke(this, new EventArgs<string>(message));
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

    public void RunTests(TestExecutionTask[] testExecutionTasks, TestExecutionOptions options)
    {
      TestService.RunTests(testExecutionTasks, options);
    }
  }
}
