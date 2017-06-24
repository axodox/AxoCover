using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AxoCover.Common.Models;
using AxoCover.Common.Settings;
using AxoCover.Models.Data;
using EnvDTE;
using AxoCover.Models.Extensions;
using Microsoft.VisualStudio.Shell;
using System.IO;

namespace AxoCover.Models.Adapters
{
  public class MsTest1Adapter : ITestAdapter
  {
    public string Name => "MSTestV1";

    public TestAdapterMode Mode => TestAdapterMode.Integrated;

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

    public AdapterLoadingOptions GetLoadingOptions(Project project)
    {
      return new AdapterLoadingOptions()
      {
        AssemblyPath = _assemblyPath
      };
    }

    private readonly string _assemblyPath;

    public MsTest1Adapter(IEditorContext editorContext)
    {
      _assemblyPath = Path.Combine(editorContext.RootPath,  @"CommonExtensions\Microsoft\TestWindow\Extensions\Microsoft.VisualStudio.TestPlatform.Extensions.VSTestIntegration.dll");
    }
  }
}
