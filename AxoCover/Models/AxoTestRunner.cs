using AxoCover.Models.Data;
using AxoCover.Models.Data.CoverageReport;
using AxoCover.Models.Data.TestReport;
using AxoCover.Models.Extensions;
using System;
using System.IO;
using System.Linq;

namespace AxoCover.Models
{
  public class AxoTestRunner : TestRunner
  {
    private ExecutionProcess _executionProcess;

    protected override void RunTests(TestItem testItem, string testSettings)
    {
      CoverageSession coverageReport = null;
      TestRun testReport = null;
      try
      {
        var testCases = testItem
          .Flatten(p => p.Children)
          .OfType<Data.TestMethod>()
          .Select(p => p.Case)
          .ToArray();
        var solution = testItem.GetParent<TestSolution>();
        var outputToProjectMapping = solution
          .Children
          .OfType<TestProject>()
          .ToDictionary(p => p.OutputFilePath, p => p.Name, StringComparer.OrdinalIgnoreCase);

        var coverageReportPath = Path.GetTempFileName();

        var openCoverProcessInfo = new OpenCoverProcessInfo(solution.CodeAssemblies, solution.TestAssemblies, coverageReportPath);

        _executionProcess = ExecutionProcess.Create(openCoverProcessInfo);
        _executionProcess.MessageReceived += (o, e) => OnTestLogAdded(e.Value);
        _executionProcess.TestResult += (o, e) => OnTestExecuted(outputToProjectMapping[e.Value.TestCase.Source] + "." + e.Value.TestCase.FullyQualifiedName, e.Value.Outcome.ToTestState());

        _executionProcess.RunTests(testCases, null);
        _executionProcess.Shutdown();

        if (_isAborting) return;

        if (System.IO.File.Exists(coverageReportPath))
        {
          coverageReport = GenericExtensions.ParseXml<CoverageSession>(coverageReportPath);
        }
      }
      finally
      {
        if (_executionProcess != null)
        {
          _executionProcess.Dispose();
          _executionProcess = null;
        }
        OnTestsFinished(coverageReport, testReport);
      }
    }

    protected override void AbortTests()
    {
      _executionProcess.Shutdown();
    }
  }
}
