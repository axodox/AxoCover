using AxoCover.Models.Data;
using AxoCover.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public class ResultProvider : IResultProvider
  {
    private readonly Regex _exceptionRegex = new Regex("^Test method [^ ]* threw exception:(?<exception>.*)$",
      RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture);

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

      var classNameRegex = new Regex("^" + testClass.FullName + "(,.*)?$");
      var testId = _report.TestDefinitions
        .FirstOrDefault(p =>
          StringComparer.OrdinalIgnoreCase.Equals(p.Storage, testProject.OutputFilePath) &&
          classNameRegex.IsMatch(p.TestMethod.ClassName) &&
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
        Outcome = result.Outcome,
        ErrorMessage = GetShortErrorMessage(errorInfo?.Message),
        StackTrace = StackItem.FromStackTrace(errorInfo?.StackTrace).ToArray()
      };
    }

    public async Task<FileResults> GetFileResultsAsync(string filePath)
    {
      if (filePath == null)
        throw new ArgumentNullException(nameof(filePath));

      return await Task.Run(() => GetFileResult(filePath));
    }

    private FileResults GetFileResult(string filePath)
    {
      if (_report == null)
        return FileResults.Empty;

      var lineResults = new List<KeyValuePair<int, LineResult>>();

      foreach (var result in _report.Results)
      {
        if (result.Items == null)
          continue;

        var errors = result.Items
          .OfType<Data.TestReport.Output>()
          .Where(p => p.ErrorInfo?.StackTrace?.Contains(filePath) ?? false)
          .Select(p => p.ErrorInfo)
          .ToArray();

        foreach (var error in errors)
        {
          var stackItems = StackItem.FromStackTrace(error.StackTrace);
          var testName = stackItems.Last().Method.TrimEnd('(', ')');
          var errorMessage = GetShortErrorMessage(error.Message);
          var isPrimary = true;
          foreach (var stackItem in stackItems)
          {
            if (stackItem.HasValidFileReference)
            {
              var lineResult = new LineResult()
              {
                TestName = testName,
                IsPrimary = isPrimary,
                ErrorMessage = errorMessage,
                StackTrace = stackItems
              };

              lineResults.Add(new KeyValuePair<int, LineResult>(stackItem.Line - 1, lineResult));
              isPrimary = false;
            }
          }
        }
      }

      return new FileResults(lineResults
        .GroupBy(p => p.Key)
        .ToDictionary(p => p.Key, p => p.Select(q => q.Value).ToArray()));
    }

    private string GetShortErrorMessage(string errorMessage)
    {
      if (errorMessage != null)
      {
        var errorMessageMatch = _exceptionRegex.Match(errorMessage);
        return errorMessageMatch.Success ? errorMessageMatch.Groups["exception"].Value.Trim() : errorMessage;
      }
      else
      {
        return errorMessage;
      }
    }
  }
}
