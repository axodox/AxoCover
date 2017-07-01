using AxoCover.Common.Extensions;
using AxoCover.Common.Models;
using AxoCover.Common.Runner;
using AxoCover.Common.Settings;
using AxoCover.Models.Data;
using AxoCover.Models.Extensions;
using EnvDTE;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AxoCover.Models.Adapters
{
  public class xUnitAdapter : ITestAdapter
  {
    public string Name => "xUnit";

    public TestAdapterMode Mode => TestAdapterMode.Standard;

    public string ExecutorUri => "executor://xunit/VsTestRunner2";

    public bool IsTestSource(Project project)
    {
      return project.TryGetReference("xunit.core", out _);
    }

    private Regex _displayNameRegex = new Regex(@"(?>(?<path>[\w.]*))(?>(?<arguments>.+))");
    private Regex _fullyQualifiedNameRegex = new Regex(@"^(?>(?<path>[\w.]*) \(\w+\))$");

    public bool CanProcessCase(TestCase testCase)
    {
      return testCase.ExecutorUri.ToString().Contains("xunit", StringComparison.OrdinalIgnoreCase);
    }

    public void ProcessCase(TestCase testCase, ref CodeItemKind testItemKind, ref string testItemPath, ref string displayName)
    {
      var fullyQualifiedNameMatch = _fullyQualifiedNameRegex.Match(testCase.FullyQualifiedName);
      if (fullyQualifiedNameMatch.Success)
      {
        var displayNameMatch = _displayNameRegex.Match(testCase.DisplayName);
        if (displayNameMatch.Success)
        {
          testItemKind = CodeItemKind.Data;
          displayName = displayNameMatch.Groups["arguments"].Value;
          testItemPath = fullyQualifiedNameMatch.Groups["path"].Value + ".Instance" + testCase.Id.ToString("N");
        }
        else
        {
          testItemPath = fullyQualifiedNameMatch.Groups["path"].Value;
        }
      }
    }

    public TestAdapterOptions GetLoadingOptions()
    {
      return new TestAdapterOptions()
      {
        AssemblyPath = _assemblyPath,
        RedirectedAssemblies = _redirectedAssemblies,
        RedirectionOptions = FileRedirectionOptions.ExcludeNonexistentDirectories | FileRedirectionOptions.ExcludeNonexistentFiles | FileRedirectionOptions.IncludeBaseDirectory,
        ExtensionUri = ExecutorUri
      };
    }

    private readonly string _assemblyPath;
    private readonly string[] _redirectedAssemblies;

    public xUnitAdapter(IEditorContext editorContext)
    {
      _assemblyPath = Path.Combine(AxoCoverPackage.PackageRoot, @"xUnitAdapter\xunit.runner.visualstudio.testadapter.dll");
      _redirectedAssemblies = Directory.GetFiles(Path.GetDirectoryName(_assemblyPath), "*.dll");
    }
  }
}
