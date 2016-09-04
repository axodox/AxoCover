using AxoCover.Models.Data;
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

namespace AxoCover.Models
{
  public class TestRunner : ITestRunner
  {
    public event EventHandler TestsStarted, TestsFinished;

    public event TestExecutedEventHandler TestExecuted;

    private const string _runnerName = "Runner\\OpenCover.Console.exe";
    private readonly static string _runnerPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _runnerName);
    private readonly Regex _outputRegex;
    private readonly Dispatcher _dispatcher = Application.Current.Dispatcher;

    private IEditorContext _editorContext;
    public TestRunner(IEditorContext editorContext)
    {
      _editorContext = editorContext;

      var statusValues = Enum.GetValues(typeof(TestState)).OfType<TestState>();
      _outputRegex = new Regex(@"^(" + string.Join("|", statusValues) + @")\s+(.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    public void RunTests(TestItem testItem)
    {
      var project = testItem.GetParent<TestProject>();

      if (project == null)
        return;

      var msTestPath = _editorContext.MsTestPath;
      var testContainerPath = project.OutputFilePath;
      var testOutputPath = Path.GetDirectoryName(testContainerPath);
      var testRunId = Guid.NewGuid().ToString();
      var testResultsPath = Path.Combine(testOutputPath, testRunId + ".trx");
      var coverageReportPath = Path.Combine(testOutputPath, testRunId + ".xml");
      var arguments = GetRunnerArguments(msTestPath, testContainerPath, testResultsPath, coverageReportPath);

      var runnerStartInfo = new ProcessStartInfo(_runnerPath, arguments)
      {
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };

      TestsStarted?.Invoke(this, EventArgs.Empty);
      new Thread(Worker).Start(new Tuple<string, ProcessStartInfo>(project.Name, runnerStartInfo));
    }

    private void Worker(object parameter)
    {
      var argument = parameter as Tuple<string, ProcessStartInfo>;

      var projectName = argument.Item1;
      var runnerProcess = Process.Start(argument.Item2);

      while (true)
      {
        var text = runnerProcess.StandardOutput.ReadLine();

        if (text == null)
          break;

        var match = _outputRegex.Match(text);
        if (match.Success)
        {
          var state = (TestState)Enum.Parse(typeof(TestState), match.Groups[1].Value);
          var path = projectName + "." + match.Groups[2].Value.Trim();

          _dispatcher.BeginInvoke(new Action<string, TestState>(OnTestExecuted), path, state);
        }
      }

      _dispatcher.BeginInvoke(new Action(OnTestsFinished));
    }

    private void OnTestExecuted(string path, TestState outcome)
    {
      TestExecuted?.Invoke(this, new TestExecutedEventArgs(path, outcome));
    }

    private void OnTestsFinished()
    {
      TestsFinished?.Invoke(this, EventArgs.Empty);
    }

    private string GetRunnerArguments(string msTestPath, string testContainerPath, string testResultsPath, string coverageReportPath)
    {
      return $"-register:user -target:\"{msTestPath}\" -targetargs:\"/noisolation /testcontainer:\\\"{testContainerPath}\\\" /resultsfile:\\\"{testResultsPath}\\\"\" -mergebyhash -output:\"{coverageReportPath}\"";
    }
  }
}
