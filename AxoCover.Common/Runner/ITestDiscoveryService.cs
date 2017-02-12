using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Collections.Generic;
using System.ServiceModel;

namespace AxoCover.Common.Runner
{
  [ServiceContract(
    SessionMode = SessionMode.Required,
    CallbackContract = typeof(ITestDiscoveryMonitor))]
  public interface ITestDiscoveryService : ITestAdapterService
  {
    [OperationContract]
    TestCase[] DiscoverTests(IEnumerable<string> testSourcePaths, string runSettingsPath);
  }
}
