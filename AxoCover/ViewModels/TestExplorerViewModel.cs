using AxoCover.Common.Events;
using AxoCover.Common.Extensions;
using AxoCover.Models.Commands;
using AxoCover.Models.Editor;
using AxoCover.Models.Events;
using AxoCover.Models.Extensions;
using AxoCover.Models.Storage;
using AxoCover.Models.Testing.Data;
using AxoCover.Models.Testing.Discovery;
using AxoCover.Models.Testing.Execution;
using AxoCover.Models.Testing.Results;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    private readonly IOptions _options;

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

    private bool _isSolutionLoading;
    public bool IsSolutionLoading
    {
      get
      {
        return _isSolutionLoading;
      }
      set
      {
        _isSolutionLoading = value;
        NotifyPropertyChanged(nameof(IsSolutionLoading));
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
    private TestMethod _testExecuting;

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
        if (value != null)
        {
          var tests = SelectedTestItem
            .Flatten(p => p.Children)
            .Select(p => p.CodeItem)
            .OfType<TestMethod>();
          LineCoverageAdornment.SelectedTests = new HashSet<TestMethod>(tests);
        }
        else
        {
          LineCoverageAdornment.SelectedTests = new HashSet<TestMethod>();
        }
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
          p => _editorContext.TryBuildSolution(),
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
          p => RunTestItem(SelectedTestItem, false, false),
          p => !IsBusy && SelectedTestItem != null,
          p => ExecuteOnPropertyChange(p, nameof(IsBusy), nameof(SelectedTestItem)));
      }
    }

    public ICommand CoverTestsCommand
    {
      get
      {
        return new DelegateCommand(
          p => RunTestItem(SelectedTestItem, true, false),
          p => !IsBusy && SelectedTestItem != null,
          p => ExecuteOnPropertyChange(p, nameof(IsBusy), nameof(SelectedTestItem)));
      }
    }

    public ICommand DebugTestsCommand
    {
      get
      {
        return new DelegateCommand(
          p => RunTestItem(SelectedTestItem, false, true),
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
          p => p.CheckAs<TestItem>(q => q.Kind == CodeItemKind.Class || q.Kind == CodeItemKind.Method || q.Kind == CodeItemKind.Data));
      }
    }

    public ICommand NavigateToSelectedItemCommand
    {
      get
      {
        return new DelegateCommand(
          p => NavigateToTestItem(SelectedTestItem.CodeItem),
          p => SelectedTestItem != null && (SelectedTestItem.CodeItem.Kind == CodeItemKind.Class || SelectedTestItem.CodeItem.Kind == CodeItemKind.Method || SelectedTestItem.CodeItem.Kind == CodeItemKind.Data),
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


    public TestExplorerViewModel(IEditorContext editorContext, ITestProvider testProvider, ITestRunner testRunner, IResultProvider resultProvider, ICoverageProvider coverageProvider, IOptions options, SelectTestCommand selectTestCommand, JumpToTestCommand jumpToTestCommand, DebugTestCommand debugTestCommand)
    {
      SearchViewModel = new CodeItemSearchViewModel<TestItemViewModel, TestItem>();
      StateGroups = new ObservableCollection<TestStateGroupViewModel>();

      _editorContext = editorContext;
      _testProvider = testProvider;
      _testRunner = testRunner;
      _resultProvider = resultProvider;
      _options = options;

      _editorContext.SolutionOpened += OnSolutionOpened;
      _editorContext.SolutionClosing += OnSolutionClosing;
      _editorContext.BuildStarted += OnBuildStarted;
      _editorContext.BuildFinished += OnBuildFinished;

      _testProvider.ScanningStarted += OnScanningStarted;
      _testProvider.ScanningFinished += OnScanningFinished;

      _testRunner.DebuggingStarted += OnDebuggingStarted;
      _testRunner.TestsStarted += OnTestsStarted;
      _testRunner.TestStarted += OnTestStarted;
      _testRunner.TestExecuted += OnTestExecuted;
      _testRunner.TestLogAdded += OnTestLogAdded;
      _testRunner.TestsFinished += OnTestsFinished;
      _testRunner.TestsFailed += OnTestsFailed;
      _testRunner.TestsAborted += OnTestsAborted;

      _options.PropertyChanged += OnOptionChanged;

      selectTestCommand.CommandCalled += OnSelectTest;
      jumpToTestCommand.CommandCalled += OnJumpToTest;
      debugTestCommand.CommandCalled += OnDebugTest;
      
      if (_editorContext.Solution.IsOpen)
      {
        LoadSolution();
      }
    }

    private async void OnOptionChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case nameof(IOptions.TestAdapterMode):
          await LoadSolution();
          break;
      }
    }

    private void RunTestItem(TestItemViewModel target, bool isCovering, bool isDebugging)
    {
      _testRunner.RunTestsAsync(target.CodeItem, isCovering, isDebugging);
      target.ScheduleAll();
    }

    private async void OnSolutionOpened(object sender, EventArgs e)
    {
      await LoadSolution();
    }

    private async void OnSolutionClosing(object sender, EventArgs e)
    {
      if (_testRunner.IsBusy)
      {
        await _testRunner.AbortTestsAsync();
      }
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
      await LoadSolution();

      if (!IsBusy && TestSolution?.AutoCoverTarget != null && _editorContext.IsBuildSuccessful)
      {
        RunTestItem(TestSolution.AutoCoverTarget, true, false);
      }
    }

    private async Task LoadSolution()
    {
      if (!_testProvider.IsActive)
      {
        IsSolutionLoading = true;
        var testSolution = await _testProvider.GetTestSolutionAsync(_editorContext.Solution, SelectedTestSettings);
        Update(testSolution);
        IsSolutionLoading = false;
        IsSolutionLoaded = true;
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

    private void OnDebuggingStarted(object sender, EventArgs e)
    {
      IsProgressIndeterminate = true;
      StatusMessage = Resources.DebuggingInProgress;
      RunnerState = RunnerStates.Testing;
    }

    private void OnTestsStarted(object sender, EventArgs<TestItem> e)
    {
      _testsToExecute = e.Value
        .Flatten(p => p.Children)
        .Where(p => p.IsTest())
        .Count();

      _testsExecuted = 0;
      IsProgressIndeterminate = true;
      StatusMessage = Resources.InitializingTestRunner;
      RunnerState = RunnerStates.Testing;
      TestSolution.ResetAll();
      StateGroups.Clear();
      _editorContext.ClearLog();
      _editorContext.ActivateLog();
    }

    private void OnTestStarted(object sender, EventArgs<TestMethod> e)
    {
      _testExecuting = e.Value;
      UpdateTestExecutionState();
    }

    private void OnTestExecuted(object sender, EventArgs<TestResult> e)
    {
      //Update test item view model and state groups
      var testItem = TestSolution.FindChild(e.Value.Method);
      if (testItem != null)
      {
        testItem.Result.Results.Add(e.Value);
        testItem.State = testItem.Result.Results.Max(p => p.Outcome);
        _testsExecuted++;

        var resultItem = testItem.CreateResultViewModel(e.Value);

        var stateGroup = StateGroups.FirstOrDefault(p => p.State == resultItem.State);
        if (stateGroup == null)
        {
          stateGroup = new TestStateGroupViewModel(resultItem.State);
          StateGroups.OrderedAdd(stateGroup, (a, b) => a.State.CompareTo(b.State));
        }

        stateGroup.Items.OrderedAdd(resultItem, (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.DisplayName, b.DisplayName));
      }

      //Update test execution state
      if (e.Value.Method == _testExecuting)
      {
        _testExecuting = null;
      }
      UpdateTestExecutionState();
    }

    private void UpdateTestExecutionState()
    {
      if (_testsExecuted < _testsToExecute)
      {
        IsProgressIndeterminate = false;
        Progress = (double)_testsExecuted / _testsToExecute;

        var statusSuffix = string.Empty;
        if (_testExecuting != null)
        {
          statusSuffix += " - " + _testExecuting.ShortName;
        }
        StatusMessage = string.Format(Resources.ExecutingTests, _testsExecuted, _testsToExecute) + statusSuffix;
      }
      else
      {
        IsProgressIndeterminate = true;
        StatusMessage = Resources.FinishingOperation;
      }
    }

    private void OnTestLogAdded(object sender, LogAddedEventArgs e)
    {
      _editorContext.WriteToLog(e.Text);
    }

    private void OnTestsFinished(object sender, EventArgs<TestReport> e)
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

    private void OnSelectTest(object sender, EventArgs<TestMethod> e)
    {
      SelectTestItem(e.Value);
    }

    private void OnDebugTest(object sender, EventArgs<TestMethod> e)
    {
      SelectTestItem(e.Value);
      if (DebugTestsCommand.CanExecute(null))
      {
        DebugTestsCommand.Execute(null);
      }
    }

    private void OnJumpToTest(object sender, EventArgs<TestMethod> e)
    {
      SelectTestItem(e.Value);
      if (NavigateToSelectedItemCommand.CanExecute(null))
      {
        NavigateToSelectedItemCommand.Execute(null);
      }
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

    public void SelectTestItem(TestMethod testMethod)
    {
      var item = TestSolution.FindChild(testMethod);
      if (item != null)
      {
        item.ExpandParents();
        item.IsSelected = true;
        IsTestsTabSelected = true;
        SelectedTestItem = item;
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
        case CodeItemKind.Data:
          testItem = testItem.Parent;
          goto case CodeItemKind.Method;
      }
    }
  }
}
