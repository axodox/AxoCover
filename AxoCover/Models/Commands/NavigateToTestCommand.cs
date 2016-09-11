using AxoCover.Models.Events;
using System;
using System.Windows.Input;

namespace AxoCover.Models.Commands
{
  public class NavigateToTestCommand : ICommand
  {
#pragma warning disable CS0067 //Not used
    public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067 //Not used

    public event TestNavigatedEventHandler TestNavigated;

    public bool CanExecute(object parameter)
    {
      return TestNavigated != null && parameter is string;
    }

    public void Execute(object parameter)
    {
      TestNavigated?.Invoke(this, new TestNavigatedEventArgs(parameter as string));
    }
  }
}
