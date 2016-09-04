using AxoCover.Models;
using AxoCover.Models.Data;
using AxoCover.Models.Events;
using System;
using System.Linq;
using System.Windows.Input;

namespace AxoCover.ViewModels
{
  public class TestExplorerViewModel : ViewModel
  {
    private IEditorContext _editorContext;
    private ITestProvider _testProvider;
    private ITestRunner _testRunner;

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
      Testing,
      Done
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

    private int _testsToExecute;
    public int TestsToExecute
    {
      get
      {
        return _testsToExecute;
      }
      set
      {
        _testsToExecute = value;
        NotifyPropertyChanged(nameof(TestsToExecute));
      }
    }

    private int _testsExecuted;
    public int TestsExecuted
    {
      get
      {
        return _testsExecuted;
      }
      set
      {
        _testsExecuted = value;
        NotifyPropertyChanged(nameof(TestsExecuted));
      }
    }

    private TestItemViewModel _TestSolution;
    public TestItemViewModel TestSolution
    {
      get
      {
        return _TestSolution;
      }
      private set
      {
        _TestSolution = value;
        NotifyPropertyChanged(nameof(TestSolution));
      }
    }

    private TestItemViewModel _SelectedItem;
    public TestItemViewModel SelectedItem
    {
      get
      {
        return _SelectedItem;
      }
      set
      {
        _SelectedItem = value;
        NotifyPropertyChanged(nameof(SelectedItem));
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
            TestsToExecute = SelectedItem.TestItem.TestCount;
            TestsExecuted = 0;
            _testRunner.RunTests(SelectedItem.TestItem);
            SelectedItem.ScheduleAll();
          },
          p => !IsBusy && SelectedItem != null,
          p => ExecuteOnPropertyChange(p, nameof(IsBusy), nameof(SelectedItem)));
      }
    }

    public TestExplorerViewModel(IEditorContext editorContext, ITestProvider testProvider, ITestRunner testRunner)
    {
      _editorContext = editorContext;
      _testProvider = testProvider;
      _testRunner = testRunner;

      _editorContext.SolutionOpened += OnSolutionOpened;
      _editorContext.SolutionClosing += OnSolutionClosing;
      _editorContext.BuildStarted += OnBuildStarted;
      _editorContext.BuildFinished += OnBuildFinished;

      _testRunner.TestsStarted += OnTestsStarted;
      _testRunner.TestExecuted += OnTestExecuted;
      _testRunner.TestLogAdded += OnTestLogAdded;
      _testRunner.TestsFinished += OnTestsFinished;
    }

    private void OnSolutionOpened(object sender, EventArgs e)
    {
      var testSolution = _testProvider.GetTestSolution(_editorContext.Solution);
      Update(testSolution);
      IsSolutionLoaded = true;
    }

    private void OnSolutionClosing(object sender, EventArgs e)
    {
      IsSolutionLoaded = false;
      Update(null);
    }

    private void OnBuildStarted(object sender, EventArgs e)
    {
      RunnerState = RunnerStates.Building;
    }

    private void OnBuildFinished(object sender, EventArgs e)
    {
      RunnerState = RunnerStates.Ready;
      IsSolutionLoaded = true;
      var testSolution = _testProvider.GetTestSolution(_editorContext.Solution);
      Update(testSolution);
    }

    private void OnTestsStarted(object sender, EventArgs e)
    {
      RunnerState = RunnerStates.Testing;
      TestSolution.ResetAll();
    }

    private void OnTestExecuted(object sender, TestExecutedEventArgs e)
    {
      var itemPath = e.Path.Split('.');

      var itemName = string.Empty;
      var testItem = TestSolution;
      foreach (var part in itemPath)
      {
        if (itemName != string.Empty)
        {
          itemName += ".";
        }
        itemName += part;

        var childItem = testItem.Children.FirstOrDefault(p => p.TestItem.Name == itemName);

        if (childItem != null)
        {
          itemName = string.Empty;
          testItem = childItem;
        }
      }

      if (testItem != null && itemName == string.Empty)
      {
        testItem.State = e.Outcome;
        TestsExecuted++;
      }
    }

    private void OnTestLogAdded(object sender, TestLogAddedEventArgs e)
    {
      _editorContext.WriteToLog(e.Text);
    }

    private void OnTestsFinished(object sender, TestFinishedEventArgs e)
    {
      RunnerState = RunnerStates.Done;
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
  }
}
