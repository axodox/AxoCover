using System.Collections.Generic;

namespace AxoCover.Models.Testing.Adapters
{
  public interface ITestAdapterRepository
  {
    IDictionary<string, ITestAdapter> Adapters { get; }
  }
}
