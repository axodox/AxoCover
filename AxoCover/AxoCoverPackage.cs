using AxoCover.Models;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;

namespace AxoCover
{
  [PackageRegistration(UseManagedResourcesOnly = true)]
  [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
  [ProvideToolWindow(typeof(TestExplorerToolWindow), MultiInstances = false, Style = VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Left, Window = EnvDTE.Constants.vsWindowKindClassView)]
  [ProvideAutoLoad(UIContextGuids.SolutionExists)]
  [Guid(Id)]
  public sealed class AxoCoverPackage : Package
  {
    public const string Id = "26901782-38e1-48d4-94e9-557d44db052e";

    public const string ResourcesPath = "/AxoCover;component/Resources/";

    private readonly UnityContainer _container;

    public AxoCoverPackage()
    {
      _container = ContainerProvider.Container;
    }

    protected override void Initialize()
    {
      base.Initialize();

      _container.RegisterType<ITestAssemblyScanner, IsolatedTestAssemblyScanner>(new ContainerControlledLifetimeManager());
      _container.RegisterType<ITestProvider, TestProvider>(new ContainerControlledLifetimeManager());
      _container.RegisterType<IEditorContext, EditorContext>(new ContainerControlledLifetimeManager());
      _container.RegisterType<ITestRunner, TestRunner>(new ContainerControlledLifetimeManager());
      _container.RegisterType<ICoverageProvider, CoverageProvider>(new ContainerControlledLifetimeManager());
      _container.RegisterType<IResultProvider, ResultProvider>(new ContainerControlledLifetimeManager());

      var window = FindToolWindow(typeof(TestExplorerToolWindow), 0, true);
      (window.Frame as IVsWindowFrame).ShowNoActivate();
    }
  }
}