using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.ServiceModel;

namespace AxoCover.Common.Runner
{
  [ServiceContract]
  public interface ITestExecutionMonitor : ITestOperationMonitor
  {
    [OperationContract(IsOneWay = true)]
    void RecordStart(TestCase testCase);

    [OperationContract(IsOneWay = true)]
    void RecordEnd(TestCase testCase, TestOutcome outcome);

    [OperationContract(IsOneWay = true)]
    void RecordResult(TestResult testResult);
  }
}
