using AxoCover.Models.Data;
using AxoCover.Models.Data.CoverageReport;
using AxoCover.Models.Events;
using AxoCover.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    public async Task<FileCoverage> GetFileCoverageAsync(string filePath)
    {
      return await Task.Run(() => GetFileCoverage(filePath));
    }

    private FileCoverage GetFileCoverage(string filePath)
    {
      if (_coverageReport != null)
      {
        foreach (var module in _coverageReport.Modules)
        {
          var file = module.Files
            .FirstOrDefault(p => StringComparer.OrdinalIgnoreCase.Equals(p.FullPath, filePath));

          if (file == null)
            continue;

          var methods = module.Classes
            .SelectMany(p => p.Methods)
            .Where(p => p.FileRef != null && p.FileRef.Id == file.Id)
            .ToArray();

          var sequenceGroups = methods
            .SelectMany(p => p.SequencePoints)
            .SelectMany(p => Enumerable
              .Range(p.StartLine, p.EndLine - p.StartLine + 1)
              .Select(q => new
              {
                LineNumber = q,
                VisitCount = p.VisitCount,
                Start = q == p.StartLine ? p.StartColumn : -1,
                End = q == p.EndLine ? p.EndColumn : -1
              }))
            .GroupBy(p => p.LineNumber)
            .ToDictionary(p => p.Key - 1);

          var branchGroups = methods
            .SelectMany(p => p.BranchPoints)
            .GroupBy(p => p.StartLine)
            .ToDictionary(p => p.Key - 1);

          var affectedLines = sequenceGroups
            .Select(p => p.Key)
            .Concat(branchGroups.Select(p => p.Key))
            .Distinct();

          var lineCoverages = new Dictionary<int, LineCoverage>();
          foreach (var affectedLine in affectedLines)
          {
            var sequenceGroup = sequenceGroups.TryGetValue(affectedLine);
            var branchGroup = branchGroups.TryGetValue(affectedLine);

            var visitCount = sequenceGroup.Max(p => p.VisitCount);
            var sequenceState =
              sequenceGroup.All(p => p.VisitCount > 0) ? CoverageState.Covered :
              (sequenceGroup.All(p => p.VisitCount == 0) ? CoverageState.Uncovered :
              CoverageState.Mixed);
            var unvisitedSections = sequenceGroup
              .Where(p => p.VisitCount == 0)
              .Select(p => new LineSection(p.Start - 1, p.End - 1))
              .ToArray();

            var branchesVisited = branchGroup?
              .GroupBy(p => p.Offset)
              .Select(p => p
                .OrderBy(q => q.Path)
                .Select(q => q.VisitCount > 0)
                .ToArray())
              .ToArray() ?? new bool[0][];
            var branchPoints = branchesVisited.SelectMany(p => p).ToArray();
            var branchState =
              branchPoints.All(p => p) ? CoverageState.Covered :
              (branchPoints.All(p => !p) ? CoverageState.Uncovered :
              CoverageState.Mixed);

            var lineCoverage = new LineCoverage(visitCount, sequenceState, branchState, branchesVisited, unvisitedSections);
            lineCoverages.Add(affectedLine, lineCoverage);
          }

          return new FileCoverage(lineCoverages);
        }
      }

      return FileCoverage.Empty;
    }
  }
}
