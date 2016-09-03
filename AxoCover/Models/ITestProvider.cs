using AxoCover.Models.Data;
using EnvDTE;

namespace AxoCover.Models
{
  public interface ITestProvider
  {
    TestSolution GetTestSolution(Solution solution);
  }
}