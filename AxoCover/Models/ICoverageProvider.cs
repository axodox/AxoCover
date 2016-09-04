using AxoCover.Models.Data;
using System;

namespace AxoCover.Models
{
  public interface ICoverageProvider
  {
    event EventHandler CoverageUpdated;

    FileCoverage GetFileCoverage(string filePath);
  }
}