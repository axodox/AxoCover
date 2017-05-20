using AxoCover.Common.Models;
using System.Collections.Generic;
using System.ServiceModel;

namespace AxoCover.Common.Runner
{
  [ServiceContract(
    SessionMode = SessionMode.Required,
    CallbackContract = typeof(ITestExecutionMonitor))]
  public interface ITestExecutionService : ITestService
  {
    [OperationContract]
    void RunTests(IEnumerable<TestCase> testCases, TestExecutionOptions options);
  }
}
