using System.ServiceModel;

namespace AxoCover.Common.Runner
{
  [ServiceContract]
  public interface ITestService
  {
    [OperationContract(IsInitiating = true)]
    int Initialize();

    [OperationContract(IsTerminating = true)]
    void Shutdown();
  }
}