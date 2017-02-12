using System.ServiceModel;

namespace AxoCover.Common.Runner
{
  public interface ITestAdapterService
  {
    [OperationContract(IsInitiating = true)]
    void Initialize();

    [OperationContract]
    string[] TryLoadAdaptersFromAssembly(string filePath);

    [OperationContract(IsTerminating = true)]
    void Shutdown();
  }
}