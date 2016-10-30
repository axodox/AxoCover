using AxoCover.Models.Data;
using AxoCover.Models.Events;
using System;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public interface ITestRunner
  {
    Task RunTestsAsync(TestItem testItem, string testSettings = null);
    Task AbortTestsAsync();

    bool IsBusy { get; }

    event EventHandler TestsStarted;
    event TestExecutedEventHandler TestExecuted;
    event TestLogAddedEventHandler TestLogAdded;
    event TestFinishedEventHandler TestsFinished;
    event EventHandler TestsFailed;
    event EventHandler TestsAborted;
  }
}