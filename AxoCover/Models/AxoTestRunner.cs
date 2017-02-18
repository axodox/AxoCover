using AxoCover.Common.Extensions;
using AxoCover.Models.Data;
using AxoCover.Models.Data.CoverageReport;
using AxoCover.Models.Extensions;
using AxoCover.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AxoCover.Models
{
  public class AxoTestRunner : TestRunner
  {
    private ExecutionProcess _executionProcess;

    protected override void RunTests(TestItem testItem, string testSettings)
    {
      List<TestResult> testResults = new List<TestResult>();
      TestReport testReport = null;
      try
      {
        var testMethods = testItem
          .Flatten(p => p.Children)
          .OfType<Data.TestMethod>()
          .Where(p => p.Case != null)
          .ToArray();
        var testCases = testMethods
          .Select(p => p.Case)
          .ToArray();
        var testMethodsById = testMethods.ToDictionary(p => p.Case.Id);

        var solution = testItem.GetParent<TestSolution>();
        var outputToProjectMapping = solution
          .Children
          .OfType<TestProject>()
          .ToDictionary(p => p.OutputFilePath, p => p.Name, StringComparer.OrdinalIgnoreCase);

        var coverageReportPath = Path.GetTempFileName();

        var openCoverProcessInfo = new OpenCoverProcessInfo(solution.CodeAssemblies, solution.TestAssemblies, coverageReportPath);

        _executionProcess = ExecutionProcess.Create(openCoverProcessInfo, Settings.Default.TestPlatform);
        _executionProcess.MessageReceived += (o, e) => OnTestLogAdded(e.Value);
        _executionProcess.TestStarted += (o, e) => OnTestStarted(testMethodsById[e.Value.Id]);
        _executionProcess.TestResult += (o, e) =>
        {
          var testResult = e.Value.ToTestResult(testMethodsById[e.Value.TestCase.Id]);
          testResults.Add(testResult);
          OnTestExecuted(testResult);
        };

        _executionProcess.RunTests(testCases, testSettings, Settings.Default.TestApartmentState);
        _executionProcess.Shutdown();
        _executionProcess.WaitForExit();

        if (_isAborting) return;

        if (System.IO.File.Exists(coverageReportPath))
        {
          var coverageReport = GenericExtensions.ParseXml<CoverageSession>(coverageReportPath);
          testReport = new TestReport(testResults, coverageReport);
        }
      }
      finally
      {
        if (_executionProcess != null)
        {
          _executionProcess.Dispose();
          _executionProcess = null;
        }

        OnTestsFinished(testReport);
      }
    }

    protected override void AbortTests()
    {
      _executionProcess.Shutdown();
    }
  }
}
