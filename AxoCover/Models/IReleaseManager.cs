using AxoCover.Models.Data;
using System;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public interface IReleaseManager
  {
    Version CurrentVersion { get; }
    bool IsUpdatingAutomatically { get; set; }
    DateTime LastUpdateCheckTime { get; }
    Version[] PreviousVersions { get; }
    Release[] ReleaseList { get; }
    string TargetBranch { get; set; }

    Task<Release[]> GetReleases(bool isCaching = true);
    Task<Release> GetTargetRelease(bool isCaching = true);
    Task<bool> TryInstallRelease(Release release);
  }
}