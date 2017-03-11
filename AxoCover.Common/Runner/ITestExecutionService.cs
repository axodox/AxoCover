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
    int Initialize();

    [OperationContract(IsOneWay = true)]
    void RunTestsAsync(string[] adapterSources, IEnumerable<TestCase> testCases, string runSettingsPath, TestApartmentState apartmentState);

    [OperationContract(IsTerminating = true)]
    void Shutdown();
  }
}
