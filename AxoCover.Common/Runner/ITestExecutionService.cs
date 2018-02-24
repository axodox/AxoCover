using System.ServiceModel;

namespace AxoCover.Common.Runner
{
  [ServiceContract(
    SessionMode = SessionMode.Required,
    CallbackContract = typeof(ITestExecutionMonitor))]
  public interface ITestExecutionService : ITestService
  {
    [OperationContract]
    void RunTests(TestExecutionTask[] executionTasks, TestExecutionOptions options);
  }
}
