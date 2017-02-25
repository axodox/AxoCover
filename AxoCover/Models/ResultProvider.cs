using AxoCover.Common.Events;
using AxoCover.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public class ResultProvider : IResultProvider
  {
    private readonly ITestRunner _testRunner;

    private Data.TestReport _report;

    public event EventHandler ResultsUpdated;

    public ResultProvider(ITestRunner testRunner)
    {
      _testRunner = testRunner;
      _testRunner.TestsFinished += OnTestsFinished;
    }

    private void OnTestsFinished(object sender, EventArgs<TestReport> e)
    {
      _report = e.Value;
      ResultsUpdated?.Invoke(this, EventArgs.Empty);
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

      foreach (var result in _report.TestResults)
      {
        if (result.ErrorMessage != null && result.StackTrace.Any(p => StringComparer.OrdinalIgnoreCase.Equals(p.SourceFile, filePath)))
        {
          var isPrimary = true;
          foreach (var stackItem in result.StackTrace)
          {
            if (stackItem.HasValidFileReference)
            {
              if(StringComparer.OrdinalIgnoreCase.Equals(stackItem.SourceFile, filePath))
              { 
                var lineResult = new LineResult()
                {
                  TestName = result.Method.FullName,
                  IsPrimary = isPrimary,
                  ErrorMessage = result.ErrorMessage,
                  StackTrace = result.StackTrace
                };

                lineResults.Add(new KeyValuePair<int, LineResult>(stackItem.Line - 1, lineResult));
              }
              isPrimary = false;
            }
          }
        }
      }

      return new FileResults(lineResults
        .GroupBy(p => p.Key)
        .ToDictionary(p => p.Key, p => p.Select(q => q.Value).ToArray()));
    }
  }
}
