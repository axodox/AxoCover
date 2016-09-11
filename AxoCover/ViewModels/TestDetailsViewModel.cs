using AxoCover.Models;
using AxoCover.Models.Data;
using System.ComponentModel;
using System.Windows.Input;

namespace AxoCover.ViewModels
{
  public class TestDetailsViewModel : ViewModel
  {
    private IEditorContext _editorContext;

    private TestItemViewModel _selectedItem;
    public TestItemViewModel SelectedItem
    {
      get
      {
        return _selectedItem;
      }
      set
      {
        if (_selectedItem != null)
        {
          _selectedItem.PropertyChanged -= OnPropertyChanged;
        }

        _selectedItem = value;
        NotifyPropertyChanged(nameof(SelectedItem));
        NotifyPropertyChanged(nameof(IsSelectionValid));

        if (_selectedItem != null)
        {
          _selectedItem.PropertyChanged += OnPropertyChanged;
        }
      }
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(TestItemViewModel.Result))
      {
        NotifyPropertyChanged(nameof(IsSelectionValid));
      }
    }

    public bool IsSelectionValid
    {
      get
      {
        return SelectedItem != null && (SelectedItem.TestItem.Kind == TestItemKind.Method && SelectedItem.Result != null);
      }
    }

    public ICommand NavigateToStackItemCommand
    {
      get
      {
        return new DelegateCommand(p =>
        {
          var stackItem = p as StackItem;
          if (stackItem != null && stackItem.HasFileReference)
          {
            _editorContext.NavigateToFile(stackItem.SourceFile, stackItem.Line);
          }
        });
      }
    }

    public TestDetailsViewModel(IEditorContext editorContext)
    {
      _editorContext = editorContext;
    }
  }
}
