using AxoCover.Common.Runner;
using AxoCover.Runner.Settings;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AxoCover.Runner
{
  public class TestExecutionContext :
    IRunContext,
    IFrameworkHandle
  {
    private static readonly int _outcomeLength = Enum.GetNames(typeof(TestOutcome)).Max(p => p.Length) + 1;
    private ITestExecutionMonitor _monitor;
    private readonly TestExecutionOptions _options;

    public bool EnableShutdownAfterTestRun
    {
      get
      {
        return true;
      }

      set
      {
        throw new NotSupportedException();
      }
    }

    public bool InIsolation
    {
      get
      {
        return false;
      }
    }

    public bool IsBeingDebugged
    {
      get
      {
        return Debugger.IsAttached;
      }
    }

    public bool IsDataCollectionEnabled
    {
      get
      {
        return true;
      }
    }

    public bool KeepAlive
    {
      get
      {
        return false;
      }
    }

    private IRunSettings _runSettings;
    public IRunSettings RunSettings
    {
      get
      {
        return _runSettings;
      }
    }

    public string SolutionDirectory
    {
      get
      {
        return Path.GetDirectoryName(_options.SolutionPath);
      }
    }

    public string TestRunDirectory
    {
      get
      {
        return _options.OutputPath;
      }
    }

    public TestExecutionContext(ITestExecutionMonitor monitor, TestExecutionOptions options)
    {
      _monitor = monitor;
      _options = options;
      _runSettings = new RunSettings(options.RunSettingsPath == null ? null : File.ReadAllText(options.RunSettingsPath));
    }

    class EmptyTestCaseFilterExpression : ITestCaseFilterExpression
    {
      public string TestCaseFilterValue
      {
        get
        {
          return string.Empty;
        }
      }

      public bool MatchTestCase(TestCase testCase, Func<string, object> propertyValueProvider)
      {
        return true;
      }
    }

    public ITestCaseFilterExpression GetTestCaseFilter(IEnumerable<string> supportedProperties, Func<string, TestProperty> propertyProvider)
    {
      return new EmptyTestCaseFilterExpression();
    }

    public int LaunchProcessWithDebuggerAttached(string filePath, string workingDirectory, string arguments, IDictionary<string, string> environmentVariables)
    {
      var processStartInfo = new ProcessStartInfo(filePath, arguments) { WorkingDirectory = workingDirectory };
      foreach (var environmentVariable in environmentVariables)
      {
        processStartInfo.EnvironmentVariables[environmentVariable.Key] = environmentVariable.Value;
      }

      try
      {
        return Process.Start(processStartInfo).Id;
      }
      catch
      {
        return -1;
      }
    }

    public void RecordAttachments(IList<AttachmentSet> attachmentSets)
    {

    }

    public void RecordEnd(TestCase testCase, TestOutcome outcome)
    {
      _monitor.RecordEnd(testCase, outcome);
    }

    public void RecordResult(TestResult testResult)
    {
      _monitor.RecordResult(testResult);
      _monitor.RecordMessage(TestMessageLevel.Informational, testResult.Outcome.ToString().PadRight(_outcomeLength) + testResult.TestCase.FullyQualifiedName);
    }

    public void RecordStart(TestCase testCase)
    {
      _monitor.RecordStart(testCase);
    }

    public void SendMessage(TestMessageLevel testMessageLevel, string message)
    {
      _monitor.RecordMessage(testMessageLevel, message);
    }
  }
}
