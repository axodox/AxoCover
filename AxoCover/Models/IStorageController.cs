using AxoCover.Models.Data;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public interface IStorageController
  {
    string CreateTestRunDirectory();
    string CreateReportDirectory();
    string[] GetOutputDirectories();
    Task<OutputDescription> GetOutputFilesAsync(string directory);
    Task CleanOutputAsync(OutputDescription testOutput);
  }
}