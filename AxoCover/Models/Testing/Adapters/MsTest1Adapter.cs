using AxoCover.Common.Models;
using AxoCover.Common.Runner;
using AxoCover.Common.Settings;
using AxoCover.Models.Editor;
using AxoCover.Models.Extensions;
using AxoCover.Models.Testing.Data;
using EnvDTE;
using System;
using System.IO;

namespace AxoCover.Models.Testing.Adapters
{
  public class MsTest1Adapter : ITestAdapter
  {
    public string Name => "MSTestV1";

    public TestAdapterMode Mode => TestAdapterMode.Integrated;

    public string ExecutorUri => "executor://mstestadapter/v1";

    public bool IsTestSource(Project project)
    {
      return project.TryGetReference("Microsoft.VisualStudio.QualityTools.UnitTestFramework", out _);
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
        ExtensionUri = ExecutorUri
      };
    }

    private readonly string _assemblyPath;

    public MsTest1Adapter(IEditorContext editorContext)
    {
      _assemblyPath = Path.Combine(editorContext.RootPath, @"CommonExtensions\Microsoft\TestWindow\Extensions\Microsoft.VisualStudio.TestPlatform.Extensions.VSTestIntegration.dll");
    }
  }
}
