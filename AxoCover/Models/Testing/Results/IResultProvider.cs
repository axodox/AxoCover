using AxoCover.Models.Testing.Data;
using System;
using System.Threading.Tasks;

namespace AxoCover.Models.Testing.Results
{
  public interface IResultProvider
  {
    event EventHandler ResultsUpdated;

    Task<FileResults> GetFileResultsAsync(string filePath);
  }
}
