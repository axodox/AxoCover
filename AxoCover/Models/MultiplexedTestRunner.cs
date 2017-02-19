using AxoCover.Common.Events;
using AxoCover.Models.Data;
using AxoCover.Models.Events;
using AxoCover.Properties;
using Microsoft.Practices.Unity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public class MultiplexedTestRunner : Multiplexer<ITestRunner>, ITestRunner
  {
    public event EventHandler DebuggingStarted;
    public event EventHandler<EventArgs<TestMethod>> TestStarted;
    public event EventHandler<EventArgs<TestResult>> TestExecuted;
    public event LogAddedEventHandler TestLogAdded;
    public event EventHandler TestsFailed;
    public event EventHandler<EventArgs<TestReport>> TestsFinished;
    public event EventHandler<EventArgs<TestItem>> TestsStarted;
    public event EventHandler TestsAborted;

    public bool IsBusy
    {
      get
      {
        return _implementation.IsBusy;
      }
    }

    public MultiplexedTestRunner(IUnityContainer container) : base(container)
    {
      var selectedImlementation = Settings.Default.TestRunner;
      if (Implementations.Contains(selectedImlementation))
      {
        Implementation = selectedImlementation;
      }

      foreach (var implementation in _implementations.Values)
      {
        implementation.DebuggingStarted += (o, e) => DebuggingStarted?.Invoke(this, e);
        implementation.TestStarted += (o, e) => TestStarted?.Invoke(this, e);
        implementation.TestExecuted += (o, e) => TestExecuted?.Invoke(this, e);
        implementation.TestLogAdded += (o, e) => TestLogAdded?.Invoke(this, e);
        implementation.TestsAborted += (o, e) => TestsAborted?.Invoke(this, e);
        implementation.TestsFailed += (o, e) => TestsFailed?.Invoke(this, e);
        implementation.TestsFinished += (o, e) => TestsFinished?.Invoke(this, e);
        implementation.TestsStarted += (o, e) => TestsStarted?.Invoke(this, e);
      }
    }

    public Task RunTestsAsync(TestItem testItem, string testSettings = null, bool isCovering = true, bool isDebugging = false)
    {
      return _implementation.RunTestsAsync(testItem, testSettings, isCovering, isDebugging);
    }

    public Task AbortTestsAsync()
    {
      return _implementation.AbortTestsAsync();
    }

    protected override void OnImplementationChanged()
    {
      Settings.Default.TestRunner = Implementation;
      base.OnImplementationChanged();
    }
  }
}
