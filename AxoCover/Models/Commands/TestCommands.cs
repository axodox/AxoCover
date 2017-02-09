using AxoCover.Models.Events;
using System;
using System.Windows.Input;

namespace AxoCover.Models.Commands
{
  public abstract class TestCommand : ICommand
  {
#pragma warning disable CS0067 //Not used
    public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067 //Not used

    public event EventHandler<EventArgs<string>> CommandCalled;

    public bool CanExecute(object parameter)
    {
      return CommandCalled != null && parameter is string;
    }

    public void Execute(object parameter)
    {
      CommandCalled?.Invoke(this, new EventArgs<string>(parameter as string));
    }
  }

  public class SelectTestCommand : TestCommand { }

  public class JumpToTestCommand : TestCommand { }

  public class DebugTestCommand : TestCommand { }
}
