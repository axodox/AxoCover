using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxoCover.Models.Adapters
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
