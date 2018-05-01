using AxoCover.Common.Events;
using AxoCover.Common.Extensions;
using AxoCover.Models.Editor;
using AxoCover.Models.Testing.Data;
using AxoCover.Models.Testing.Execution;
using AxoCover.Models.Testing.Results;
using AxoCover.Views;
using System;
using System.Windows.Input;

namespace AxoCover.ViewModels
{
  public class CoverageExplorerViewModel : ViewModel
  {
    private readonly ICoverageProvider _coverageProvider;
    private readonly IEditorContext _editorContext;
    private readonly ITestRunner _testRunner;

    private CoverageItemViewModel _selectedCoverageItem;
    public CoverageItemViewModel SelectedCoverageItem
    {
      get
      {
        return _selectedCoverageItem;
      }
      set
      {
        _selectedCoverageItem = value;
        NotifyPropertyChanged(nameof(SelectedCoverageItem));
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
        value.Ordering = Ordering;
        _resultSolution = value;
        SearchViewModel.Solution = value;
        NotifyPropertyChanged(nameof(ResultSolution));
      }
    }

    private CoverageItemOrder _ordering;
    public CoverageItemOrder Ordering
    {
      get { return _ordering; }
      set
      {
        if (Equals(_ordering, value)) return;

        _ordering = value;
        NotifyPropertyChanged(nameof(Ordering));

        if (ResultSolution != null)
        {
          ResultSolution.Ordering = value;
        }
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
            if (coverageItem != null && (coverageItem.Kind == CodeItemKind.Class || coverageItem.Kind == CodeItemKind.Method) && coverageItem.SourceFile != null)
            {
              _editorContext.NavigateToFile(coverageItem.SourceFile, coverageItem.SourceLine);
            }
          });
      }
    }

    public ICommand CollapseAllCommand
    {
      get
      {
        return new DelegateCommand(p => ResultSolution.CollapseAll());
      }
    }

    public ICommand GenerateReportCommand
    {
      get
      {
        return new DelegateCommand(
          p => GenerateReport(),
          p => ReportPath != null,
          p => ExecuteOnPropertyChange(p, nameof(ReportPath)));
      }
    }

    private string _reportPath;
    public string ReportPath
    {
      get { return _reportPath; }
      set
      {
        _reportPath = value;
        NotifyPropertyChanged(nameof(ReportPath));
      }
    }

    private void GenerateReport()
    {
      var dialog = new ViewDialog<ReportGeneratorView>();
      dialog.View.ViewModel.GenerateReport(ReportPath);
      dialog.ShowDialog();
    }

    public CodeItemSearchViewModel<CoverageItemViewModel, CoverageItem> SearchViewModel { get; private set; }

    public CoverageExplorerViewModel(ICoverageProvider coverageProvider, IEditorContext editorContext, ITestRunner testRunner)
    {
      _coverageProvider = coverageProvider;
      _editorContext = editorContext;
      _testRunner = testRunner;

      SearchViewModel = new CodeItemSearchViewModel<CoverageItemViewModel, CoverageItem>();

      _coverageProvider.CoverageUpdated += OnCoverageUpdated;
      _editorContext.SolutionClosing += OnSolutionClosing;
      _testRunner.TestsFinished += OnTestsFinished;
    }

    private void OnTestsFinished(object sender, EventArgs<TestReport> e)
    {
      if (e.Value.CoverageReport != null)
      {
        ReportPath = e.Value.CoverageReport.FilePath;
      }
    }

    private void OnSolutionClosing(object sender, EventArgs e)
    {
      Update(null as CoverageItem);
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
          ResultSolution.Sort();
        }
      }
      else
      {
        ResultSolution = null;
      }
    }

    private async void OnCoverageUpdated(object sender, EventArgs e)
    {
      var resultSolution = await _coverageProvider.GetCoverageAsync();
      Update(resultSolution);
    }
  }
}
