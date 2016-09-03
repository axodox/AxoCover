using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AxoCover
{
  [PackageRegistration(UseManagedResourcesOnly = true)]
  [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
  [ProvideAutoLoad(UIContextGuids.SolutionExists)]
  [Guid(Guid)]
  public sealed class AxoCoverPackage : Package
  {
    public const string Guid = "26901782-38e1-48d4-94e9-557d44db052e";

    public AxoCoverPackage()
    {
      Debug.WriteLine("Package instantiated.");
    }

    protected override void Initialize()
    {
      Debug.WriteLine("Package initializing...");
      base.Initialize();
      Debug.WriteLine("Package initialized.");
    }
  }
}