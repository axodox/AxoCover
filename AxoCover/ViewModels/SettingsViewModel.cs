using AxoCover.Models.Editor;
using AxoCover.Models.Storage;
using AxoCover.Models.Telemetry;
using AxoCover.Models.Testing.Data;
using AxoCover.Models.Testing.Execution;
using AxoCover.Models.Toolkit;
using AxoCover.Models.Updater;
using AxoCover.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace AxoCover.ViewModels
{
  public class SettingsViewModel : ViewModel
  {
    private readonly IEditorContext _editorContext;
    private readonly IStorageController _storageController;
    private readonly ITestRunner _testRunner;
    private readonly IOptions _options;
    private readonly IIoProvider _ioProvider;

    public PackageManifest Manifest
    {
      get
      {
        return AxoCoverPackage.Manifest;
      }
    }

    public string AssemblyVersion
    {
      get
      {
        return Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
      }
    }

    public IOptions Options
    {
      get
      {
        return _options;
      }
    }

    private readonly ObservableEnumeration<OutputDirectoryViewModel> _outputDirectories;
    public ObservableEnumeration<OutputDirectoryViewModel> OutputDirectories
    {
      get
      {
        return _outputDirectories;
      }
    }

    private readonly ObservableEnumeration<string> _testSettingsFiles;
    public ObservableEnumeration<string> TestSettingsFiles
    {
      get
      {
        return _testSettingsFiles;
      }
    }

    private bool _isSolutionOpened;
    public bool IsSolutionOpened
    {
      get
      {
        return _isSolutionOpened;
      }
      set
      {
        _isSolutionOpened = value;
        NotifyPropertyChanged(nameof(IsSolutionOpened));
      }
    }

    public bool CanSelectTestRunner
    {
      get
      {
        return TestRunners.Count() > 1;
      }
    }

    public IEnumerable<string> TestRunners
    {
      get
      {
        return (_testRunner as IMultiplexer).Implementations;
      }
    }

    public ICommand OpenWebSiteCommand
    {
      get
      {
        return new DelegateCommand(p => Process.Start(Manifest.WebSite));
      }
    }

    public ICommand OpenIssuesCommand
    {
      get
      {
        return new DelegateCommand(p => Process.Start(Options.IssuesUrl));
      }
    }

    public ICommand OpenSourceCodeCommand
    {
      get
      {
        return new DelegateCommand(p => Process.Start(Options.SourceCodeUrl));
      }
    }

    public ICommand SendFeedbackCommand
    {
      get
      {
        return new DelegateCommand(p => Process.Start("mailto:" + Options.FeedbackEmail));
      }
    }

    public ICommand OpenLicenseDialogCommand
    {
      get
      {
        return new DelegateCommand(p =>
        {
          var dialog = new ViewDialog<TextView>()
          {
            Title = Manifest.Name + " " + Resources.License
          };
          dialog.View.ViewModel.Text = Manifest.License;
          dialog.ShowDialog();
        });
      }
    }

    public ICommand OpenReleaseNotesDialogCommand
    {
      get
      {
        return new DelegateCommand(p =>
        {
          var dialog = new ViewDialog<TextView>()
          {
            Title = Manifest.Name + " " + Resources.ReleaseNotes
          };
          dialog.View.ViewModel.Text = Manifest.ReleaseNotes;
          dialog.ShowDialog();
        });
      }
    }

    public ICommand CleanTestOutputCommand
    {
      get
      {
        return new DelegateCommand(async p =>
        {
          await _storageController.CleanOutputAsync(p as OutputDescription);
          RefreshProjectSizes();
        });
      }
    }

    public ICommand OpenPathCommand
    {
      get
      {
        return new DelegateCommand(p => _editorContext.OpenPathInExplorer(p as string));
      }
    }

    public ICommand ClearTestSettingsCommand
    {
      get
      {
        return new DelegateCommand(p => Options.TestSettings = null);
      }
    }

    public ICommand NavigateToFileCommand
    {
      get
      {
        return new DelegateCommand(
          p =>
          {
            _editorContext.NavigateToFile(_ioProvider.GetAbsolutePath(p as string));
          });
      }
    }

    public SettingsViewModel(IEditorContext editorContext, IStorageController storageController, ITestRunner testRunner, ITelemetryManager telemetryManager, IOptions options, IIoProvider ioProvider)
    {
      _editorContext = editorContext;      
      _storageController = storageController;
      _testRunner = testRunner;
      _options = options;
      _ioProvider = ioProvider;

      _outputDirectories = new ObservableEnumeration<OutputDirectoryViewModel>(() =>
        storageController.GetOutputDirectories().Select(p => new OutputDirectoryViewModel(p)), (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name));
      _testSettingsFiles = new ObservableEnumeration<string>(() =>
        (_editorContext?.Solution.FindFiles(new Regex("^.*\\.runSettings$", RegexOptions.Compiled | RegexOptions.IgnoreCase)) ?? new string[0])
          .Select(p => _ioProvider.GetRelativePath(p)), 
        StringComparer.OrdinalIgnoreCase.Compare);

      editorContext.BuildFinished += (o, e) => Refresh();
      editorContext.SolutionOpened += (o, e) => { Refresh(); IsSolutionOpened = true; };
      editorContext.SolutionClosing += (o, e) => IsSolutionOpened = false;

      //Fix unsupported state
      if (_options.IsExcludingTestAssemblies && _options.IsCoveringByTest)
      {
        _options.IsExcludingTestAssemblies = false;
        _options.IsCoveringByTest = false;
      }
    }

    public async void RefreshProjectSizes()
    {
      foreach (var outputDirectory in OutputDirectories.ToArray())
      {
        outputDirectory.Output = await _storageController.GetOutputFilesAsync(outputDirectory.Location);
      }
    }

    public void Refresh()
    {
      OutputDirectories.Refresh();
      TestSettingsFiles.Refresh();
      RefreshProjectSizes();
    }
  }
}
