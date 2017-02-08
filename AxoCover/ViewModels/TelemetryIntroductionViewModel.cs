using AxoCover.Properties;

namespace AxoCover.ViewModels
{
  public class TelemetryIntroductionViewModel : ViewModel
  {
    private bool _isTelemetryEnabled = Settings.Default.IsTelemetryEnabled;
    public bool IsTelemetryEnabled
    {
      get
      {
        return _isTelemetryEnabled;
      }
      set
      {
        _isTelemetryEnabled = value;
        Settings.Default.IsTelemetryEnabled = value;
        NotifyPropertyChanged(nameof(IsTelemetryEnabled));
      }
    }
  }
}
