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

    private bool _isUpdateAvailable;
    public bool IsUpdateAvailable
    {
      get { return _isUpdateAvailable; }
      private set
      {
        _isUpdateAvailable = value;
        NotifyPropertyChanged(nameof(IsUpdateAvailable));
      }
    }

    private Release[] _updateReleases;
    public Release[] UpdateReleases
    {
      get { return _updateReleases; }
      private set
      {
        _updateReleases = value;
        NotifyPropertyChanged(nameof(UpdateReleases));
        IsUpdateAvailable = value?.Length > 0;
      }
    }

    private Release _updateRelease;
    public Release UpdateRelease
    {
      get { return _updateRelease; }
      set
      {
        _updateRelease = value;
        NotifyPropertyChanged(nameof(UpdateRelease));

        _releaseManager.TargetBranch = value?.Branch ?? _releaseManager.DefaultBranch;
      }
    }

    private bool _isRollbackAvailable;
    public bool IsRollbackAvailable
    {
      get { return _isRollbackAvailable; }
      private set
      {
        _isRollbackAvailable = value;
        NotifyPropertyChanged(nameof(IsRollbackAvailable));
      }
    }

    private Release[] _rollbackReleases;
    public Release[] RollbackReleases
    {
      get { return _rollbackReleases; }
      private set
      {
        _rollbackReleases = value;
        NotifyPropertyChanged(nameof(RollbackReleases));
        IsRollbackAvailable = value?.Length > 0;
      }
    }

    private Release _rollbackRelease;
    public Release RollbackRelease
    {
      get { return _rollbackRelease; }
      set
      {
        _rollbackRelease = value;
        NotifyPropertyChanged(nameof(RollbackRelease));
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

    private Release _installingRelease;
    public Release InstallingRelease
    {
      get { return _installingRelease; }
      set
      {
        _installingRelease = value;
        NotifyPropertyChanged(nameof(InstallingRelease));
      }
    }

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
          p => Update(UpdateRelease),
          p => !IsUpdating && UpdateRelease != null && UpdateRelease.Version != _releaseManager.CurrentVersion,
          p => ExecuteOnPropertyChange(p, nameof(IsUpdating), nameof(UpdateRelease)));
      }
    }

    public ICommand RetryUpdateCommand
    {
      get
      {
        return new DelegateCommand(
          p => Update(InstallingRelease),
          p => !IsUpdating && InstallingRelease != null,
          p => ExecuteOnPropertyChange(p, nameof(IsUpdating), nameof(InstallingRelease)));
      }
    }

    public ICommand RollbackUpdateCommand
    {
      get
      {
        return new DelegateCommand(
          p => { IsUpdatingAutomatically = false; Update(RollbackRelease); },
          p => !IsUpdating && RollbackRelease != null,
          p => ExecuteOnPropertyChange(p, nameof(IsUpdating), nameof(RollbackRelease)));
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

    public ReleaseManagerViewModel(IReleaseManager releaseManager, IEditorContext editorContext)
    {
      _releaseManager = releaseManager;
      _editorContext = editorContext;
      Settings.Default.PropertyChanged += (o, e) => NotifyPropertyChanged(e.PropertyName);
      
      Refresh();
    }

    private async void Update(Release release)
    {
      if (IsUpdating) return;
      if (release == null)
      {
        throw new ArgumentNullException(nameof(release));
      }

      IsUpdating = true;
      IsSuccessful = null;
      InstallingRelease = release;

      IsSuccessful = await _releaseManager.TryInstallRelease(release);
      IsUpdating = false;
    }

    private async void Refresh(bool isCaching = true)
    {
      var releases = await _releaseManager.GetReleases(isCaching);
      var updateBranch = UpdateRelease?.Branch ?? _releaseManager.TargetBranch ?? _releaseManager.DefaultBranch;
      UpdateReleases = releases
        .GroupBy(p => p.Branch)
        .Select(p => p.OrderBy(q => q.CreatedAt).Last())
        .Where(p => p.MergedTo == null)
        .OrderBy(p => p.Branch)
        .ToArray();
      UpdateRelease = UpdateReleases.FirstOrDefault(p => p.Branch == updateBranch) ?? UpdateReleases.FirstOrDefault();

      var rollbackVersion = RollbackRelease?.Version;
      RollbackReleases = _releaseManager.PreviousVersions
        .Select(p => releases.FirstOrDefault(q => q.Version == p))
        .Where(p => p != null && p.Version != _releaseManager.CurrentVersion)
        .ToArray();
      RollbackRelease = RollbackReleases.FirstOrDefault(p => p.Version == rollbackVersion) ?? RollbackReleases.FirstOrDefault();
    }
  }
}
