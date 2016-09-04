using AxoCover.Models.Data;
using AxoCover.Models.Data.CoverageReport;
using AxoCover.Models.Events;
using System;
using System.Linq;

namespace AxoCover.Models
{
  public class CoverageProvider : ICoverageProvider
  {
    public event EventHandler CoverageUpdated;

    private readonly ITestRunner _testRunner;

    private CoverageSession _coverageReport;

    public CoverageProvider(ITestRunner testRunner)
    {
      _testRunner = testRunner;
      _testRunner.TestsFinished += OnTestsFinished;
    }

    private void OnTestsFinished(object sender, TestFinishedEventArgs e)
    {
      _coverageReport = e.Report;
      CoverageUpdated?.Invoke(this, EventArgs.Empty);
    }

    public FileCoverage GetFileCoverage(string filePath)
    {
      if (_coverageReport != null)
      {
        foreach (var module in _coverageReport.Modules)
        {
          var file = module.Files
            .FirstOrDefault(p => StringComparer.OrdinalIgnoreCase.Equals(p.FullPath, filePath));

          if (file == null)
            continue;

          var lineCoverage = module.Classes
            .SelectMany(p => p.Methods)
            .Where(p => p.FileRef != null && p.FileRef.Id == file.Id)
            .SelectMany(p => p.SequencePoints)
            .SelectMany(p => Enumerable
              .Range(p.StartLine, p.EndLine - p.StartLine + 1)
              .Select(q => new { LineNumber = q, VisitCount = p.VisitCount }))
            .GroupBy(p => p.LineNumber)
            .ToDictionary(p => p.Key - 1, p => new LineCoverage(
              p.Max(q => q.VisitCount),
              p.All(q => q.VisitCount > 0) ? CoverageState.Covered : (p.All(q => q.VisitCount == 0) ? CoverageState.Uncovered : CoverageState.Mixed)));

          return new FileCoverage(lineCoverage);
        }
      }

      return FileCoverage.Empty;
    }
  }
}
