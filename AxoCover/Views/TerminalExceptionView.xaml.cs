using AxoCover.ViewModels;
using System.ComponentModel;
using System.Windows;

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

    public void InitializeWindow(Window window)
    {
      window.Title = AxoCover.Resources.TerminalException;
      window.MinWidth = 640;
      window.MinHeight = 480;
      window.Closing += OnClosing;
    }

    private void OnClosing(object sender, CancelEventArgs e)
    {
      ViewModel.RestartCommand.Execute(null);
    }
  }
}
