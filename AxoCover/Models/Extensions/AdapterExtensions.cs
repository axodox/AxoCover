using AxoCover.Models.Data;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System.IO;
using System.Linq;
using System.Reflection;

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

    public static string GetTestPlatformPath()
    {
      var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
      if (dte != null)
      {
        return Path.Combine(Path.GetDirectoryName(dte.FullName),
          @"CommonExtensions\Microsoft\TestWindow\Microsoft.VisualStudio.TestPlatform.ObjectModel.dll");
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
  }
}
