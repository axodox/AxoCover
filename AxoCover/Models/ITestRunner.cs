using AxoCover.Common.Events;
using AxoCover.Models.Data;
using AxoCover.Models.Events;
using System;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public interface ITestRunner
  {
    Task RunTestsAsync(TestItem testItem, bool isCovering = true, bool isDebugging = false);
    Task AbortTestsAsync();

    bool IsBusy { get; }

    event EventHandler DebuggingStarted;
    event EventHandler<EventArgs<TestItem>> TestsStarted;
    event EventHandler<EventArgs<TestResult>> TestExecuted;
    event EventHandler<EventArgs<TestMethod>> TestStarted;
    event LogAddedEventHandler TestLogAdded;
    event EventHandler<EventArgs<TestReport>> TestsFinished;
    event EventHandler TestsFailed;
    event EventHandler TestsAborted;
  }
}