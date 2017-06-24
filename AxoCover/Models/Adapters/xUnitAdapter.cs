using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AxoCover.Common.Models;
using AxoCover.Common.Settings;
using AxoCover.Models.Data;
using EnvDTE;
using System.Text.RegularExpressions;
using AxoCover.Common.Extensions;
using AxoCover.Models.Extensions;
using System.IO;

namespace AxoCover.Models.Adapters
{
  public class xUnitAdapter : ITestAdapter
  {
    public string Name => "xUnit";

    public TestAdapterMode Mode => TestAdapterMode.Standard;

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

    public AdapterLoadingOptions GetLoadingOptions(Project project)
    {
      return new AdapterLoadingOptions()
      {
        AssemblyPath = _assemblyPath,
        RedirectedAssemblies = _redirectedAssemblies,
        RedirectionOptions = RedirectionOptions.ExcludeNonexistentDirectories | RedirectionOptions.ExcludeNonexistentFiles | RedirectionOptions.IncludeBaseDirectory
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
