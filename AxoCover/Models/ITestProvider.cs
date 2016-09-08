using AxoCover.Models.Data;
using EnvDTE;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public interface ITestProvider
  {
    Task<TestSolution> GetTestSolutionAsync(Solution solution);
  }
}