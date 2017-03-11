using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System.ServiceModel;

namespace AxoCover.Common.Runner
{
  [ServiceContract]
  public interface ITestExecutionMonitor
  {
    [OperationContract(IsOneWay = true)]
    void RecordMessage(TestMessageLevel testMessageLevel, string message);

    [OperationContract(IsOneWay = true)]
    void RecordStart(TestCase testCase);

    [OperationContract(IsOneWay = true)]
    void RecordEnd(TestCase testCase, TestOutcome outcome);

    [OperationContract(IsOneWay = true)]
    void RecordResult(TestResult testResult);

    [OperationContract(IsOneWay = true)]
    void RecordFinish();

    [OperationContract(IsOneWay = true)]
    void RecordDebuggerStatus(bool isAttached);
  }
}
