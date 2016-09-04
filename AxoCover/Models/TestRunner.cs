using AxoCover.Models.Data;
using AxoCover.Models.Data.CoverageReport;
using AxoCover.Models.Events;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace AxoCover.Models
{
  public class TestRunner : ITestRunner
  {
    public event EventHandler TestsStarted;
    public event TestExecutedEventHandler TestExecuted;
    public event TestLogAddedEventHandler TestLogAdded;
    public event TestFinishedEventHandler TestsFinished;

    private const string _runnerName = "Runner\\OpenCover.Console.exe";
    private readonly static string _runnerPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _runnerName);
    private readonly Regex _outputRegex;
    private readonly Dispatcher _dispatcher = Application.Current.Dispatcher;

    private readonly IEditorContext _editorContext;

    public TestRunner(IEditorContext editorContext)
    {
      _editorContext = editorContext;

      var statusValues = Enum.GetValues(typeof(TestState)).OfType<TestState>();
      _outputRegex = new Regex(@"^(" + string.Join("|", statusValues) + @")\s+(.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    public void RunTests(TestItem testItem)
    {
      TestsStarted?.Invoke(this, EventArgs.Empty);
      new Thread(Worker).Start(testItem);
    }

    private void Worker(object parameter)
    {
      CoverageSession report = null;
      try
      {
        var testItem = parameter as TestItem;
        var project = testItem.GetParent<TestProject>();

        if (project != null)
        {
          var msTestPath = _editorContext.MsTestPath;
          var testContainerPath = project.OutputFilePath;
          var testOutputPath = Path.GetDirectoryName(testContainerPath);
          var testRunId = Guid.NewGuid().ToString();
          var testResultsPath = Path.Combine(testOutputPath, testRunId + ".trx");
          var coverageReportPath = Path.Combine(testOutputPath, testRunId + ".xml");
          var testFilter = testItem is TestProject ? null : testItem.FullName;
          var arguments = GetRunnerArguments(msTestPath, testContainerPath, testFilter, testResultsPath, coverageReportPath);

          var runnerStartInfo = new ProcessStartInfo(_runnerPath, arguments)
          {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
          };

          var runnerProcess = Process.Start(runnerStartInfo);
          while (true)
          {
            var text = runnerProcess.StandardOutput.ReadLine();

            if (text == null)
              break;

            _dispatcher.BeginInvoke(new Action<string>(OnTestLogAdded), text);

            var match = _outputRegex.Match(text);
            if (match.Success)
            {
              var state = (TestState)Enum.Parse(typeof(TestState), match.Groups[1].Value);
              var path = project.Name + "." + match.Groups[2].Value.Trim();

              _dispatcher.BeginInvoke(new Action<string, TestState>(OnTestExecuted), path, state);
            }
          }

          using (var reportStream = new FileStream(coverageReportPath, FileMode.Open, FileAccess.Read))
          {
            report = new XmlSerializer(typeof(CoverageSession)).Deserialize(reportStream) as CoverageSession;
          }
        }
      }
      finally
      {
        _dispatcher.BeginInvoke(new Action<CoverageSession>(OnTestsFinished), report);
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

    private void OnTestsFinished(CoverageSession report)
    {
      TestsFinished?.Invoke(this, new TestFinishedEventArgs(report));
    }

    private string GetRunnerArguments(string msTestPath, string testContainerPath, string testFilter, string testResultsPath, string coverageReportPath)
    {
      return $"-register:user -target:\"{msTestPath}\" -targetargs:\"/noisolation /testcontainer:\\\"{testContainerPath}\\\" " +
        (testFilter == null ? "" : $"/test:{testFilter} ") + $"/resultsfile:\\\"{testResultsPath}\\\"\" -mergebyhash -output:\"{coverageReportPath}\"";
    }
  }
}
