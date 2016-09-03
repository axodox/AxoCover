using System;
using System.Windows.Input;

namespace AxoCover.ViewModels
{
  public class DelagateCommand : ICommand
  {
    public event EventHandler CanExecuteChanged;

    private Action<object> _execute;
    private Predicate<object> _canExecute;

    public DelagateCommand(Action<object> execute, Predicate<object> canExecute = null)
    {
      _execute = execute;
      _canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
      return _canExecute == null ? true : _canExecute(parameter);
    }

    public void Execute(object parameter)
    {
      _execute(parameter);
    }
  }
}
