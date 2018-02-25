using AxoCover.Models.Testing.Data;
using System.Threading.Tasks;

namespace AxoCover.Models.Storage
{
  public interface IStorageController
  {
    string AxoCoverRoot { get; }
    string CreateTestRunDirectory();
    string CreateReportDirectory();
    string[] GetOutputDirectories();
    Task<OutputDescription> GetOutputFilesAsync(string directory);
    Task CleanOutputAsync(OutputDescription testOutput);
  }
}
