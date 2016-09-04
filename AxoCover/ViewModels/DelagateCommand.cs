using System;
using System.Windows.Input;

namespace AxoCover.ViewModels
{
  public class DelegateCommand : ICommand
  {
    public event EventHandler CanExecuteChanged;

    private Action<object> _execute;
    private Predicate<object> _canExecute;

    public DelegateCommand(Action<object> execute, Predicate<object> canExecute = null, Action<Action> canExecuteChanged = null)
    {
      _execute = execute;
      _canExecute = canExecute;
      canExecuteChanged?.Invoke(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
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
