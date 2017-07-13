using AxoCover.Models;

namespace AxoCover.ViewModels
{
  public class TelemetryIntroductionViewModel : ViewModel
  {
    private readonly IOptions _options;

    public bool IsTelemetryEnabled
    {
      get
      {
        return _options.IsTelemetryEnabled;
      }
      set
      {
        _options.IsTelemetryEnabled = value;
        NotifyPropertyChanged(nameof(IsTelemetryEnabled));
      }
    }

    public TelemetryIntroductionViewModel(IOptions options)
    {
      _options = options;
    }
  }
}
