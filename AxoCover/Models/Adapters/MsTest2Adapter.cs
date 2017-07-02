using AxoCover.Common.Models;
using AxoCover.Common.Runner;
using AxoCover.Common.Settings;
using AxoCover.Models.Data;
using AxoCover.Models.Extensions;
using EnvDTE;
using System;
using System.IO;

namespace AxoCover.Models.Adapters
{
  public class MsTest2Adapter : ITestAdapter
  {
    public string Name => "MSTestV2";

    public TestAdapterMode Mode => TestAdapterMode.Standard;

    public string ExecutorUri => "executor://mstestadapter/v2";

    public bool IsTestSource(Project project)
    {
      return project.TryGetReference("Microsoft.VisualStudio.TestPlatform.TestFramework", out _);
    }

    public bool CanProcessCase(TestCase testCase)
    {
      return false;
    }

    public void ProcessCase(TestCase testCase, ref CodeItemKind testItemKind, ref string testItemPath, ref string displayName)
    {
      throw new NotSupportedException();
    }

    public TestAdapterOptions GetLoadingOptions()
    {
      return new TestAdapterOptions()
      {
        AssemblyPath = _assemblyPath,
        RedirectedAssemblies = _redirectedAssemblies,
        ExtensionUri = ExecutorUri
      };
    }

    private readonly string _assemblyPath;
    private readonly string[] _redirectedAssemblies;

    public MsTest2Adapter(IEditorContext editorContext)
    {
      _assemblyPath = Path.Combine(AxoCoverPackage.PackageRoot, @"MSTestAdapter\Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.dll");
      _redirectedAssemblies = Directory.GetFiles(Path.GetDirectoryName(_assemblyPath), "*.dll");
    }
  }
}
