using AxoCover.Models.Data;
using AxoCover.Models.Data.CoverageReport;
using AxoCover.Models.Data.TestReport;
using AxoCover.Models.Extensions;
using System;
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
        var project = testItem.GetParent<TestProject>();

        if (project != null)
        {
          var testContainerPath = project.OutputFilePath;
          var testOutputPath = project.OutputDirectory;
          var testRunId = Guid.NewGuid().ToString();
          var coverageReportPath = Path.Combine(testOutputPath, testRunId + ".xml");
          var testFilter = testItem is TestProject ? null : testItem.FullName;
          var arguments = GetRunnerArguments(_testRunnerPath, testContainerPath, testFilter, coverageReportPath, testSettings);

          var testMethods = testItem
            .Flatten(p => p.Children)
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

          var methodIndex = 0;
          var runnerStartInfo = new ProcessStartInfo(_runnerPath, arguments)
          {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = testOutputPath
          };

          string testResultsPath = null;
          _testProcess = Process.Start(runnerStartInfo);
          while (true)
          {
            string text;
            try
            {
              text = _testProcess.StandardOutput.ReadLine();
            }
            catch
            {
              break;
            }

            if (text == null)
              break;

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
                var path = project.Name + "." + testMethods[methodIndex].FullName;
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
          }

          if (_isAborting) return;

          if (System.IO.File.Exists(testResultsPath))
          {
            testReport = GenericExtensions.ParseXml<TestRun>(testResultsPath);

            if (System.IO.File.Exists(coverageReportPath))
            {
              coverageReport = GenericExtensions.ParseXml<CoverageSession>(coverageReportPath);
              System.IO.File.Move(coverageReportPath, Path.ChangeExtension(testResultsPath, ".xml"));
            }
          }
        }
      }
      finally
      {
        _testProcess.Dispose();
        _testProcess = null;
        OnTestsFinished(coverageReport, testReport);
      }
    }

    private string GetRunnerArguments(string testRunnerPath, string testContainerPath, string testFilter, string coverageReportPath, string testSettings)
    {
      return $"-register:user -target:\"{testRunnerPath}\" -targetargs:\"\\\"{testContainerPath}\\\" " +
        (testFilter == null ? "" : $"/tests:{testFilter} ") + (testSettings == null ? "" : $"/settings:\\\"{testSettings}\\\" ") +
        $"/Logger:trx\" -mergebyhash -output:\"{coverageReportPath}\"";
    }

    protected override void AbortTests()
    {
      if (_testProcess != null)
      {
        _testProcess.Close();
      }
    }
  }
}
