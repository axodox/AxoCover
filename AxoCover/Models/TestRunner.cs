using AxoCover.Models.Data;
using AxoCover.Models.Data.CoverageReport;
using AxoCover.Models.Data.TestReport;
using AxoCover.Models.Events;
using AxoCover.Models.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AxoCover.Models
{
  public class TestRunner : ITestRunner
  {
    public event EventHandler TestsStarted;
    public event TestExecutedEventHandler TestExecuted;
    public event TestLogAddedEventHandler TestLogAdded;
    public event TestFinishedEventHandler TestsFinished;
    public event EventHandler TestsFailed;

    private const string _runnerName = "Runner\\OpenCover.Console.exe";
    private readonly static string _runnerPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _runnerName);
    private readonly Regex _testOutcomeRegex;
    private readonly Regex _testOutputFileRegex;
    private readonly Dispatcher _dispatcher = Application.Current.Dispatcher;

    private readonly IEditorContext _editorContext;

    public TestRunner(IEditorContext editorContext)
    {
      _editorContext = editorContext;

      _testOutcomeRegex = new Regex(@"^(" + string.Join("|", Enum.GetNames(typeof(TestState))) + @")\s+(.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
      _testOutputFileRegex = new Regex(@"^Results File:\s*(.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    public void RunTestsAsync(TestItem testItem, string testSettings = null)
    {
      TestsStarted?.Invoke(this, EventArgs.Empty);
      Task.Run(() => RunTests(testItem, testSettings));
    }

    private void RunTests(TestItem testItem, string testSettings)
    {
      CoverageSession coverageReport = null;
      TestRun testReport = null;
      try
      {
        var project = testItem.GetParent<TestProject>();

        if (project != null)
        {
          var testRunnerPath = _editorContext.TestRunnerPath;
          var testContainerPath = project.OutputFilePath;
          var testOutputPath = project.OutputDirectory;
          var testRunId = Guid.NewGuid().ToString();
          var coverageReportPath = Path.Combine(testOutputPath, testRunId + ".xml");
          var testFilter = testItem is TestProject ? null : testItem.FullName;
          var arguments = GetRunnerArguments(testRunnerPath, testContainerPath, testFilter, coverageReportPath, testSettings);

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
          var runnerProcess = Process.Start(runnerStartInfo);
          while (true)
          {
            var text = runnerProcess.StandardOutput.ReadLine();

            if (text == null)
              break;

            _dispatcher.BeginInvoke(new Action<string>(OnTestLogAdded), text);

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
                _dispatcher.BeginInvoke(new Action<string, TestState>(OnTestExecuted), path, state);
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

          if (System.IO.File.Exists(coverageReportPath))
          {
            coverageReport = GenericExtensions.ParseXml<CoverageSession>(coverageReportPath);
          }

          if (System.IO.File.Exists(testResultsPath))
          {
            testReport = GenericExtensions.ParseXml<TestRun>(testResultsPath);
          }
        }
      }
      finally
      {
        _dispatcher.BeginInvoke(new Action<CoverageSession, TestRun>(OnTestsFinished), coverageReport, testReport);
      }
    }

    private void OnTestLogAdded(string text)
    {
      TestLogAdded?.Invoke(this, new TestLogAddedEventArgs(text));
    }

    private void OnTestExecuted(string path, TestState outcome)
    {
      TestExecuted?.Invoke(this, new TestExecutedEventArgs(path, outcome));
    }

    private void OnTestsFinished(CoverageSession coverageReport, TestRun testReport)
    {
      if (coverageReport != null && testReport != null)
      {
        TestsFinished?.Invoke(this, new TestFinishedEventArgs(coverageReport, testReport));
      }
      else
      {
        TestsFailed?.Invoke(this, EventArgs.Empty);
      }
    }

    private string GetRunnerArguments(string testRunnerPath, string testContainerPath, string testFilter, string coverageReportPath, string testSettings)
    {
      return $"-register:user -target:\"{testRunnerPath}\" -targetargs:\"\\\"{testContainerPath}\\\" " +
        (testFilter == null ? "" : $"/tests:{testFilter} ") + (testSettings == null ? "" : $"/settings:\\\"{testSettings}\\\" ") +
        $"/Logger:trx\" -mergebyhash -output:\"{coverageReportPath}\"";
    }
  }
}
