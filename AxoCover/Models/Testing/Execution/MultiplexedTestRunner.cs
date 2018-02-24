using AxoCover.Common.Events;
using AxoCover.Models.Storage;
using AxoCover.Models.Testing.Data;
using AxoCover.Models.Toolkit;
using Microsoft.Practices.Unity;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace AxoCover.Models.Testing.Execution
{
  public class MultiplexedTestRunner : Multiplexer<ITestRunner>, ITestRunner
  {
    public event EventHandler DebuggingStarted;
    public event EventHandler<EventArgs<TestMethod>> TestStarted;
    public event EventHandler<EventArgs<TestResult>> TestExecuted;
    public event EventHandler<EventArgs<string>> TestLogAdded;
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

    private readonly IOptions _options;

    public MultiplexedTestRunner(IUnityContainer container, IOptions options) : base(container)
    {
      _options = options;
      UpdateImplementation();

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

      options.PropertyChanged += OnOptionChanged;
    }

    private void UpdateImplementation()
    {
      var selectedImlementation = _options.TestRunner;
      if (Implementations.Contains(selectedImlementation))
      {
        Implementation = selectedImlementation;
      }
    }

    private void OnOptionChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(IOptions.TestRunner)) UpdateImplementation();
    }

    public Task RunTestsAsync(TestItem testItem, bool isCovering = true, bool isDebugging = false)
    {
      return _implementation.RunTestsAsync(testItem, isCovering, isDebugging);
    }

    public Task AbortTestsAsync()
    {
      return _implementation.AbortTestsAsync();
    }
  }
}
