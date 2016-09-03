using AxoCover.Models;
using AxoCover.ViewModels;
using Microsoft.Practices.Unity;
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
  [Guid(Id)]
  public sealed class AxoCoverPackage : Package
  {
    public const string Id = "26901782-38e1-48d4-94e9-557d44db052e";

    private UnityContainer _container;

    private TestExplorerWindow _window;

    public AxoCoverPackage()
    {
      Debug.WriteLine("Package instantiated.");
      _container = ContainerProvider.Container;
    }

    protected override void Initialize()
    {
      Debug.WriteLine("Package initializing...");
      base.Initialize();

      _container.RegisterType<ITestAssemblyScanner, IsolatedTestAssemblyScanner>();
      _container.RegisterType<ITestProvider, TestProvider>();
      _container.RegisterType<IEditorContext, EditorContext>();


      _window = new TestExplorerWindow();
      _window.Show();


      Debug.WriteLine("Package initialized.");
    }
  }
}