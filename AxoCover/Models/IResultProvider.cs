using AxoCover.Models.Data;
using System;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public interface IResultProvider
  {
    event EventHandler ResultsUpdated;

    TestResult GetTestResult(TestMethod testMethod);

    Task<FileResults> GetFileResultsAsync(string filePath);
  }
}