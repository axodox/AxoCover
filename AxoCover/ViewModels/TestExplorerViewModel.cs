using AxoCover.Models;
using AxoCover.Models.Commands;
using AxoCover.Models.Data;
using AxoCover.Models.Events;
using AxoCover.Models.Extensions;
using AxoCover.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AxoCover.ViewModels
{
  public class TestExplorerViewModel : ViewModel
  {
    private readonly IEditorContext _editorContext;
    private readonly ITestProvider _testProvider;
    private readonly ITestRunner _testRunner;
    private readonly IResultProvider _resultProvider;
    private readonly ICoverageProvider _coverageProvider;
    private readonly IOutputCleaner _outputCleaner;

    private bool _isSolutionLoaded;
    public bool IsSolutionLoaded
    {
      get
      {
        return _isSolutionLoaded;
      }
      set
      {
        _isSolutionLoaded = value;
        NotifyPropertyChanged(nameof(IsSolutionLoaded));
      }
    }

    public enum RunnerStates
    {
      Ready,
      Building,
      Scanning,
      Testing
    }

    private RunnerStates _runnerState;
    public RunnerStates RunnerState
    {
      get
      {
        return _runnerState;
      }
      set
      {
        _runnerState = value;
        NotifyPropertyChanged(nameof(RunnerState));
        NotifyPropertyChanged(nameof(IsBusy));
        NotifyPropertyChanged(nameof(IsTesting));
        NotifyPropertyChanged(nameof(IsNotTesting));
      }
    }

    public bool IsBusy
    {
      get
      {
        return RunnerState == RunnerStates.Building || RunnerState == RunnerStates.Testing;
      }
    }

    public bool IsTesting
    {
      get
      {
        return RunnerState == RunnerStates.Testing;
      }
    }

    public bool IsNotTesting
    {
      get
      {
        return RunnerState != RunnerStates.Testing;
      }
    }

    private bool _isProgressIndeterminate;
    public bool IsProgressIndeterminate
    {
      get
      {
        return _isProgressIndeterminate;
      }
      set
      {
        _isProgressIndeterminate = value;
        NotifyPropertyChanged(nameof(IsProgressIndeterminate));
      }
    }

    private int _testsToExecute;
    private int _testsExecuted;

    private double _Progress;
    public double Progress
    {
      get
      {
        return _Progress;
      }
      set
      {
        _Progress = value;
        NotifyPropertyChanged(nameof(Progress));
      }
    }

    private string _statusMessage = Resources.Ready;
    public string StatusMessage
    {
      get
      {
        return _statusMessage;
      }
      set
      {
        _statusMessage = value;
        NotifyPropertyChanged(nameof(StatusMessage));
      }
    }

    private bool _isAutoCoverEnabled = Settings.Default.IsAutoCoverEnabled;
    public bool IsAutoCoverEnabled
    {
      get
      {
        return _isAutoCoverEnabled;
      }
      set
      {
        _isAutoCoverEnabled = value;
        Settings.Default.IsAutoCoverEnabled = value;
        NotifyPropertyChanged(nameof(IsAutoCoverEnabled));
      }
    }

    public bool IsHighlighting
    {
      get
      {
        return LineCoverageAdornment.IsHighlighting;
      }
      set
      {
        LineCoverageAdornment.IsHighlighting = value;
        NotifyPropertyChanged(nameof(IsHighlighting));
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

    private readonly ObservableEnumeration<string> _testSettingsFiles;
    public ObservableEnumeration<string> TestSettingsFiles
    {
      get
      {
        return _testSettingsFiles;
      }
    }

    private TestItemViewModel _testSolution;
    public TestItemViewModel TestSolution
    {
      get
      {
        return _testSolution;
      }
      private set
      {
        if (_testSolution != null)
        {
          RemoveItems(_testSolution.Flatten(p => p.Children));
        }
        _testSolution = value;
        if (_testSolution != null)
        {
          AddItems(_testSolution.Flatten(p => p.Children));
        }
        NotifyPropertyChanged(nameof(TestSolution));
      }
    }

    private TestItemViewModel _selectedItem;
    public TestItemViewModel SelectedItem
    {
      get
      {
        return _selectedItem;
      }
      set
      {
        _selectedItem = value;
        NotifyPropertyChanged(nameof(SelectedItem));
        NotifyPropertyChanged(nameof(IsItemSelected));
      }
    }

    public bool IsItemSelected
    {
      get
      {
        return SelectedItem != null;
      }
    }

    public ObservableCollection<TestStateGroupViewModel> StateGroups { get; set; }

    public bool IsStateGroupSelected
    {
      get
      {
        return StateGroups.Any(p => p.IsSelected);
      }
    }

    private readonly ObservableCollection<TestItemViewModel> _testList;
    public OrderedFilteredCollection<TestItemViewModel> TestList
    {
      get;
      private set;
    }

    private string _filterText = string.Empty;
    public string FilterText
    {
      get
      {
        return _filterText;
      }
      set
      {
        _filterText = value ?? string.Empty;
        NotifyPropertyChanged(nameof(FilterText));
        var filterText = _filterText.ToLower();
        TestList.ApplyFilter(p => p.CodeItem.Name.ToLower().Contains(filterText));
      }
    }
    

    private CoverageItemViewModel _resultSolution;
    public CoverageItemViewModel ResultSolution
    {
      get
      {
        return _resultSolution;
      }
      set
      {
        _resultSolution = value;
        NotifyPropertyChanged(nameof(ResultSolution));
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

    public ICommand BuildCommand
    {
      get
      {
        return new DelegateCommand(
          p => _editorContext.BuildSolution(),
          p => !IsBusy,
          p => ExecuteOnPropertyChange(p, nameof(IsBusy)));
      }
    }

    public ICommand ExpandAllCommand
    {
      get
      {
        return new DelegateCommand(p => TestSolution.ExpandAll());
      }
    }

    public ICommand CollapseAllCommand
    {
      get
      {
        return new DelegateCommand(p => TestSolution.CollapseAll());
      }
    }

    public ICommand RunTestsCommand
    {
      get
      {
        return new DelegateCommand(
          p =>
          {
            _testRunner.RunTestsAsync(SelectedItem.CodeItem, SelectedTestSettings);
            SelectedItem.ScheduleAll();
          },
          p => !IsBusy && SelectedItem != null,
          p => ExecuteOnPropertyChange(p, nameof(IsBusy), nameof(SelectedItem)));
      }
    }

    public ICommand AbortTestsCommand
    {
      get
      {
        return new DelegateCommand(
          p =>
          {
            _testRunner.AbortTestsAsync();
          },
          p => IsBusy,
          p => ExecuteOnPropertyChange(p, nameof(IsBusy)));
      }
    }

    public ICommand NavigateToTestItemCommand
    {
      get
      {
        return new DelegateCommand(
          p =>
          {
            var testItem = p as TestItem;
            switch (testItem.Kind)
            {
              case CodeItemKind.Class:
                _editorContext.NavigateToClass(testItem.GetParent<TestProject>().Name, testItem.FullName);
                break;
              case CodeItemKind.Method:
                _editorContext.NavigateToMethod(testItem.GetParent<TestProject>().Name, testItem.Parent.FullName, testItem.Name);
                break;
            }
          },
          p => p.CheckAs<TestItem>(q => q.Kind == CodeItemKind.Class || q.Kind == CodeItemKind.Method));
      }
    }

    public ICommand NavigateToCoverageItemCommand
    {
      get
      {
        return new DelegateCommand(
          p =>
          {
            var coverageItem = p as CoverageItem;
            if (coverageItem.SourceFile != null)
            {
              _editorContext.NavigateToFile(coverageItem.SourceFile, coverageItem.SourceLine);
            }
          },
          p => p.CheckAs<CoverageItem>(q => q.Kind == CodeItemKind.Class || q.Kind == CodeItemKind.Method));
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

    public ICommand SelectStateGroupCommand
    {
      get
      {
        return new DelegateCommand(
          p =>
          {
            var selectedStateGroup = p as TestStateGroupViewModel;
            var previousState = selectedStateGroup.IsSelected;

            foreach (var stateGroup in StateGroups)
            {
              stateGroup.IsSelected = false;
            }

            selectedStateGroup.IsSelected = !previousState;
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

    public TestExplorerViewModel(IEditorContext editorContext, ITestProvider testProvider, ITestRunner testRunner, IResultProvider resultProvider, ICoverageProvider coverageProvider, IOutputCleaner outputCleaner,
      NavigateToTestCommand navigateToTestCommand)
    {
      _editorContext = editorContext;
      _testProvider = testProvider;
      _testRunner = testRunner;
      _coverageProvider = coverageProvider;
      _resultProvider = resultProvider;
      _outputCleaner = outputCleaner;

      _editorContext.SolutionOpened += OnSolutionOpened;
      _editorContext.SolutionClosing += OnSolutionClosing;
      _editorContext.BuildStarted += OnBuildStarted;
      _editorContext.BuildFinished += OnBuildFinished;

      _testProvider.ScanningStarted += OnScanningStarted;
      _testProvider.ScanningFinished += OnScanningFinished;

      _testRunner.TestsStarted += OnTestsStarted;
      _testRunner.TestExecuted += OnTestExecuted;
      _testRunner.TestLogAdded += OnTestLogAdded;
      _testRunner.TestsFinished += OnTestsFinished;
      _testRunner.TestsFailed += OnTestsFailed;
      _testRunner.TestsAborted += OnTestsAborted;

      _resultProvider.ResultsUpdated += OnResultsUpdated;
      _coverageProvider.CoverageUpdated += OnCoverageUpdated;

      StateGroups = new ObservableCollection<TestStateGroupViewModel>();

      navigateToTestCommand.TestNavigated += OnTestNavigated;

      _testList = new ObservableCollection<TestItemViewModel>();
      TestList = new OrderedFilteredCollection<TestItemViewModel>(_testList, (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.CodeItem.Name, b.CodeItem.Name));

      _testSettingsFiles = new ObservableEnumeration<string>(() => IsSolutionLoaded ?
        _editorContext.Solution.FindFiles(new Regex("^.*\\.testSettings$", RegexOptions.Compiled | RegexOptions.IgnoreCase)) : new string[0], StringComparer.OrdinalIgnoreCase.Compare);
    }
    

    private async void OnSolutionOpened(object sender, EventArgs e)
    {
      var testSolution = await _testProvider.GetTestSolutionAsync(_editorContext.Solution);
      Update(testSolution);
      IsSolutionLoaded = true;
    }

    private void OnSolutionClosing(object sender, EventArgs e)
    {
      IsSolutionLoaded = false;
      Update(null as TestSolution);
      Update(null as CoverageItem);
      StateGroups.Clear();
    }

    private void OnBuildStarted(object sender, EventArgs e)
    {
      IsProgressIndeterminate = true;
      StatusMessage = Resources.Building;
      RunnerState = RunnerStates.Building;
    }

    private async void OnBuildFinished(object sender, EventArgs e)
    {
      IsProgressIndeterminate = false;
      StatusMessage = Resources.Done;
      RunnerState = RunnerStates.Ready;
      IsSolutionLoaded = true;
      var testSolution = await _testProvider.GetTestSolutionAsync(_editorContext.Solution);
      Update(testSolution);

      if (IsAutoCoverEnabled && RunTestsCommand.CanExecute(null))
      {
        RunTestsCommand.Execute(null);
      }
    }

    private void OnScanningStarted(object sender, EventArgs e)
    {
      IsProgressIndeterminate = true;
      StatusMessage = Resources.ScanningForTests;
      RunnerState = RunnerStates.Scanning;
    }

    private void OnScanningFinished(object sender, EventArgs e)
    {
      IsProgressIndeterminate = false;
      StatusMessage = Resources.Done;
      RunnerState = RunnerStates.Ready;
    }

    private void OnTestsStarted(object sender, EventArgs e)
    {
      _testsToExecute = SelectedItem.CodeItem.TestCount;
      _testsExecuted = 0;
      IsProgressIndeterminate = true;
      StatusMessage = Resources.InitializingTestRunner;
      RunnerState = RunnerStates.Testing;
      TestSolution.ResetAll();
      StateGroups.Clear();
      _editorContext.ClearLog();
      _editorContext.ActivateLog();
    }

    private void OnTestExecuted(object sender, TestExecutedEventArgs e)
    {
      //Update test item view model and state groups
      var testItem = TestSolution.FindChild(e.Path);
      if (testItem != null)
      {
        testItem.State = e.Outcome;
        _testsExecuted++;

        var stateGroup = StateGroups.FirstOrDefault(p => p.State == testItem.State);
        if (stateGroup == null)
        {
          stateGroup = new TestStateGroupViewModel(testItem.State);
          StateGroups.OrderedAdd(stateGroup, (a, b) => a.State.CompareTo(b.State));
        }
        stateGroup.Items.OrderedAdd(testItem, (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.CodeItem.Name, b.CodeItem.Name));
      }

      //Update test execution state
      if (_testsExecuted < _testsToExecute)
      {
        IsProgressIndeterminate = false;
        Progress = (double)_testsExecuted / _testsToExecute;
        StatusMessage = string.Format(Resources.ExecutingTests, _testsExecuted, _testsToExecute);
      }
      else
      {
        IsProgressIndeterminate = true;
        StatusMessage = Resources.GeneratingCoverageReport;
      }
    }

    private void OnTestLogAdded(object sender, TestLogAddedEventArgs e)
    {
      _editorContext.WriteToLog(e.Text);
    }

    private void OnTestsFinished(object sender, TestFinishedEventArgs e)
    {
      IsProgressIndeterminate = false;
      StatusMessage = Resources.Done;
      RunnerState = RunnerStates.Ready;
    }

    private void OnTestsFailed(object sender, EventArgs e)
    {
      IsProgressIndeterminate = false;
      StatusMessage = Resources.TestRunFailed;
      RunnerState = RunnerStates.Ready;
    }

    private void OnTestsAborted(object sender, EventArgs e)
    {
      IsProgressIndeterminate = false;
      StatusMessage = Resources.TestRunAborted;
      RunnerState = RunnerStates.Ready;
    }

    private async void OnResultsUpdated(object sender, EventArgs e)
    {
      var testMethodViewModels = TestSolution
        .Children
        .Flatten(p => p.Children)
        .Where(p => p.CodeItem.Kind == CodeItemKind.Method)
        .ToList();

      var items = new ConcurrentDictionary<TestItemViewModel, TestResult>();

      await Task.Run(() =>
      {
        Parallel.ForEach(testMethodViewModels, p =>
        {
          var result = _resultProvider.GetTestResult(p.CodeItem as TestMethod);
          if (result != null)
          {
            items[p] = result;
          }
        });
      });

      foreach (var item in items)
      {
        item.Key.Result = item.Value;
      }
    }

    private async void OnCoverageUpdated(object sender, EventArgs e)
    {
      var resultSolution = await _coverageProvider.GetCoverageAsync();
      Update(resultSolution);
    }

    private void OnTestNavigated(object sender, TestNavigatedEventArgs e)
    {
      SelectTestItem(e.Name);
    }

    private void Update(TestSolution testSolution)
    {
      if (testSolution != null)
      {
        if (TestSolution == null)
        {
          TestSolution = new TestItemViewModel(null, testSolution);
        }
        else
        {
          TestSolution.UpdateItem(testSolution);
        }
      }
      else
      {
        TestSolution = null;
      }
    }

    private void Update(CoverageItem resultSolution)
    {
      if (resultSolution != null)
      {
        if (ResultSolution == null)
        {
          ResultSolution = new CoverageItemViewModel(null, resultSolution);
        }
        else
        {
          ResultSolution.UpdateItem(resultSolution);
        }
      }
      else
      {
        ResultSolution = null;
      }
    }
    
    public void SelectTestItem(string name)
    {
      foreach (var child in TestSolution.Children)
      {
        var item = child.FindChild(name);
        if (item != null)
        {
          item.ExpandParents();
          item.IsSelected = true;
          break;
        }
      }
    }

    private void OnTestItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
      {
        RemoveItems(e.OldItems.OfType<TestItemViewModel>());
      }

      if (e.NewItems != null)
      {
        AddItems(e.NewItems.OfType<TestItemViewModel>());
      }
    }

    private void RemoveItems(IEnumerable<TestItemViewModel> items)
    {
      foreach (var item in items)
      {
        item.Children.CollectionChanged -= OnTestItemCollectionChanged;
        _testList.Remove(item);
      }
    }

    private void AddItems(IEnumerable<TestItemViewModel> items)
    {
      foreach (var item in items)
      {
        _testList.Add(item);
        item.Children.CollectionChanged += OnTestItemCollectionChanged;
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
  }
}
