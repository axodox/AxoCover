using System.Collections.Generic;

namespace AxoCover.Models.Adapters
{
  public interface ITestAdapterRepository
  {
    IDictionary<string, ITestAdapter> Adapters { get; }
  }
}