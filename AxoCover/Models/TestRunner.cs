using AxoCover.Models.Data;
using AxoCover.Models.Data.CoverageReport;
using AxoCover.Models.Data.TestReport;
using AxoCover.Models.Events;
using AxoCover.Models.Extensions;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AxoCover.Models
{
  public abstract class TestRunner : ITestRunner
  {
    private readonly Dispatcher _dispatcher = Application.Current.Dispatcher;

    public event EventHandler TestsStarted;
    public event TestExecutedEventHandler TestExecuted;
    public event TestLogAddedEventHandler TestLogAdded;
    public event TestFinishedEventHandler TestsFinished;
    public event EventHandler TestsFailed;

    private const string _runnerName = "Runner\\OpenCover.Console.exe";
    protected readonly static string _runnerPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _runnerName);

    public void RunTestsAsync(TestItem testItem, string testSettings = null)
    {
      TestsStarted?.Invoke(this, EventArgs.Empty);
      Task.Run(() => RunTests(testItem, testSettings));
    }

    protected abstract void RunTests(TestItem testItem, string testSettings);

    protected void OnTestLogAdded(string text)
    {
      _dispatcher.BeginInvoke(() => TestLogAdded?.Invoke(this, new TestLogAddedEventArgs(text)));
    }

    protected void OnTestExecuted(string path, TestState outcome)
    {
      _dispatcher.BeginInvoke(() => TestExecuted?.Invoke(this, new TestExecutedEventArgs(path, outcome)));
    }

    protected void OnTestsFinished(CoverageSession coverageReport, TestRun testReport)
    {
      if (coverageReport != null && testReport != null)
      {
        _dispatcher.BeginInvoke(() => TestsFinished?.Invoke(this, new TestFinishedEventArgs(coverageReport, testReport)));
      }
      else
      {
        _dispatcher.BeginInvoke(() => TestsFailed?.Invoke(this, EventArgs.Empty));
      }
    }
  }
}
