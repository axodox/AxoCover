using AxoCover.Models;
using AxoCover.Models.Data;
using System.Windows.Input;

namespace AxoCover.ViewModels
{
  public class TestExplorerViewModel : ViewModel
  {
    private IEditorContext _editorContext;
    private ITestProvider _testProvider;

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

    public TestExplorerViewModel(IEditorContext editorContext, ITestProvider testProvider)
    {
      _editorContext = editorContext;
      _testProvider = testProvider;

      _editorContext.SolutionOpened += OnSolutionOpened;
      _editorContext.SolutionClosing += OnSolutionClosing;
      _editorContext.BuildFinished += OnBuildFinished;
    }

    private void OnSolutionOpened(object sender, System.EventArgs e)
    {
      var testSolution = _testProvider.GetTestSolution(_editorContext.Solution);
      Update(testSolution);
      IsSolutionLoaded = true;
    }

    private void OnSolutionClosing(object sender, System.EventArgs e)
    {
      IsSolutionLoaded = false;
      Update(null);
    }

    private void OnBuildFinished(object sender, System.EventArgs e)
    {
      IsSolutionLoaded = true;
      var testSolution = _testProvider.GetTestSolution(_editorContext.Solution);
      Update(testSolution);
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
          TestSolution.Update(testSolution);
        }
      }
      else
      {
        TestSolution = null;
      }
    }
  }
}
