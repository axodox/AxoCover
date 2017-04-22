using AxoCover.Models;
using AxoCover.Models.Data;
using AxoCover.Properties;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AxoCover.ViewModels
{
  public class ReleaseManagerViewModel : ViewModel
  {
    private readonly IReleaseManager _releaseManager;
    private readonly IEditorContext _editorContext;

    public Release[] UpdateReleases { get; private set; }

    public Release[] PreviousReleases { get; private set; }

    public bool IsUpdateAvailable { get; private set; }

    private Release _updateRelease;
    public Release UpdateRelease
    {
      get { return _updateRelease; }
      set
      {
        _updateRelease = value;
        NotifyPropertyChanged(nameof(UpdateRelease));
      }
    }

    public string ReleaseBranch
    {
      get { return Settings.Default.ReleaseBranch; }
      set
      {
        Settings.Default.ReleaseBranch = value;
        NotifyPropertyChanged(nameof(ReleaseBranch));
        CheckForUpdates();
      }
    }

    public bool IsUpdatingAutomatically
    {
      get { return Settings.Default.IsUpdatingAutomatically; }
      set
      {
        Settings.Default.IsUpdatingAutomatically = value;
        NotifyPropertyChanged(nameof(IsUpdatingAutomatically));
      }
    }

    public string BranchDescription { get; private set; }

    public ICommand RefreshCommand
    {
      get
      {
        return new DelegateCommand(p => Refresh(false));
      }
    }

    public ICommand InstallUpdateCommand
    {
      get
      {
        return new DelegateCommand(
          p => Update(),
          p => !IsUpdating && IsUpdateAvailable && ReleaseBranch != null,
          p => ExecuteOnPropertyChange(p, nameof(IsUpdating), nameof(IsUpdateAvailable), nameof(ReleaseBranch)));
      }
    }

    public ICommand RetryUpdateCommand
    {
      get
      {
        return new DelegateCommand(
          p => Update(UpdateRelease),
          p => !IsUpdating,
          p => ExecuteOnPropertyChange(p, nameof(IsUpdating)));
      }
    }

    public ICommand RestartCommand
    {
      get
      {
        return new DelegateCommand(
          p => _editorContext.Restart());
      }
    }

    private bool _isUpdating;
    public bool IsUpdating
    {
      get { return _isUpdating; }
      set
      {
        _isUpdating = value;
        NotifyPropertyChanged(nameof(IsUpdating));
      }
    }

    private bool? _isSuccessful = null;
    public bool? IsSuccessful
    {
      get { return _isSuccessful; }
      set
      {
        _isSuccessful = value;
        NotifyPropertyChanged(nameof(IsSuccessful));
      }
    }

    public DateTime ReleaseListUpdateTime
    {
      get { return Settings.Default.ReleaseListUpdateTime; }
    }

    private async void Update(Release release = null)
    {
      if (IsUpdating) return;

      IsUpdating = true;
      IsSuccessful = null;
      var isSuccessful = false;

      var targetRelease = release ?? await _releaseManager.GetTargetRelease();
      if (targetRelease != null)
      {
        UpdateRelease = targetRelease;
        isSuccessful = await _releaseManager.TryInstallRelease(targetRelease);
      }
      IsUpdating = false;
      IsSuccessful = isSuccessful;
    }

    public ReleaseManagerViewModel(IReleaseManager releaseManager, IEditorContext editorContext)
    {
      _releaseManager = releaseManager;
      _editorContext = editorContext;
      Settings.Default.PropertyChanged += (o, e) => NotifyPropertyChanged(e.PropertyName);

      UpdateReleases = _releaseManager.Releases;
      Refresh();
    }

    private async void Refresh(bool isCaching = true)
    {
      var releases = await _releaseManager.GetReleases(isCaching);
      var releaseBranch = ReleaseBranch;
      UpdateReleases = releases
        .GroupBy(p => p.Branch)
        .Select(p => p.OrderBy(q => q.CreatedAt).Last())
        .Where(p => p.MergedTo == null)
        .OrderBy(p => p.Branch)
        .ToArray();
      NotifyPropertyChanged(nameof(UpdateReleases));
      ReleaseBranch = releaseBranch;

      PreviousReleases = _releaseManager.PreviousVersions
        .Select(p => releases.FirstOrDefault(q => q.Version == p))
        .Where(p => p != null)
        .ToArray();
      NotifyPropertyChanged(nameof(PreviousReleases));
      await CheckForUpdates();
    }

    private async Task CheckForUpdates()
    {
      var targetRelease = await _releaseManager.GetTargetRelease();
      if (targetRelease != null)
      {
        IsUpdateAvailable = targetRelease.Version > _releaseManager.CurrentVersion;
      }
      else
      {
        IsUpdateAvailable = false;
      }
      NotifyPropertyChanged(nameof(IsUpdateAvailable));
    }
  }
}
