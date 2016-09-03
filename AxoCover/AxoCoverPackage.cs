using AxoCover.Model;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace AxoCover
{
  [PackageRegistration(UseManagedResourcesOnly = true)]
  [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
  [ProvideAutoLoad(UIContextGuids.SolutionExists)]
  [Guid(Id)]
  public sealed class AxoCoverPackage : Package
  {
    public const string Id = "26901782-38e1-48d4-94e9-557d44db052e";

    public DTE Context { get; private set; }

    public AxoCoverPackage()
    {
      Debug.WriteLine("Package instantiated.");
    }

    protected override void Initialize()
    {
      Debug.WriteLine("Package initializing...");
      base.Initialize();

      Context = GetGlobalService(typeof(DTE)) as DTE;
      Context.Events.SolutionEvents.Opened += OnSolutionOpened;
      Context.Events.BuildEvents.OnBuildDone += OnBuildDone;
      Debug.WriteLine("Package initialized.");
    }

    private void OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
    {
      var w = new TestProvider(Context).GetTests().ToList();
    }

    private void OnSolutionOpened()
    {

    }


  }
}