using AxoCover.Common.Events;
using AxoCover.Models.Data;
using System;
using System.Windows.Input;

namespace AxoCover.Models.Commands
{
  public abstract class TestCommand : ICommand
  {
#pragma warning disable CS0067 //Not used
    public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067 //Not used

    public event EventHandler<EventArgs<TestMethod>> CommandCalled;

    public bool CanExecute(object parameter)
    {
      return CommandCalled != null && parameter is TestMethod;
    }

    public void Execute(object parameter)
    {
      CommandCalled?.Invoke(this, new EventArgs<TestMethod>(parameter as TestMethod));
    }
  }

  public class SelectTestCommand : TestCommand { }

  public class JumpToTestCommand : TestCommand { }

  public class DebugTestCommand : TestCommand { }
}
