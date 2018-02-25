using AxoCover.Common.Events;
using AxoCover.Common.Extensions;
using AxoCover.Models.Testing.Data;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AxoCover.Models.Testing.Execution
{
  public abstract class TestRunner : ITestRunner
  {
    private readonly Dispatcher _dispatcher = Application.Current.Dispatcher;

    public event EventHandler DebuggingStarted;
    public event EventHandler<EventArgs<TestItem>> TestsStarted;
    public event EventHandler<EventArgs<TestMethod>> TestStarted;
    public event EventHandler<EventArgs<TestResult>> TestExecuted;
    public event EventHandler<EventArgs<string>> TestLogAdded;
    public event EventHandler<EventArgs<TestReport>> TestsFinished;
    public event EventHandler TestsFailed;
    public event EventHandler TestsAborted;

    private const string _runnerName = @"OpenCover\OpenCover.Console.exe";
    protected readonly static string _runnerPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _runnerName);
    private Task _testTask;
    protected bool _isAborting;

    public bool IsBusy
    {
      get
      {
        return _testTask != null;
      }
    }

    public Task RunTestsAsync(TestItem testItem, bool isCovering = true, bool isDebugging = false)
    {
      if (IsBusy)
      {
        throw new InvalidOperationException("The test runner is busy. Please wait for tests to complete or abort.");
      }

      _testTask = Task.Run(() =>
      {
        try
        {
          OnTestLogAdded(Resources.TestExecutionStarted);
          var result = RunTests(testItem, isCovering, isDebugging);

          _testTask = null;
          OnTestsFinished(result);
          OnTestLogAdded(Resources.TestExecutionFinished);
        }
        catch (Exception e)
        {
          if (!_isAborting)
          {
            OnTestLogAdded(Resources.TestExecutionFailed);
            OnTestLogAdded(e.GetDescription());
          }
          else
          {
            OnTestLogAdded(Resources.TestRunAborted);
          }

          _testTask = null;
          OnTestsFinished(null);
        }
        finally
        {
          _testTask = null;
          _isAborting = false;
        }
      });
      TestsStarted?.Invoke(this, new EventArgs<TestItem>(testItem));
      return _testTask;
    }

    protected abstract TestReport RunTests(TestItem testItem, bool isCovering, bool isDebugging);

    protected void OnTestLogAdded(string text)
    {
      _dispatcher.BeginInvoke(() => TestLogAdded?.Invoke(this, new EventArgs<string>(text)));
    }

    protected void OnDebuggingStarted()
    {
      _dispatcher.BeginInvoke(() => DebuggingStarted?.Invoke(this, EventArgs.Empty));
    }

    protected void OnTestStarted(TestMethod testMethod)
    {
      _dispatcher.BeginInvoke(() => TestStarted?.Invoke(this, new EventArgs<TestMethod>(testMethod)));
    }

    protected void OnTestExecuted(TestResult testResult)
    {
      _dispatcher.BeginInvoke(() => TestExecuted?.Invoke(this, new EventArgs<TestResult>(testResult)));
    }

    private void OnTestsFinished(TestReport testReport)
    {
      if (_isAborting)
      {
        _dispatcher.BeginInvoke(() => TestsAborted?.Invoke(this, EventArgs.Empty));
      }
      else if (testReport != null)
      {
        _dispatcher.BeginInvoke(() => TestsFinished?.Invoke(this, new EventArgs<TestReport>(testReport)));
      }
      else
      {
        _dispatcher.BeginInvoke(() => TestsFailed?.Invoke(this, EventArgs.Empty));
      }
    }

    public Task AbortTestsAsync()
    {
      if (IsBusy)
      {
        _isAborting = true;
        AbortTests();
      }

      var testTask = _testTask;
      if(testTask != null)
      {
        return testTask;
      }
      else
      {
        var taskCompletionSource = new TaskCompletionSource<object>();
        taskCompletionSource.SetResult(null);
        return taskCompletionSource.Task;
      }      
    }

    protected abstract void AbortTests();
  }
}
