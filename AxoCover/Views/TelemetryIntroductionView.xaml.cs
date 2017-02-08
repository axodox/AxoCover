using AxoCover.Models.Events;
using AxoCover.ViewModels;
using System;

namespace AxoCover.Views
{
  /// <summary>
  /// Interaction logic for TelemetryIntroductionView.xaml
  /// </summary>
  public partial class TelemetryIntroductionView : View<TelemetryIntroductionViewModel>, IDialog
  {
    public string Title
    {
      get
      {
        return AxoCoverPackage.Manifest.Name;
      }
    }

    public event EventHandler<ResultEventArgs<bool?>> ClosingDialog;

    public void OnClosing() { }

    public TelemetryIntroductionView()
    {
      InitializeComponent();
    }

    private void OnOkButtonClick(object sender, System.Windows.RoutedEventArgs e)
    {
      ClosingDialog?.Invoke(this, new ResultEventArgs<bool?>(true));
    }
  }
}
