using System;
using System.Linq;
using AxoCover.Common.Models;
using AxoCover.Common.Settings;
using EnvDTE;
using System.Text.RegularExpressions;
using AxoCover.Common.Extensions;
using AxoCover.Models.Extensions;
using System.IO;
using AxoCover.Common.Runner;
using AxoCover.Models.Testing.Data;

namespace AxoCover.Models.Testing.Adapters
{
  public abstract class NUnitAdapter : ITestAdapter
  {
    public string Name { get; }

    public TestAdapterMode Mode => TestAdapterMode.Standard;

    public abstract string ExecutorUri { get; }

    public bool IsTestSource(Project project)
    {
      if (project.TryGetReference("nunit.framework", out var reference))
      {
        return reference.MajorVersion == _version;
      }
      else
      {
        return false;
      }
    }

    private Regex _nameWithArgumentsRegex = new Regex(@"^(?>(?<name>[\w.+<>]*)(?<arguments>(?>\((?>[^()'""]|'.'|""([^""]|(?<=\\)"")*""|(?<n>\()|(?<-n>\)))*\)(?(n)(?!)))))(?>\|[\d-]*)?$");

    public bool CanProcessCase(TestCase testCase)
    {
      return testCase.ExecutorUri.ToString().Contains("nunit", StringComparison.OrdinalIgnoreCase);
    }

    public void ProcessCase(TestCase testCase, ref CodeItemKind testItemKind, ref string testItemPath, ref string displayName)
    {
      var pathItems = testItemPath.SplitPath();
      var nameWithArgumentsMatch = _nameWithArgumentsRegex.Match(pathItems.Last());
      if (nameWithArgumentsMatch.Success)
      {
        testItemKind = CodeItemKind.Data;
        displayName = nameWithArgumentsMatch.Groups["arguments"].Value;
        testItemPath = string.Join(string.Empty, pathItems.Take(pathItems.Length - 1)) + nameWithArgumentsMatch.Groups["name"].Value + ".Instance" + testCase.Id.ToString("N");
      }
    }

    public TestAdapterOptions GetLoadingOptions()
    {
      return new TestAdapterOptions()
      {
        AssemblyPath = _assemblyPath,
        RedirectedAssemblies = _redirectedAssemblies,
        RedirectionOptions = FileRedirectionOptions.ExcludeNonexistentDirectories,
        ExtensionUri = ExecutorUri
      };
    }

    private readonly int _version;
    private readonly string _assemblyPath;
    private readonly string[] _redirectedAssemblies;

    public NUnitAdapter(int version, string assemblyName)
    {
      _version = version;
      Name = "NUnit" + version;
      _assemblyPath = Path.Combine(AxoCoverPackage.PackageRoot, $"NUnit{version}Adapter", assemblyName);
      _redirectedAssemblies = Directory.GetFiles(Path.GetDirectoryName(_assemblyPath), "*.dll");
    }
  }
}
