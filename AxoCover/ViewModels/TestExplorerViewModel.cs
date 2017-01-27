using AxoCover.Models;
using AxoCover.Models.Commands;
using AxoCover.Models.Data;
using AxoCover.Models.Events;
using AxoCover.Models.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
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

    private TestSolutionViewModel _testSolution;
    public TestSolutionViewModel TestSolution
    {
      get
      {
        return _testSolution;
      }
      private set
      {
        _testSolution = value;
        SearchViewModel.Solution = value;
        NotifyPropertyChanged(nameof(TestSolution));
      }
    }

    private TestItemViewModel _selectedTestItem;
    public TestItemViewModel SelectedTestItem
    {
      get
      {
        return _selectedTestItem;
      }
      set
      {
        _selectedTestItem = value;
        NotifyPropertyChanged(nameof(SelectedTestItem));
        NotifyPropertyChanged(nameof(IsTestItemSelected));
      }
    }

    public bool IsTestItemSelected
    {
      get
      {
        return SelectedTestItem != null;
      }
    }

    public ObservableCollection<TestStateGroupViewModel> StateGroups { get; set; }

    public CodeItemSearchViewModel<TestItemViewModel, TestItem> SearchViewModel { get; private set; }

    private bool _isTestsTabSelected = true;
    public bool IsTestsTabSelected
    {
      get
      {
        return _isTestsTabSelected;
      }
      set
      {
        _isTestsTabSelected = value;
        NotifyPropertyChanged(nameof(IsTestsTabSelected));

        if (value)
        {
          SearchViewModel.FilterText = null;
        }
      }
    }

    private bool _isSettingsTabSelected;
    public bool IsSettingsTabSelected
    {
      get
      {
        return _isSettingsTabSelected;
      }
      set
      {
        _isSettingsTabSelected = value;
        NotifyPropertyChanged(nameof(IsSettingsTabSelected));
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
          p => CoverTestItem(SelectedTestItem),
          p => !IsBusy && SelectedTestItem != null,
          p => ExecuteOnPropertyChange(p, nameof(IsBusy), nameof(SelectedTestItem)));
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
          p => NavigateToTestItem(p as TestItem),
          p => p.CheckAs<TestItem>(q => q.Kind == CodeItemKind.Class || q.Kind == CodeItemKind.Method));
      }
    }

    public ICommand NavigateToSelectedItemCommand
    {
      get
      {
        return new DelegateCommand(
          p => NavigateToTestItem(SelectedTestItem.CodeItem),
          p => SelectedTestItem != null && (SelectedTestItem.CodeItem.Kind == CodeItemKind.Class || SelectedTestItem.CodeItem.Kind == CodeItemKind.Method),
          p => ExecuteOnPropertyChange(p, nameof(SelectedTestItem)));
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

    public ICommand DebugTestItemCommand
    {
      get
      {
        return new DelegateCommand(
          p =>
          {
            var testItem = SelectedTestItem.CodeItem;
            _editorContext.NavigateToMethod(testItem.GetParent<TestProject>().Name, testItem.Parent.FullName, testItem.Name);
            _editorContext.DebugContextualTest();
          },
          p => SelectedTestItem != null && SelectedTestItem.CanDebugged,
          p => ExecuteOnPropertyChange(p, nameof(SelectedTestItem)));
      }
    }

    public TestExplorerViewModel(IEditorContext editorContext, ITestProvider testProvider, ITestRunner testRunner, IResultProvider resultProvider, ICoverageProvider coverageProvider, NavigateToTestCommand navigateToTestCommand)
    {
      _editorContext = editorContext;
      _testProvider = testProvider;
      _testRunner = testRunner;
      _resultProvider = resultProvider;

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

      SearchViewModel = new CodeItemSearchViewModel<TestItemViewModel, TestItem>();
      StateGroups = new ObservableCollection<TestStateGroupViewModel>();

      navigateToTestCommand.TestNavigated += OnTestNavigated;
    }

    private void CoverTestItem(TestItemViewModel target)
    {
      _testRunner.RunTestsAsync(target.CodeItem, SelectedTestSettings);
      target.ScheduleAll();
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
      SetStateToReady();
      IsSolutionLoaded = true;
      var testSolution = await _testProvider.GetTestSolutionAsync(_editorContext.Solution);
      Update(testSolution);

      if (!IsBusy && TestSolution?.AutoCoverTarget != null)
      {
        CoverTestItem(TestSolution.AutoCoverTarget);
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
      SetStateToReady();
    }

    private void OnTestsStarted(object sender, EventArgs e)
    {
      _testsToExecute = SelectedTestItem.TestCount;
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

    private void OnTestLogAdded(object sender, LogAddedEventArgs e)
    {
      _editorContext.WriteToLog(e.Text);
    }

    private void OnTestsFinished(object sender, TestFinishedEventArgs e)
    {
      SetStateToReady();
    }

    private void OnTestsFailed(object sender, EventArgs e)
    {
      SetStateToReady(Resources.TestRunFailed);
    }

    private void OnTestsAborted(object sender, EventArgs e)
    {
      SetStateToReady(Resources.TestRunAborted);
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
          TestSolution = new TestSolutionViewModel(testSolution);
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

    public void SelectTestItem(string name)
    {
      foreach (var child in TestSolution.Children)
      {
        var item = child.FindChild(name);
        if (item != null)
        {
          item.ExpandParents();
          item.IsSelected = true;
          IsTestsTabSelected = true;
          break;
        }
      }
    }

    private void SetStateToReady(string message = null)
    {
      if (_testRunner.IsBusy)
      {
        IsProgressIndeterminate = true;
        StatusMessage = Resources.FinishingOperation;
        RunnerState = RunnerStates.Testing;
      }
      else
      {
        IsProgressIndeterminate = false;
        StatusMessage = message ?? Resources.Done;
        RunnerState = RunnerStates.Ready;
      }
    }

    private void NavigateToTestItem(TestItem testItem)
    {
      switch (testItem.Kind)
      {
        case CodeItemKind.Class:
          _editorContext.NavigateToClass(testItem.GetParent<TestProject>().Name, testItem.FullName);
          break;
        case CodeItemKind.Method:
          _editorContext.NavigateToMethod(testItem.GetParent<TestProject>().Name, testItem.Parent.FullName, testItem.Name);
          break;
      }
    }
  }
}
