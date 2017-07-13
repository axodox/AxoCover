using AxoCover.ViewModels;
using System.Windows;

namespace AxoCover.Views
{
  /// <summary>
  /// Interaction logic for TelemetryIntroductionView.xaml
  /// </summary>
  public partial class TelemetryIntroductionView : View<TelemetryIntroductionViewModel>, IDialog
  {
    private Window _window;

    public TelemetryIntroductionView()
    {
      InitializeComponent();
    }

    private void OnOkButtonClick(object sender, System.Windows.RoutedEventArgs e)
    {
      _window.DialogResult = true;
    }

    public void InitializeWindow(Window window)
    {
      _window = window;
      _window.Title = AxoCoverPackage.Manifest.Name;
      _window.SizeToContent = SizeToContent.Height;
      _window.ResizeMode = ResizeMode.NoResize;
    }
  }
}
