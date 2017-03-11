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
    void RunTestsAsync(IEnumerable<TestCase> testCases, TestExecutionOptions options);

    [OperationContract(IsTerminating = true)]
    void Shutdown();
  }
}
