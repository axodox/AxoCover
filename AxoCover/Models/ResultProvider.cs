using AxoCover.Models.Data;
using AxoCover.Models.Events;
using System;
using System.Linq;

namespace AxoCover.Models
{
  public class ResultProvider : IResultProvider
  {
    private readonly ITestRunner _testRunner;

    private Data.TestReport.TestRun _report;

    public event EventHandler ResultsUpdated;

    public ResultProvider(ITestRunner testRunner)
    {
      _testRunner = testRunner;
      _testRunner.TestsFinished += OnTestsFinished;
    }

    private void OnTestsFinished(object sender, TestFinishedEventArgs e)
    {
      _report = e.TestReport;
      ResultsUpdated?.Invoke(this, EventArgs.Empty);
    }

    public TestResult GetTestResult(TestMethod testMethod)
    {
      if (_report == null)
        return null;

      var testProject = testMethod.GetParent<TestProject>();
      var testClass = testMethod.GetParent<TestClass>();

      var className = testClass.FullName + ",";
      var testId = _report.TestDefinitions
        .FirstOrDefault(p =>
          StringComparer.OrdinalIgnoreCase.Equals(p.Storage, testProject.OutputFilePath) &&
          p.TestMethod.ClassName.StartsWith(className) &&
          p.TestMethod.MethodName == testMethod.Name)?
        .Id;
      if (testId == null)
        return null;

      var result = _report.Results
        .FirstOrDefault(p => p.TestId == testId);
      if (testId == null)
        return null;

      var errorInfo = result.Items?
        .OfType<Data.TestReport.Output>()
        .Where(p => p.ErrorInfo != null)
        .FirstOrDefault()?
        .ErrorInfo;

      return new TestResult()
      {
        Duration = result.Duration,
        ErrorMessage = errorInfo?.Message,
        StackTrace = StackItem.FromStackTrace(errorInfo?.StackTrace).ToArray()
      };
    }
  }
}
