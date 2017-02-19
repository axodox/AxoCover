using AxoCover.Common.Settings;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Collections.Generic;
using System.ServiceModel;

namespace AxoCover.Common.Runner
{
  [ServiceContract(
    SessionMode = SessionMode.Required,
    CallbackContract = typeof(ITestExecutionMonitor))]
  public interface ITestExecutionService
  {
    [OperationContract(IsInitiating = true)]
    void Initialize();

    [OperationContract]
    string[] TryLoadAdaptersFromAssembly(string filePath);

    [OperationContract]
    bool WaitForDebugger(int timeout);

    [OperationContract(IsTerminating = true)]
    void Shutdown();

    [OperationContract]
    void RunTests(IEnumerable<TestCase> testCases, string runSettingsPath, TestApartmentState apartmentState);
  }
}
