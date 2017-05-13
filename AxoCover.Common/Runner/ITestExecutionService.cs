using AxoCover.Common.Models;
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

    [OperationContract]
    void RunTests(IEnumerable<TestCase> testCases, TestExecutionOptions options);

    [OperationContract(IsTerminating = true)]
    void Shutdown();
  }
}
