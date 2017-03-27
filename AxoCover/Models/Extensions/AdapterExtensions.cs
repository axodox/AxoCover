using AxoCover.Models.Data;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AxoCover.Models.Extensions
{
  public static class AdapterExtensions
  {
    public static string[] GetAdapters()
    {
      var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      var adapters = Directory.GetFiles(currentDirectory, "*.TestAdapter.dll", SearchOption.AllDirectories).ToList();

      var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
      if (dte != null)
      {
        var vsTestAdapter = Path.Combine(Path.GetDirectoryName(dte.FullName),
          @"CommonExtensions\Microsoft\TestWindow\Extensions\Microsoft.VisualStudio.TestPlatform.Extensions.VSTestIntegration.dll");
        adapters.Add(vsTestAdapter);
      }

      return adapters.ToArray();
    }

    private static readonly string[] _testPlatformAssemblies = new string[]
    {
      @"CommonExtensions\Microsoft\TestWindow\Microsoft.VisualStudio.TestPlatform.ObjectModel.dll",
      @"CommonExtensions\Microsoft\TestWindow\Microsoft.VisualStudio.QualityTools.UnitTestFramework.dll"
    };

    public static string[] GetTestPlatformPaths()
    {
      var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
      if (dte != null)
      {
        var root = Path.GetDirectoryName(dte.FullName);
        return _testPlatformAssemblies
          .Select(p => Path.Combine(root, p))
          .Where(p => File.Exists(p))
          .ToArray();
      }
      else
      {
        return null;
      }
    }

    public static string GetShortName(this TestMessageLevel testMessageLevel)
    {
      switch (testMessageLevel)
      {
        case TestMessageLevel.Informational:
          return "INFO";
        case TestMessageLevel.Warning:
          return "WARN";
        case TestMessageLevel.Error:
          return "FAIL";
        default:
          return "MISC";
      }
    }

    public static TestState ToTestState(this TestOutcome testOutcome)
    {
      switch (testOutcome)
      {
        case TestOutcome.Failed:
          return TestState.Failed;
        case TestOutcome.Passed:
          return TestState.Passed;
        default:
          return TestState.Inconclusive;
      }
    }

    public static Data.TestResult ToTestResult(this Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult testResult, TestMethod testMethod)
    {
      return new Data.TestResult()
      {
        Method = testMethod,
        Duration = testResult.Duration,
        Outcome = testResult.Outcome.ToTestState(),
        ErrorMessage = GetShortErrorMessage(testResult.ErrorMessage),
        StackTrace = StackItem.FromStackTrace(testResult.ErrorStackTrace)
      };
    }

    private static readonly Regex _exceptionRegex = new Regex("^Test method [^ ]* threw exception:(?<exception>.*)$",
      RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture);

    private static string GetShortErrorMessage(string errorMessage)
    {
      if (errorMessage != null)
      {
        var errorMessageMatch = _exceptionRegex.Match(errorMessage);
        return errorMessageMatch.Success ? errorMessageMatch.Groups["exception"].Value.Trim() : errorMessage;
      }
      else
      {
        return errorMessage;
      }
    }
  }
}
