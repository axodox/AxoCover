using AxoCover.Common.Events;
using AxoCover.Common.Extensions;
using AxoCover.Models.Editor;
using AxoCover.Models.Extensions;
using AxoCover.Models.Telemetry;
using AxoCover.Models.Testing.Data;
using AxoCover.Models.Testing.Data.CoverageReport;
using AxoCover.Models.Testing.Discovery;
using AxoCover.Models.Testing.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AxoCover.Models.Testing.Results
{
  public class CoverageProvider : ICoverageProvider
  {
    public event EventHandler CoverageUpdated;

    private const string _anonymousGroupName = "Anonymous";
    private readonly ITestRunner _testRunner;
    private readonly ITelemetryManager _telemetryManager;
    private readonly IEditorContext _editorContext;

    private CoverageSession _report;
    private Dictionary<int, TestMethod[]> _trackedMethods = new Dictionary<int, TestMethod[]>();

    private static readonly Regex _methodNameRegex = new Regex("^(?<returnType>[^ ]*) [^:]*::(?<methodName>[^\\(]*)\\((?<argumentList>[^\\)]*)\\)$", RegexOptions.Compiled);
    private readonly Regex _visitorNameRegex = new Regex("^[^ ]* (?<visitorName>[^:]*::[^\\(]*)\\([^\\)]*\\)$", RegexOptions.Compiled);

    public CoverageProvider(ITestProvider testProvider, ITestRunner testRunner, ITelemetryManager telemetryManager, IEditorContext editorContext)
    {
      _testRunner = testRunner;
      _telemetryManager = telemetryManager;
      _testRunner.TestsFinished += OnTestsFinished;
      _editorContext = editorContext;

      _editorContext.SolutionClosing += OnSolutionClosing;
    }

    public void OpenCoverageReport(string reportPath)
    {
      _report = GenericExtensions.ParseXml<CoverageSession>(reportPath);
      CoverageUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void OnSolutionClosing(object sender, EventArgs e)
    {
      _report = null;
      _trackedMethods = new Dictionary<int, TestMethod[]>();
    }

    private void OnTestsFinished(object sender, EventArgs<TestReport> e)
    {
      if (e.Value.CoverageReport != null)
      {
        _report = e.Value.CoverageReport;

        var testMethods = e.Value.TestResults
          .Select(p => p.Method)
          .GroupBy(p => (p.Kind == CodeItemKind.Data ? p.Parent.FullName : p.FullName).CleanPath(true))
          .ToDictionary(p => p.Key, p => p.ToArray());

        _trackedMethods = _report.Modules
          .SelectMany(p => p.TrackedMethods)
          .Select(p => new { Id = p.Id, NameMatch = _visitorNameRegex.Match(p.Name), Name = p.Name })
          .DoIf(p => !p.NameMatch.Success, p => _telemetryManager.UploadExceptionAsync(new Exception("Could not parse tracked method name: " + p.Name)))
          .Where(p => p.NameMatch.Success)
          .ToDictionary(p => p.Id, p => testMethods.TryGetValue(p.NameMatch.Groups["visitorName"].Value.Replace("::", ".")) ?? new TestMethod[0]);

        CoverageUpdated?.Invoke(this, EventArgs.Empty);
      }
    }

    public async Task<FileCoverage> GetFileCoverageAsync(string filePath)
    {
      if (filePath == null)
        throw new ArgumentNullException(nameof(filePath));

      var report = _report;
      var trackedMethods = _trackedMethods;
      return await Task.Run(() => GetFileCoverage(report, trackedMethods, filePath));
    }

    private static FileCoverage GetFileCoverage(CoverageSession report, Dictionary<int, TestMethod[]> trackedMethods, string filePath)
    {
      if (report != null)
      {
        foreach (var module in report.Modules)
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
                LineNumber = q - 1,
                VisitCount = p.VisitCount,
                Start = q == p.StartLine ? p.StartColumn - 1 : 0,
                End = q == p.EndLine ? p.EndColumn - 1 : int.MaxValue,
                Visitors = p.TrackedMethodRefs
              }))
            .GroupBy(p => p.LineNumber)
            .ToDictionary(p => p.Key);

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
              .Select(p => new LineSection(p.Start, p.End))
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

            var lineVisitors = new HashSet<TestMethod>(sequenceGroup
              .SelectMany(p => p.Visitors)
              .Select(p => p.Id)
              .Distinct()
              .Where(p => trackedMethods.ContainsKey(p))
              .SelectMany(p => trackedMethods[p]));

            var branchVisitors = branchGroup?
              .GroupBy(p => p.Offset)
              .Select(p => p
                .OrderBy(q => q.Path)
                .Select(q => new HashSet<TestMethod>(q.TrackedMethodRefs
                  .Where(r => trackedMethods.ContainsKey(r.Id))
                  .SelectMany(r => trackedMethods[r.Id])))
                .ToArray())
              .ToArray() ?? new HashSet<TestMethod>[0][];

            var lineCoverage = new LineCoverage(visitCount, sequenceState, branchState, branchesVisited, unvisitedSections, lineVisitors, branchVisitors);
            lineCoverages.Add(affectedLine, lineCoverage);
          }

          return new FileCoverage(lineCoverages);
        }
      }

      return FileCoverage.Empty;
    }

    public async Task<CoverageItem> GetCoverageAsync()
    {
      var report = _report;
      return await Task.Run(() => GetCoverage(report));
    }

    private static CoverageItem GetCoverage(CoverageSession report)
    {
      if (report == null)
        return null;

      var solutionResult = new CoverageItem(null, Resources.Assemblies, CodeItemKind.Solution);
      foreach (var moduleReport in report.Modules)
      {
        if (!moduleReport.Classes.Any())
          continue;

        var projectResult = new CoverageItem(solutionResult, moduleReport.ModuleName, CodeItemKind.Project);
        var results = new Dictionary<string, CoverageItem>()
        {
          { "", projectResult }
        };

        foreach (var classReport in moduleReport.Classes)
        {
          if (classReport.Methods.Length == 0) continue;
          var classResult = AddResultItem(results, CodeItemKind.Class, 
            PreparePath(classReport.FullName), classReport.Summary ?? new Summary());

          foreach (var methodReport in classReport.Methods)
          {
            if (methodReport.SequencePoints.Length == 0) continue;

            var sourceFile = methodReport.FileRef != null ? moduleReport.Files.Where(p => p.Id == methodReport.FileRef.Id).Select(p => p.FullPath).FirstOrDefault() : null;
            var sourceLine = methodReport.SequencePoints.Select(p => p.StartLine).FirstOrDefault();

            var methodNameMatch = _methodNameRegex.Match(methodReport.Name);
            if (!methodNameMatch.Success) continue;

            var returnType = methodNameMatch.Groups["returnType"].Value;
            var methodName = methodNameMatch.Groups["methodName"].Value;
            var argumentList = methodNameMatch.Groups["argumentList"].Value;

            var name = $"{methodName}({argumentList}) : {returnType}";
            var methodResult = AddResultItem(results, CodeItemKind.Method, 
              PreparePath(classResult.FullName + "." + methodName.Replace(".", "-")), methodReport.Summary ?? new Summary(), name);
            methodResult.SourceFile = sourceFile;
            methodResult.SourceLine = sourceLine;
          }

          var firstSource = classResult.Children
            .Where(p => p.SourceFile != null)
            .OrderBy(p => p.SourceLine)
            .FirstOrDefault();
          if (firstSource != null)
          {
            classResult.SourceFile = firstSource.SourceFile;
            classResult.SourceLine = firstSource.SourceLine;
          }
        }
      }

      return solutionResult;
    }

    private static string PreparePath(string itemPath)
    {
      var nameParts = itemPath.Replace('/', '.').SplitPath(false);
      var result = string.Empty;
      var isInsideAnonymousGroup = false;
      foreach(var namePart in nameParts)
      {
        if(!isInsideAnonymousGroup && namePart.StartsWith("<"))
        {
          result += "." + _anonymousGroupName;
          isInsideAnonymousGroup = true;
        }
        result += "." + namePart;
      }
      return result.TrimStart('.');
    }

    private static CoverageItem AddResultItem(Dictionary<string, CoverageItem> items, CodeItemKind itemKind, string itemPath, Summary summary, string displayName = null)
    {
      var nameParts = itemPath.SplitPath(false);
      var parentName = string.Join(".", nameParts.Take(nameParts.Length - 1)).TrimEnd('.'); //Remove dot for .ctor and .cctor
      var itemName = nameParts[nameParts.Length - 1];
            
      CoverageItem parent;
      if (!items.TryGetValue(parentName, out parent))
      {
        if(parentName.EndsWith("." + _anonymousGroupName) || parentName == _anonymousGroupName)
        {
          parent = AddResultItem(items, CodeItemKind.Group, parentName, new Summary());
        }
        else if (itemKind == CodeItemKind.Method)
        {
          parent = AddResultItem(items, CodeItemKind.Class, parentName, new Summary());
        }
        else
        {
          parent = AddResultItem(items, CodeItemKind.Namespace, parentName, new Summary());
        }
      }

      var item = new CoverageItem(parent, itemName, itemKind, summary, displayName);

      //Methods cannot be a parent so adding them is unnecessary - also overloads would result in key already exists exceptions
      if (itemKind != CodeItemKind.Method)
      {
        items.Add(itemPath, item);
      }
      return item;
    }
  }
}
