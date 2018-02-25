using AxoCover.Models.Testing.Data;
using EnvDTE;
using System;
using System.Threading.Tasks;

namespace AxoCover.Models.Testing.Discovery
{
  public interface ITestProvider
  {
    event EventHandler ScanningStarted;

    event EventHandler ScanningFinished;

    Task<TestSolution> GetTestSolutionAsync(Solution solution, string testSettings);

    bool IsActive { get; }
  }
}
