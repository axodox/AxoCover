using AxoCover.Common.Events;
using System;
using System.Threading.Tasks;

namespace AxoCover.Models.Testing.Results
{
  public interface IReportProvider
  {
    bool IsBusy { get; }

    event EventHandler<EventArgs<string>> LogAdded;

    Task AbortReportGenerationAsync();

    Task<string> GenerateReportAsync(string coverageFile);
  }
}
