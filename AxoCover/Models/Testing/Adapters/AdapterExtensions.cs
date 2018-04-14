using AxoCover.Common.Models;
using AxoCover.Common.Settings;
using AxoCover.Models.Testing.Data;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AxoCover.Models.Testing.Adapters
{
  public static class AdapterExtensions
  {
    private static readonly string[] _integratedTestPlatformAssemblies = new string[]
    {
      @"CommonExtensions\Microsoft\TestWindow\msdia110typelib_clr0200.dll",
      @"CommonExtensions\Microsoft\TestWindow\msdia120typelib_clr0200.dll",
      @"CommonExtensions\Microsoft\TestWindow\msdia140typelib_clr0200.dll",
      @"CommonExtensions\Microsoft\TestWindow\Microsoft.VisualStudio.TestPlatform.ObjectModel.dll",
      @"CommonExtensions\Microsoft\TestWindow\Microsoft.VisualStudio.QualityTools.UnitTestFramework.dll"
    };

    private static readonly string[] _standardTestPlatformAssemblies = new string[]
    {
      @"TestPlatform\Microsoft.VisualStudio.TestPlatform.ObjectModel.dll"
    };

    public static string[] GetTestPlatformAssemblyPaths(TestAdapterMode adapterMode)
    {
      string root;
      string[] testPlatformAssemblies;

      switch (adapterMode)
      {
        case TestAdapterMode.Integrated:
          {
            testPlatformAssemblies = _integratedTestPlatformAssemblies;

            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            if (dte != null)
              root = Path.GetDirectoryName(dte.FullName);
            else
              throw new InvalidOperationException();
          }
          break;
        case TestAdapterMode.Standard:
          {
            root = AxoCoverPackage.PackageRoot;
            testPlatformAssemblies = _standardTestPlatformAssemblies;
          }
          break;
        default:
          throw new NotImplementedException();
      }

      return testPlatformAssemblies
        .Select(p => Path.Combine(root, p))
        .Where(p => File.Exists(p))
        .ToArray();
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

    public static Testing.Data.TestResult ToTestResult(this Common.Models.TestResult testResult, TestMethod testMethod, int sessionId)
    {
      return new Testing.Data.TestResult()
      {
        Method = testMethod,
        Duration = testResult.Duration,
        Outcome = testResult.Outcome.ToTestState(),
        ErrorMessage = GetShortErrorMessage(testResult.ErrorMessage),
        StackTrace = StackItem.FromStackTrace(testResult.ErrorStackTrace),
        Output = testResult.Messages?.Length > 0 ? 
          string.Join(Environment.NewLine, testResult.Messages.Select(p => p.Text)).Trim() : null,
        SessionId = sessionId
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
