using AxoCover.Models.Testing.Data;
using System;
using System.Threading.Tasks;

namespace AxoCover.Models.Testing.Results
{
  public interface ICoverageProvider
  {
    event EventHandler CoverageUpdated;

    void OpenCoverageReport(string reportPath);

    Task<FileCoverage> GetFileCoverageAsync(string filePath);

    Task<CoverageItem> GetCoverageAsync();
  }
}
