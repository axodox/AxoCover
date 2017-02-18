using AxoCover.Common.Settings;
using AxoCover.Models;
using AxoCover.Models.Data;
using AxoCover.Models.Extensions;
using AxoCover.Properties;
using AxoCover.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Media;

namespace AxoCover.ViewModels
{
  public class SettingsViewModel : ViewModel
  {
    private readonly IEditorContext _editorContext;
    private readonly IOutputCleaner _outputCleaner;
    private readonly ITestRunner _testRunner;
    private readonly ITelemetryManager _telemetryManager;

    public PackageManifest Manifest
    {
      get
      {
        return AxoCoverPackage.Manifest;
      }
    }

    public bool IsTelemetryEnabled
    {
      get
      {
        return _telemetryManager.IsTelemetryEnabled;
      }
      set
      {
        _telemetryManager.IsTelemetryEnabled = value;
        NotifyPropertyChanged(nameof(IsTelemetryEnabled));
      }
    }

    public bool IsShowingLineCoverage
    {
      get
      {
        return LineCoverageAdornment.IsShowingLineCoverage;
      }
      set
      {
        LineCoverageAdornment.IsShowingLineCoverage = value;
        NotifyPropertyChanged(nameof(IsShowingLineCoverage));
      }
    }

    public bool IsShowingBranchCoverage
    {
      get
      {
        return LineCoverageAdornment.IsShowingBranchCoverage;
      }
      set
      {
        LineCoverageAdornment.IsShowingBranchCoverage = value;
        NotifyPropertyChanged(nameof(IsShowingBranchCoverage));
      }
    }

    public bool IsShowingExceptions
    {
      get
      {
        return LineCoverageAdornment.IsShowingExceptions;
      }
      set
      {
        LineCoverageAdornment.IsShowingExceptions = value;
        NotifyPropertyChanged(nameof(IsShowingExceptions));
      }
    }

    public bool IsShowingPartialCoverage
    {
      get
      {
        return LineCoverageAdornment.IsShowingPartialCoverage;
      }
      set
      {
        LineCoverageAdornment.IsShowingPartialCoverage = value;
        NotifyPropertyChanged(nameof(IsShowingPartialCoverage));
      }
    }

    public Color SelectedColor
    {
      get
      {
        return LineCoverageAdornment.SelectedColor;
      }
      set
      {
        LineCoverageAdornment.SelectedColor = value;
        NotifyPropertyChanged(nameof(SelectedColor));
      }
    }

    public Color CoveredColor
    {
      get
      {
        return LineCoverageAdornment.CoveredColor;
      }
      set
      {
        LineCoverageAdornment.CoveredColor = value;
        NotifyPropertyChanged(nameof(CoveredColor));
      }
    }

    public Color MixedColor
    {
      get
      {
        return LineCoverageAdornment.MixedColor;
      }
      set
      {
        LineCoverageAdornment.MixedColor = value;
        NotifyPropertyChanged(nameof(MixedColor));
      }
    }

    public Color UncoveredColor
    {
      get
      {
        return LineCoverageAdornment.UncoveredColor;
      }
      set
      {
        LineCoverageAdornment.UncoveredColor = value;
        NotifyPropertyChanged(nameof(UncoveredColor));
      }
    }

    public Color ExceptionOriginColor
    {
      get
      {
        return LineCoverageAdornment.ExceptionOriginColor;
      }
      set
      {
        LineCoverageAdornment.ExceptionOriginColor = value;
        NotifyPropertyChanged(nameof(ExceptionOriginColor));
      }
    }

    public Color ExceptionTraceColor
    {
      get
      {
        return LineCoverageAdornment.ExceptionTraceColor;
      }
      set
      {
        LineCoverageAdornment.ExceptionTraceColor = value;
        NotifyPropertyChanged(nameof(ExceptionTraceColor));
      }
    }

    private bool _isCoveringByTest = Settings.Default.IsCoveringByTest;
    public bool IsCoveringByTest
    {
      get
      {
        return _isCoveringByTest;
      }
      set
      {
        _isCoveringByTest = value;
        Settings.Default.IsCoveringByTest = value;
        if (value) IsExcludingTestAssemblies = false;
        NotifyPropertyChanged(nameof(IsCoveringByTest));
      }
    }

    private string _excludeAttributes = Settings.Default.ExcludeAttributes;
    public string ExcludeAttributes
    {
      get
      {
        return _excludeAttributes;
      }
      set
      {
        _excludeAttributes = value;
        Settings.Default.ExcludeAttributes = value;
        NotifyPropertyChanged(nameof(ExcludeAttributes));
      }
    }

    private string _excludeFiles = Settings.Default.ExcludeFiles;
    public string ExcludeFiles
    {
      get
      {
        return _excludeFiles;
      }
      set
      {
        _excludeFiles = value;
        Settings.Default.ExcludeFiles = value;
        NotifyPropertyChanged(nameof(ExcludeFiles));
      }
    }

    private string _excludeDirectories = Settings.Default.ExcludeDirectories;
    public string ExcludeDirectories
    {
      get
      {
        return _excludeDirectories;
      }
      set
      {
        _excludeDirectories = value;
        Settings.Default.ExcludeDirectories = value;
        NotifyPropertyChanged(nameof(ExcludeDirectories));
      }
    }

    private string _filters = Settings.Default.Filters;
    public string Filters
    {
      get
      {
        return _filters;
      }
      set
      {
        _filters = value;
        Settings.Default.Filters = value;
        NotifyPropertyChanged(nameof(Filters));
      }
    }

    private bool _isIncludingSolutionAssemblies = Settings.Default.IsIncludingSolutionAssemblies;
    public bool IsIncludingSolutionAssemblies
    {
      get
      {
        return _isIncludingSolutionAssemblies;
      }
      set
      {
        _isIncludingSolutionAssemblies = value;
        Settings.Default.IsIncludingSolutionAssemblies = value;
        NotifyPropertyChanged(nameof(IsIncludingSolutionAssemblies));
      }
    }

    private bool _isExcludingTestAssemblies = Settings.Default.IsExcludingTestAssemblies;
    public bool IsExcludingTestAssemblies
    {
      get
      {
        return _isExcludingTestAssemblies;
      }
      set
      {
        _isExcludingTestAssemblies = value;
        Settings.Default.IsExcludingTestAssemblies = value;
        if (value) IsCoveringByTest = false;
        NotifyPropertyChanged(nameof(IsExcludingTestAssemblies));
      }
    }

    private TestPlatform _testPlatform;
    public TestPlatform TestPlatform
    {
      get
      {
        return _testPlatform;
      }
      set
      {
        _testPlatform = value;
        Settings.Default.TestPlatform = value;
        NotifyPropertyChanged(nameof(TestPlatform));
      }
    }

    private TestApartmentState _testApartmentState;
    public TestApartmentState TestApartmentState
    {
      get
      {
        return _testApartmentState;
      }
      set
      {
        _testApartmentState = value;
        Settings.Default.TestApartmentState = value;
        NotifyPropertyChanged(nameof(TestApartmentState));
      }
    }

    private TestItemViewModel _testSolution;
    public TestItemViewModel TestSolution
    {
      get
      {
        return _testSolution;
      }
      set
      {
        _testSolution = value;
        NotifyPropertyChanged(nameof(TestSolution));
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

    private string _selectedTestSettings;
    public string SelectedTestSettings
    {
      get
      {
        return _selectedTestSettings;
      }
      set
      {
        _selectedTestSettings = value;
        NotifyPropertyChanged(nameof(SelectedTestSettings));
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

    public string SelectedTestRunner
    {
      get
      {
        return (_testRunner as IMultiplexer).Implementation;
      }
      set
      {
        (_testRunner as IMultiplexer).Implementation = value;
        NotifyPropertyChanged(nameof(SelectedTestRunner));
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
        return new DelegateCommand(p => Process.Start(Settings.Default.IssuesUrl));
      }
    }

    public ICommand OpenSourceCodeCommand
    {
      get
      {
        return new DelegateCommand(p => Process.Start(Settings.Default.SourceCodeUrl));
      }
    }

    public ICommand SendFeedbackCommand
    {
      get
      {
        return new DelegateCommand(p => Process.Start("mailto:" + Settings.Default.FeedbackEmail));
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
          await _outputCleaner.CleanOutputAsync(p as TestOutputDescription);
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
        return new DelegateCommand(
          p => SelectedTestSettings = null,
          p => SelectedTestSettings != null,
          p => ExecuteOnPropertyChange(p, nameof(SelectedTestSettings)));
      }
    }

    public ICommand NavigateToFileCommand
    {
      get
      {
        return new DelegateCommand(
          p =>
          {
            _editorContext.NavigateToFile(p as string);
          });
      }
    }

    public SettingsViewModel(IEditorContext editorContext, IOutputCleaner outputCleaner, ITestRunner testRunner, ITelemetryManager telemetryManager)
    {
      _editorContext = editorContext;
      _outputCleaner = outputCleaner;
      _testRunner = testRunner;
      _telemetryManager = telemetryManager;

      _testSettingsFiles = new ObservableEnumeration<string>(() =>
        _editorContext?.Solution.FindFiles(new Regex("^.*\\.runSettings$", RegexOptions.Compiled | RegexOptions.IgnoreCase)) ?? new string[0], StringComparer.OrdinalIgnoreCase.Compare);

      editorContext.BuildFinished += (o, e) => Refresh();
      editorContext.SolutionOpened += (o, e) => Refresh();

      //Fix unsupported state
      if (IsExcludingTestAssemblies && IsCoveringByTest)
      {
        IsExcludingTestAssemblies = false;
        IsCoveringByTest = false;
      }
    }

    public async void RefreshProjectSizes()
    {
      if (TestSolution != null)
      {
        foreach (TestProjectViewModel testProject in TestSolution.Children.ToArray())
        {
          testProject.Output = await _outputCleaner.GetOutputFilesAsync(testProject.CodeItem as TestProject);
        }
      }
    }

    public void Refresh()
    {
      TestSettingsFiles.Refresh();
      RefreshProjectSizes();
    }
  }
}
