using AxoCover.Models.Data;
using System;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public interface ICoverageProvider
  {
    event EventHandler CoverageUpdated;

    Task<FileCoverage> GetFileCoverageAsync(string filePath);
  }
}