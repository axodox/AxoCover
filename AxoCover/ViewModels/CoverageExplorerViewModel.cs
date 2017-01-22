using AxoCover.Models;
using AxoCover.Models.Data;
using AxoCover.Models.Extensions;
using System;
using System.Windows.Input;

namespace AxoCover.ViewModels
{
  public class CoverageExplorerViewModel : ViewModel
  {
    private readonly ICoverageProvider _coverageProvider;
    private readonly IEditorContext _editorContext;

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
        _resultSolution = value;
        SearchViewModel.Solution = value;
        NotifyPropertyChanged(nameof(ResultSolution));
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

    public ICommand NavigateToSelectedCoverageItemCommand
    {
      get
      {
        return new DelegateCommand(
          p => _editorContext.NavigateToFile(SelectedCoverageItem.CodeItem.SourceFile, SelectedCoverageItem.CodeItem.SourceLine),
          p => SelectedCoverageItem != null && (SelectedCoverageItem.CodeItem.Kind == CodeItemKind.Method || SelectedCoverageItem.CodeItem.Kind == CodeItemKind.Class),
          p => ExecuteOnPropertyChange(p, nameof(SelectedCoverageItem)));
      }
    }

    public CodeItemSearchViewModel<CoverageItemViewModel, CoverageItem> SearchViewModel { get; private set; }

    public CoverageExplorerViewModel(ICoverageProvider coverageProvider, IEditorContext editorContext)
    {
      _coverageProvider = coverageProvider;
      _editorContext = editorContext;

      SearchViewModel = new CodeItemSearchViewModel<CoverageItemViewModel, CoverageItem>();

      _coverageProvider.CoverageUpdated += OnCoverageUpdated;
      _editorContext.SolutionClosing += OnSolutionClosing;
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
