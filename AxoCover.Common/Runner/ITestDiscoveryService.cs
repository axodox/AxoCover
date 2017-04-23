using System.Collections.Generic;
using System.ServiceModel;

namespace AxoCover.Common.Runner
{
  [ServiceContract(
    SessionMode = SessionMode.Required,
    CallbackContract = typeof(ITestDiscoveryMonitor))]
  public interface ITestDiscoveryService
  {
    [OperationContract(IsInitiating = true)]
    void Initialize();

    [OperationContract(IsOneWay = true)]
    void DiscoverTestsAsync(string[] adapterSources, IEnumerable<string> testSourcePaths, string runSettingsPath);

    [OperationContract(IsTerminating = true)]
    void Shutdown();
  }
}
