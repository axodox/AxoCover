using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System.ServiceModel;

namespace AxoCover.Common.Runner
{
  [ServiceContract]
  public interface ITestDiscoveryMonitor
  {
    [OperationContract(IsOneWay = true)]
    void SendMessage(TestMessageLevel testMessageLevel, string message);
  }
}
