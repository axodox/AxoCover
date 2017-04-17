using AxoCover.Models;

namespace AxoCover.ViewModels
{
  public class ReleaseManagerViewModel : ViewModel
  {
    private readonly IReleaseManager _releaseManager;

    public ReleaseManagerViewModel(IReleaseManager releaseManager)
    {
      _releaseManager = releaseManager;
    }
  }
}
