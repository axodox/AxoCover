using AxoCover.Common.Events;
using System;
using System.Windows.Input;

namespace AxoCover.Models.Commands
{
  public abstract class RelayCommand<T> : ICommand
  {
    event EventHandler ICommand.CanExecuteChanged
    {
      add
      {

      }

      remove
      {

      }
    }

    bool ICommand.CanExecute(object parameter)
    {
      return CanExecute((T)parameter);
    }

    public bool CanExecute(T argument)
    {
      return true;
    }

    void ICommand.Execute(object parameter)
    {
      Execute((T)parameter);
    }

    public void Execute(T argument)
    {
      CommandCalled?.Invoke(this, new EventArgs<T>(argument));
    }

    public event EventHandler<EventArgs<T>> CommandCalled;
  }
}
