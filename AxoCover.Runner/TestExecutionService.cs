using AxoCover.Common.Extensions;
using AxoCover.Common.Runner;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;

namespace AxoCover.Runner
{
  [ServiceBehavior(
    InstanceContextMode = InstanceContextMode.PerSession,
    ConcurrencyMode = ConcurrencyMode.Multiple,
    AddressFilterMode = AddressFilterMode.Any)]
  public class TestExecutionService : ITestExecutionService
  {
    private ITestExecutionMonitor _monitor;
    private Dictionary<string, ITestExecutor> _testExecutors = new Dictionary<string, ITestExecutor>(StringComparer.OrdinalIgnoreCase);

    public void Initialize()
    {
      _monitor = OperationContext.Current.GetCallbackChannel<ITestExecutionMonitor>();
    }

    public string[] TryLoadAdaptersFromAssembly(string filePath)
    {
      try
      {
        _monitor.SendMessage(TestMessageLevel.Informational, $"Loading assembly from {filePath}...");
        var discovererNames = new List<string>();
        var assembly = Assembly.LoadFrom(filePath);
        var implementers = assembly.FindImplementers<ITestExecutor>();
        foreach (var implementer in implementers)
        {
          try
          {
            var uriAttribute = implementer.GetCustomAttribute<ExtensionUriAttribute>();
            if (uriAttribute == null) continue;

            var testExecutor = Activator.CreateInstance(implementer) as ITestExecutor;
            discovererNames.Add(testExecutor.GetType().FullName);

            _testExecutors[uriAttribute.ExtensionUri] = testExecutor;
          }
          catch (Exception e)
          {
            _monitor.SendMessage(TestMessageLevel.Warning, $"Could not instantiate executor {implementer.FullName}.\r\n{e.GetDescription()}");
          }
        }

        _monitor.SendMessage(TestMessageLevel.Informational, $"Assembly loaded.");
        return discovererNames.ToArray();
      }
      catch (Exception e)
      {
        _monitor.SendMessage(TestMessageLevel.Error, $"Could not load assembly {filePath}.\r\n{e.GetDescription()}");
        return new string[0];
      }
    }

    public void RunTests(IEnumerable<TestCase> testCases, string runSettingsPath)
    {
      _monitor.SendMessage(TestMessageLevel.Informational, $"Executing tests...");
      if (runSettingsPath != null)
      {
        _monitor.SendMessage(TestMessageLevel.Informational, $"Using run settings  {runSettingsPath}.");
      }

      try
      {
        var runSettings = new RunSettings(runSettingsPath == null ? null : File.ReadAllText(runSettingsPath));
        var context = new TestExecutionContext(_monitor, runSettings);

        var testCaseGroups = testCases.GroupBy(p => p.ExecutorUri.ToString());
        foreach (var testCaseGroup in testCaseGroups)
        {
          ITestExecutor testExecutor;

          if (_testExecutors.TryGetValue(testCaseGroup.Key.TrimEnd('/'), out testExecutor))
          {
            _monitor.SendMessage(TestMessageLevel.Informational, $"Running executor: {testCaseGroup.Key}.");

            testExecutor.RunTests(testCases, context, context);
          }
          else
          {
            foreach (var testCase in testCases)
            {
              var testResult = new TestResult(testCase)
              {
                ErrorMessage = "Test executor is not loaded.",
                Outcome = TestOutcome.Skipped
              };
              _monitor.RecordResult(testResult);
            }
          }
        }
        _monitor.SendMessage(TestMessageLevel.Informational, $"Test execution finished.");
      }
      catch (Exception e)
      {
        _monitor.SendMessage(TestMessageLevel.Error, $"Could not execute tests.\r\n{e.GetDescription()}");
      }
    }

    public void Shutdown()
    {
      Environment.Exit(0);
    }
  }
}
