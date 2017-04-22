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
    
    public Release[] Releases { get; private set; }

    public bool IsUpdateAvailable { get; private set; }

    public Version UpdateVersion { get; private set; }

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

    public ICommand InstallUpdateCommand
    {
      get
      {
        return new DelegateCommand(
          p => UpdateNow(),
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

    private async void UpdateNow()
    {
      if (IsUpdating) return;

      IsUpdating = true;
      var isSuccessful = false;

      var targetRelease = await _releaseManager.GetTargetRelease();
      if(targetRelease != null)
      {
        isSuccessful = await _releaseManager.TryInstallRelease(targetRelease);
      }
      IsUpdating = false;
      IsSuccessful = isSuccessful;
    }

    public ReleaseManagerViewModel(IReleaseManager releaseManager, IEditorContext editorContext)
    {
      _releaseManager = releaseManager;
      _editorContext = editorContext;
      Settings.Default.SettingChanging += (o,e)=> NotifyPropertyChanged(e.SettingName);
      
      Releases = _releaseManager.Releases;
      Refresh();      
    }

    private async void Refresh()
    {
      var releases = await _releaseManager.GetReleases();
      Releases = releases
        .GroupBy(p => p.Branch)
        .Select(p => p.OrderBy(q => q.CreatedAt).Last())
        .Where(p => p.MergedTo == null)
        .OrderBy(p => p.Branch)
        .ToArray();
      NotifyPropertyChanged(nameof(Releases));
      await CheckForUpdates();
    }

    private async Task CheckForUpdates()
    {
      var targetRelease = await _releaseManager.GetTargetRelease();
      if (targetRelease != null)
      {
        IsUpdateAvailable = targetRelease.Version > _releaseManager.CurrentVersion;
        UpdateVersion = targetRelease.Version;
      }
      else
      {
        IsUpdateAvailable = false;
        UpdateVersion = _releaseManager.CurrentVersion;
      }
      NotifyPropertyChanged(nameof(IsUpdateAvailable));
      NotifyPropertyChanged(nameof(UpdateVersion));
    }
  }
}
