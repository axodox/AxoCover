using AxoCover.Models.Data;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace AxoCover.Models
{
  public class TestRunner : ITestRunner
  {
    private const string _runnerName = "Runner\\OpenCover.Console.exe";
    private readonly static string _runnerPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _runnerName);

    private IEditorContext _editorContext;
    public TestRunner(IEditorContext editorContext)
    {
      _editorContext = editorContext;
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

      Process.Start(_runnerPath, arguments);
    }

    private string GetRunnerArguments(string msTestPath, string testContainerPath, string testResultsPath, string coverageReportPath)
    {
      return $"-register:user -target:\"{msTestPath}\" -targetargs:\"/noisolation /testcontainer:\\\"{testContainerPath}\\\" /resultsfile:\\\"{testResultsPath}\\\"\" -mergebyhash -output:\"{coverageReportPath}\"";
    }
  }
}
