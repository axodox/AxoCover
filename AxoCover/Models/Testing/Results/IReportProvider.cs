using AxoCover.Models.Events;
using System.Threading.Tasks;

namespace AxoCover.Models.Testing.Results
{
  public interface IReportProvider
  {
    bool IsBusy { get; }

    event LogAddedEventHandler LogAdded;

    Task AbortReportGenerationAsync();

    Task<string> GenerateReportAsync(string coverageFile);
  }
}
