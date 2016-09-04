using AxoCover.Models;
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
  [ProvideToolWindow(typeof(TestExplorerToolWindow), MultiInstances = false, Style = VsDockStyle.Tabbed,
    Orientation = ToolWindowOrientation.Left, Window = EnvDTE.Constants.vsWindowKindClassView)]
  [ProvideAutoLoad(UIContextGuids.SolutionExists)]
  [Guid(Id)]
  public sealed class AxoCoverPackage : Package
  {
    public const string Id = "26901782-38e1-48d4-94e9-557d44db052e";

    private UnityContainer _container;

    public AxoCoverPackage()
    {
      Debug.WriteLine("Package instantiated.");
      _container = ContainerProvider.Container;
    }

    protected override void Initialize()
    {
      Debug.WriteLine("Package initializing...");
      base.Initialize();

      _container.RegisterType<ITestAssemblyScanner, IsolatedTestAssemblyScanner>(new ContainerControlledLifetimeManager());
      _container.RegisterType<ITestProvider, TestProvider>(new ContainerControlledLifetimeManager());
      _container.RegisterType<IEditorContext, EditorContext>(new ContainerControlledLifetimeManager());
      _container.RegisterType<ITestRunner, TestRunner>(new ContainerControlledLifetimeManager());

      var window = FindToolWindow(typeof(TestExplorerToolWindow), 0, true);
      (window.Frame as IVsWindowFrame).ShowNoActivate();

      Debug.WriteLine("Package initialized.");
    }
  }
}