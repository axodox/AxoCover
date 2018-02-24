using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Linq;

namespace AxoCover.Models.Testing.Adapters
{
  public class TestAdapterRepository : ITestAdapterRepository
  {
    public IDictionary<string, ITestAdapter> Adapters { get; private set; }

    public TestAdapterRepository(IUnityContainer container)
    {
      Adapters = container
        .ResolveAll<ITestAdapter>()
        .ToDictionary(p => p.Name);
    }
  }
}
