using AxoCover.Common.Events;
using AxoCover.ViewModels;
using System;

namespace AxoCover.Views
{
  /// <summary>
  /// Interaction logic for TerminalExceptionView.xaml
  /// </summary>
  public partial class TerminalExceptionView : View<TerminalExceptionViewModel>, IDialog
  {
    public TerminalExceptionView()
    {
      InitializeComponent();
    }

    public string Title
    {
      get
      {
        return AxoCover.Resources.TerminalException;
      }
    }

    public event EventHandler<EventArgs<bool?>> ClosingDialog;

    public void OnClosing()
    {
      ViewModel.RestartCommand.Execute(null);
    }
  }
}
