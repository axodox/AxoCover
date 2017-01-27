using AxoCover.Models.Data;
using AxoCover.Models.Data.CoverageReport;
using AxoCover.Models.Data.TestReport;
using AxoCover.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AxoCover.Models
{
  public class VsTestRunner : TestRunner
  {
    private readonly Regex _testOutcomeRegex;
    private readonly Regex _testOutputFileRegex;
    private readonly string _testRunnerPath;
    private Process _testProcess;

    public VsTestRunner(IEditorContext editorContext)
    {
      _testRunnerPath = Path.Combine(editorContext.RootPath, @"CommonExtensions\Microsoft\TestWindow\vstest.console.exe");

      _testOutcomeRegex = new Regex(@"^(" + string.Join("|", Enum.GetNames(typeof(TestState))) + @")\s+(.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
      _testOutputFileRegex = new Regex(@"^Results File:\s*(.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    protected override void RunTests(TestItem testItem, string testSettings)
    {
      CoverageSession coverageReport = null;
      TestRun testReport = null;
      try
      {
        var projects = testItem.Kind == CodeItemKind.Solution ?
          testItem.Children.OfType<TestProject>().ToArray() :
          new[] { testItem.GetParent<TestProject>() };

        if (projects.Length > 0)
        {
          var projectMappings = projects
            .Flatten<TestItem>(p => p.Children)
            .OfType<Data.TestMethod>()
            .ToDictionary(p => p.FullName, p => p.GetParent<TestProject>().Name);

          var testContainerPaths = projects.Select(p => p.OutputFilePath);
          var testOutputPath = projects[0].OutputDirectory;
          var testRunId = Guid.NewGuid().ToString();
          var coverageReportPath = Path.Combine(testOutputPath, testRunId + ".xml");
          var testFilter = testItem.Kind == CodeItemKind.Project || testItem.Kind == CodeItemKind.Solution ? null : testItem.FullName;
          var arguments = GetRunnerArguments(_testRunnerPath, testContainerPaths, testFilter, coverageReportPath, testSettings);

          var testMethods = testItem
            .Flatten(p => p.Children, false)
            .OfType<Data.TestMethod>()
            .OrderBy(p => p.Index)
            .ToArray();

          if (testSettings != null)
          {
            testMethods = testMethods
              .Where(p => p.IsIgnored)
              .Concat(testMethods.Where(p => !p.IsIgnored))
              .ToArray();
          }

          _testProcess = new Process()
          {
            StartInfo = new ProcessStartInfo(_runnerPath, arguments)
            {
              RedirectStandardOutput = true,
              UseShellExecute = false,
              CreateNoWindow = true,
              WorkingDirectory = testOutputPath
            }
          };

          var methodIndex = 0;
          string testResultsPath = null;
          _testProcess.OutputDataReceived += (o, e) =>
          {
            if (e.Data == null) return;
            var text = e.Data;

            OnTestLogAdded(text);

            var match = _testOutcomeRegex.Match(text);
            if (match.Success)
            {
              var state = (TestState)Enum.Parse(typeof(TestState), match.Groups[1].Value);
              var name = match.Groups[2].Value;
              while (methodIndex < testMethods.Length && testMethods[methodIndex].Name != name)
              {
                methodIndex++;
              }

              if (methodIndex < testMethods.Length)
              {
                var testName = testMethods[methodIndex].FullName;
                var path = projectMappings[testName] + "." + testName;
                OnTestExecuted(path, state);
                methodIndex++;
              }
              else
              {
                methodIndex = 0;
              }
            }
            else if ((match = _testOutputFileRegex.Match(text)).Success)
            {
              testResultsPath = match.Groups[1].Value;
            }
          };

          _testProcess.Start();
          _testProcess.BeginOutputReadLine();

          while (!_testProcess.HasExited)
          {
            _testProcess.WaitForExit(1000);
          }

          if (_isAborting) return;

          if (System.IO.File.Exists(testResultsPath))
          {
            testReport = GenericExtensions.ParseXml<TestRun>(testResultsPath);
          }

          if (System.IO.File.Exists(coverageReportPath))
          {
            var newCoverageReportPath = Path.ChangeExtension(testResultsPath, ".xml");
            System.IO.File.Move(coverageReportPath, newCoverageReportPath);
            coverageReportPath = newCoverageReportPath;

            coverageReport = GenericExtensions.ParseXml<CoverageSession>(coverageReportPath);
          }
        }
      }
      finally
      {
        if (_testProcess != null)
        {
          _testProcess.Dispose();
          _testProcess = null;
        }
        OnTestsFinished(coverageReport, testReport);
      }
    }

    private string GetRunnerArguments(string testRunnerPath, IEnumerable<string> testContainerPaths, string testFilter, string coverageReportPath, string testSettings)
    {
      return GetSettingsBasedArguments() + $"-register:user -target:\"{testRunnerPath}\" -targetargs:\"{string.Join(" ", testContainerPaths.Select(p => "\\\"" + p + "\\\""))} " +
        (testFilter == null ? "" : $"/tests:{testFilter} ") + (testSettings == null ? "" : $"/settings:\\\"{testSettings}\\\" ") +
        $"/Logger:trx\" -mergebyhash -output:\"{coverageReportPath}\"";
    }

    protected override void AbortTests()
    {
      if (_testProcess != null && !_testProcess.HasExited)
      {
        _testProcess.Kill();
      }
    }
  }
}
