using AxoCover.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace AxoCover.Views
{
  /// <summary>
  /// Interaction logic for ReportGeneratorView.xaml
  /// </summary>
  public partial class ReportGeneratorView : View<ReportGeneratorViewModel>, IDialog
  {
    private Window _window;

    public ReportGeneratorView()
    {
      InitializeComponent();
      ViewModel.Finished += OnFinished;
    }

    public void InitializeWindow(Window window)
    {
      _window = window;
      _window.Title = AxoCover.Resources.ReportGenerator;
      _window.MinWidth = 640;
      _window.MinHeight = 480;
      _window.Closing += OnClosing;
    }

    private void OnClosing(object sender, CancelEventArgs e)
    {
      ViewModel.Abort();
    }

    private void OnFinished(object sender, System.EventArgs e)
    {
      if (!ViewModel.IsFailed)
      {
        _window.DialogResult = true;
      }
    }

    private void OnOkButtonClick(object sender, System.Windows.RoutedEventArgs e)
    {
      _window.DialogResult = true;
    }
  }
}
