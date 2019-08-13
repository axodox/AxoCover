using AxoCover.Models;
using AxoCover.Models.Storage;
using AxoCover.Models.Updater;
using AxoCover.Views;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace AxoCover
{
  [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
  [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
  [ProvideToolWindow(typeof(TestExplorerToolWindow), MultiInstances = false, Style = VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Left, Window = "{C9C0AE26-AA77-11D2-B3F0-0000F87570EE}")]
  [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
  [Guid(Id)]
  [ProvideMenuResource("Menus.ctmenu", 1)]
  public sealed class AxoCoverPackage : AsyncPackage
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

    private IOptions _options;

    protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
      _options = ContainerProvider.Container.Resolve<IOptions>();

      await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

      var dte = await GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

      await InitializeTelemetryAsync();

      await CommandRepository.InitializeAsync(this, dte);
    }

    private async System.Threading.Tasks.Task InitializeTelemetryAsync()
    {
      if (!_options.IsTelemetryModeSelected)
      {
        ThreadHelper.ThrowIfNotOnUIThread();

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
  }
}