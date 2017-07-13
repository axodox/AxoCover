using AxoCover.Common.Models;
using System.ServiceModel;

namespace AxoCover.Common.Runner
{
  [ServiceContract]
  public interface ITestDiscoveryMonitor
  {
    [OperationContract(IsOneWay = true)]
    void RecordMessage(TestMessageLevel testMessageLevel, string message);
  }
}
