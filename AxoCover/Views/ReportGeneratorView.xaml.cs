using AxoCover.Models.Events;
using AxoCover.ViewModels;
using System;

namespace AxoCover.Views
{
  /// <summary>
  /// Interaction logic for ReportGeneratorView.xaml
  /// </summary>
  public partial class ReportGeneratorView : View<ReportGeneratorViewModel>, IDialog
  {
    public ReportGeneratorView()
    {
      InitializeComponent();
      ViewModel.Finished += OnFinished;
    }

    public string Title
    {
      get
      {
        return AxoCover.Resources.ReportGenerator;
      }
    }

    public event EventHandler<EventArgs<bool?>> ClosingDialog;

    public void OnClosing()
    {
      ViewModel.Abort();
    }

    private void OnFinished(object sender, System.EventArgs e)
    {
      if (!ViewModel.IsFailed)
      {
        ClosingDialog?.Invoke(this, new EventArgs<bool?>(true));
      }
    }

    private void OnOkButtonClick(object sender, System.Windows.RoutedEventArgs e)
    {
      ClosingDialog?.Invoke(this, new EventArgs<bool?>(true));
    }
  }
}
