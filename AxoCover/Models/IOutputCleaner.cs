using System.Threading.Tasks;
using AxoCover.Models.Data;

namespace AxoCover.Models
{
  public interface IOutputCleaner
  {
    Task CleanOutputAsync(TestOutputDescription testOutput);
    Task<TestOutputDescription> GetOutputFilesAsync(TestProject testProject);
  }
}