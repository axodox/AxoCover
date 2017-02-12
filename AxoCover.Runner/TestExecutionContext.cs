using AxoCover.Common.Runner;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AxoCover.Runner
{
  public class TestExecutionContext :
    IRunContext,
    IFrameworkHandle
  {
    private readonly ITestExecutionMonitor _monitor;

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
      _runSettings = runSettings ?? new RunSettings();
      _testRunDirectory = testRunDirectory ?? (Path.GetTempPath() + "AxoCover-" + Guid.NewGuid());
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
      _monitor.RecordEnd(testCase, outcome);
    }

    public void RecordResult(TestResult testResult)
    {
      _monitor.RecordResult(testResult);
    }

    public void RecordStart(TestCase testCase)
    {
      _monitor.RecordStart(testCase);
    }

    public void SendMessage(TestMessageLevel testMessageLevel, string message)
    {
      _monitor.SendMessage(testMessageLevel, message);
    }
  }
}
