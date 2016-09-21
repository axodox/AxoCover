using AxoCover.Models.Data;
using EnvDTE;
using System;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public interface ITestProvider
  {
    event EventHandler ScanningStarted;

    event EventHandler ScanningFinished;

    Task<TestSolution> GetTestSolutionAsync(Solution solution);
  }
}