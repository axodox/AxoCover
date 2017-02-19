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
using System.ServiceModel;

namespace AxoCover.Runner
{
  public class TestExecutionContext :
    IRunContext,
    IFrameworkHandle
  {
    private static readonly int _outcomeLength = Enum.GetNames(typeof(TestOutcome)).Max(p => p.Length) + 1;

    private ITestExecutionMonitor _monitor;

    public bool EnableShutdownAfterTestRun
    {
      get
      {
        throw new NotImplementedException();
      }

      set
      {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
      }
    }

    private string _testRunDirectory;
    public string TestRunDirectory
    {
      get
      {
        return _testRunDirectory;
      }
    }

    public TestExecutionContext(ITestExecutionMonitor monitor, RunSettings runSettings = null, string testRunDirectory = null)
    {
      _monitor = monitor;
      var monitorObject = monitor as ICommunicationObject;
      monitorObject.Closing += OnMonitorShutdown;
      monitorObject.Faulted += OnMonitorShutdown;
      _runSettings = runSettings ?? new RunSettings();
      _testRunDirectory = testRunDirectory ?? (Path.GetTempPath() + "AxoCover-" + Guid.NewGuid());
    }

    private void OnMonitorShutdown(object sender, EventArgs e)
    {
      _monitor = null;
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
      throw new NotSupportedException();
    }

    public void RecordAttachments(IList<AttachmentSet> attachmentSets)
    {

    }

    public void RecordEnd(TestCase testCase, TestOutcome outcome)
    {
      _monitor?.RecordEnd(testCase, outcome);
    }

    public void RecordResult(TestResult testResult)
    {
      _monitor?.RecordResult(testResult);
      _monitor?.SendMessage(TestMessageLevel.Informational, testResult.Outcome.ToString().PadRight(_outcomeLength) + testResult.TestCase.FullyQualifiedName);
    }

    public void RecordStart(TestCase testCase)
    {
      _monitor?.RecordStart(testCase);
    }

    public void SendMessage(TestMessageLevel testMessageLevel, string message)
    {
      _monitor?.SendMessage(testMessageLevel, message);
    }
  }
}
