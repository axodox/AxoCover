using AxoCover.Models;
using AxoCover.Models.Storage;
using AxoCover.Models.Updates;
using AxoCover.Views;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace AxoCover
{
  [PackageRegistration(UseManagedResourcesOnly = true)]
  [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
  [ProvideToolWindow(typeof(TestExplorerToolWindow), MultiInstances = false, Style = VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Left, Window = EnvDTE.Constants.vsWindowKindClassView)]
  [ProvideAutoLoad(UIContextGuids.SolutionExists)]
  [Guid(Id)]
  [ProvideMenuResource("Menus.ctmenu", 1)]
  public sealed class AxoCoverPackage : Package
  {
    public const string Id = "26901782-38e1-48d4-94e9-557d44db052e";

    public const string ResourcesPath = "/AxoCover;component/Resources/";

    public static readonly string PackageRoot;

    public static readonly PackageManifest Manifest;

    static AxoCoverPackage()
    {
      PackageRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      Manifest = PackageManifest.FromFile(Path.Combine(PackageRoot, "extension.vsixmanifest"));
    }

    private readonly IOptions _options;

    public AxoCoverPackage()
    {
      _options = ContainerProvider.Container.Resolve<IOptions>();
      Application.Current.Dispatcher.BeginInvoke(new Action(InitializeTelemetry), DispatcherPriority.ApplicationIdle);
    }

    private void InitializeTelemetry()
    {
      if (!_options.IsTelemetryModeSelected)
      {
        var dialog = new ViewDialog<TelemetryIntroductionView>()
        {
          ResizeMode = ResizeMode.NoResize
        };

        if (dialog.ShowDialog() == true)
        {
          _options.IsTelemetryModeSelected = true;
        }
      }
    }

    protected override void Initialize()
    {
      base.Initialize();
      OpenAxoCoverCommand.Initialize(this);
    }
  }
}