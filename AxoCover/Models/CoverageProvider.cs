using AxoCover.Models.Data;
using AxoCover.Models.Data.CoverageReport;
using AxoCover.Models.Events;
using AxoCover.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public class CoverageProvider : ICoverageProvider
  {
    public event EventHandler CoverageUpdated;

    private readonly ITestRunner _testRunner;

    private CoverageSession _report;

    private readonly Regex _methodNameRegex = new Regex("^(?<returnType>[^ ]*) [^:]*::(?<methodName>[^\\(]*)\\((?<argumentList>[^\\)]*)\\)$", RegexOptions.Compiled);

    public CoverageProvider(ITestRunner testRunner)
    {
      _testRunner = testRunner;
      _testRunner.TestsFinished += OnTestsFinished;
    }

    private void OnTestsFinished(object sender, TestFinishedEventArgs e)
    {
      _report = e.CoverageReport;
      CoverageUpdated?.Invoke(this, EventArgs.Empty);
    }

    public async Task<FileCoverage> GetFileCoverageAsync(string filePath)
    {
      if (filePath == null)
        throw new ArgumentNullException(nameof(filePath));

      return await Task.Run(() => GetFileCoverage(filePath));
    }

    private FileCoverage GetFileCoverage(string filePath)
    {
      if (_report != null)
      {
        foreach (var module in _report.Modules)
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

    public async Task<ResultItem> GetCoverageAsync()
    {
      return await Task.Run(() => GetCoverage());
    }

    private ResultItem GetCoverage()
    {
      if (_report == null)
        return null;

      var solutionResult = new ResultItem(null, CodeItemKind.Solution, null, _report.Summary);
      foreach (var moduleReport in _report.Modules)
      {
        if (!moduleReport.Classes.Any())
          continue;

        var projectResult = new ResultItem(solutionResult, CodeItemKind.Project, moduleReport.ModuleName, moduleReport.Summary);
        var results = new Dictionary<string, ResultItem>()
        {
          { "", projectResult }
        };

        foreach (var classReport in moduleReport.Classes)
        {
          var classResult = AddResultItem(results, CodeItemKind.Class, classReport.FullName);

          foreach (var methodReport in classReport.Methods)
          {
            var methodNameMatch = _methodNameRegex.Match(methodReport.Name);
            if (!methodNameMatch.Success) continue;

            var returnType = methodNameMatch.Groups["returnType"].Value;
            var methodName = methodNameMatch.Groups["methodName"].Value;
            var argumentList = methodNameMatch.Groups["argumentList"].Value;

            var name = $"{methodName}({argumentList}) : {returnType}";
            new ResultItem(classResult, CodeItemKind.Method, name);
          }
        }
      }

      return solutionResult;
    }

    private ResultItem AddResultItem(Dictionary<string, ResultItem> items, CodeItemKind itemKind, string itemPath)
    {
      var nameParts = itemPath.Split('.');
      var parentName = string.Join(".", nameParts.Take(nameParts.Length - 1));
      var itemName = nameParts[nameParts.Length - 1];

      ResultItem parent;
      if (!items.TryGetValue(parentName, out parent))
      {
        if (itemKind == CodeItemKind.Method)
        {
          parent = AddResultItem(items, CodeItemKind.Class, parentName);
        }
        else
        {
          parent = AddResultItem(items, CodeItemKind.Namespace, parentName);
        }
      }

      var item = new ResultItem(parent, itemKind, itemName);
      items.Add(itemPath, item);
      return item;
    }
  }
}
