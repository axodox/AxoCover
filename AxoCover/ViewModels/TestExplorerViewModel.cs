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

    public ICommand BuildCommand
    {
      get
      {
        return new DelagateCommand(p => _editorContext.BuildSolution());
      }
    }

    public TestExplorerViewModel(IEditorContext editorContext, ITestProvider testProvider, ITestRunner testRunner)
    {
      _editorContext = editorContext;
      _testProvider = testProvider;
      _testRunner = testRunner;

      _editorContext.SolutionOpened += OnSolutionOpened;
      _editorContext.SolutionClosing += OnSolutionClosing;
      _editorContext.BuildFinished += OnBuildFinished;

      _testRunner.TestsStarted += OnTestsStarted;
      _testRunner.TestExecuted += OnTestExecuted;
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

    private void OnBuildFinished(object sender, EventArgs e)
    {
      IsSolutionLoaded = true;
      var testSolution = _testProvider.GetTestSolution(_editorContext.Solution);
      Update(testSolution);
    }


    private void OnTestsStarted(object sender, EventArgs e)
    {
      TestSolution.ResetState();
    }

    private void OnTestExecuted(object sender, TestExecutedEventArgs e)
    {
      var itemPath = e.Path.Split('.');

      var testItem = TestSolution;
      foreach (var part in itemPath)
      {
        testItem = testItem.Children.FirstOrDefault(p => p.TestItem.Name == part);

        if (testItem == null)
          break;
      }

      if (testItem != null)
      {
        testItem.State = e.Outcome;
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
