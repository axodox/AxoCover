using AxoCover.Models.Data;
using AxoCover.Models.Events;
using System;

namespace AxoCover.Models
{
  public interface ITestRunner
  {
    void RunTestsAsync(TestItem testItem, string testSettings = null);

    event EventHandler TestsStarted;
    event TestExecutedEventHandler TestExecuted;
    event TestLogAddedEventHandler TestLogAdded;
    event TestFinishedEventHandler TestsFinished;
    event EventHandler TestsFailed;
  }
}