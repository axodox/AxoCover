using AxoCover.Models;
using AxoCover.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel;
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

    public AxoCoverPackage()
    {
      Settings.Default.PropertyChanged += OnSettingChanged;
      ContainerProvider.Initialize();
    }

    private void OnSettingChanged(object sender, PropertyChangedEventArgs e)
    {
      Settings.Default.Save();
    }

    protected override void Initialize()
    {
      base.Initialize();
      var window = FindToolWindow(typeof(TestExplorerToolWindow), 0, true);
      (window.Frame as IVsWindowFrame).ShowNoActivate();
    }
  }
}