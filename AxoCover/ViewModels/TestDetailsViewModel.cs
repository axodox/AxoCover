using AxoCover.Models;
using AxoCover.Models.Data;
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
        _selectedItem = value;
        NotifyPropertyChanged(nameof(SelectedItem));
        NotifyPropertyChanged(nameof(IsSelectionValid));
      }
    }

    public bool IsSelectionValid
    {
      get
      {
        return SelectedItem != null && (SelectedItem.CodeItem.Kind == CodeItemKind.Method);
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

    public ICommand NavigateToTestItemCommand
    {
      get
      {
        return new DelegateCommand(
          p =>
          {
            var testItem = SelectedItem.CodeItem;
            _editorContext.NavigateToMethod(testItem.GetParent<TestProject>().Name, testItem.Parent.FullName, testItem.Name);
          },
          p => IsSelectionValid,
          p => PropertyChanged += (o, e) =>
          {
            if (e.PropertyName == nameof(IsSelectionValid))
            {
              p();
            }
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
            var testItem = SelectedItem.CodeItem;
            _editorContext.NavigateToMethod(testItem.GetParent<TestProject>().Name, testItem.Parent.FullName, testItem.Name);
            _editorContext.DebugContextualTest();
          },
          p => IsSelectionValid,
          p => PropertyChanged += (o, e) =>
          {
            if (e.PropertyName == nameof(IsSelectionValid))
            {
              p();
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
