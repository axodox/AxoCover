using System;
using System.Threading.Tasks;

namespace AxoCover.Models.Updates
{
  public interface IReleaseManager
  {
    string DefaultBranch { get; }
    Version CurrentVersion { get; }
    bool IsUpdatingAutomatically { get; set; }
    DateTime LastUpdateCheckTime { get; }
    Version[] PreviousVersions { get; }
    Release[] Releases { get; }
    string TargetBranch { get; set; }

    Task<Release[]> GetReleases(bool isCaching = true);
    Task<Release> GetTargetRelease(bool isCaching = true);
    Task<bool> TryInstallRelease(Release release);
  }
}
