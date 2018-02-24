using AxoCover.Common.Events;
using AxoCover.Models.Testing.Data;
using AxoCover.Models.Testing.Execution;
using System;
using System.Windows.Input;

namespace AxoCover.Commands
{
  public abstract class TestCommand : ICommand
  {
    public event EventHandler CanExecuteChanged;

    public event EventHandler<EventArgs<TestMethod>> CommandCalled;

    public virtual bool CanExecute(object parameter)
    {
      return CommandCalled != null && parameter is TestMethod;
    }

    public void Execute(object parameter)
    {
      CommandCalled?.Invoke(this, new EventArgs<TestMethod>(parameter as TestMethod));
    }

    public void RefreshCanExecuteChanged()
    {
      CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
  }

  public class SelectTestCommand : TestCommand { }

  public class JumpToTestCommand : TestCommand { }

  public class DebugTestCommand : TestCommand
  {
    private readonly ITestRunner _testRunner;

    public DebugTestCommand(ITestRunner testRunner)
    {
      _testRunner = testRunner;
      _testRunner.TestsStarted += (o, e) => RefreshCanExecuteChanged();
      _testRunner.TestsFinished += (o, e) => RefreshCanExecuteChanged();
    }

    public override bool CanExecute(object parameter)
    {
      return base.CanExecute(parameter) && !_testRunner.IsBusy;
    }
  }
}
