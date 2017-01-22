using AxoCover.Models;
using AxoCover.Models.Data;
using System.Windows.Input;

namespace AxoCover.ViewModels
{
  public class CoverageDetailsViewModel : ViewModel
  {
    private IEditorContext _editorContext;

    private CoverageItemViewModel _selectedItem;
    public CoverageItemViewModel SelectedItem
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
        NotifyPropertyChanged(nameof(IsMethod));
        NotifyPropertyChanged(nameof(IsGroup));
        NotifyPropertyChanged(nameof(HasClasses));
        NotifyPropertyChanged(nameof(HasMethods));
      }
    }

    public bool IsSelectionValid
    {
      get
      {
        return SelectedItem != null;
      }
    }

    public bool IsMethod
    {
      get
      {
        return IsSelectionValid && SelectedItem.CodeItem.Kind == CodeItemKind.Method;
      }
    }

    public bool IsGroup
    {
      get
      {
        return IsSelectionValid && SelectedItem.CodeItem.Kind != CodeItemKind.Method;
      }
    }

    public bool HasClasses
    {
      get
      {
        return IsSelectionValid && SelectedItem.CodeItem.Kind != CodeItemKind.Method && SelectedItem.CodeItem.Kind != CodeItemKind.Class;
      }
    }

    public bool HasMethods
    {
      get
      {
        return IsSelectionValid && SelectedItem.CodeItem.Kind != CodeItemKind.Method;
      }
    }

    public ICommand NavigateToCoverageItemCommand
    {
      get
      {
        return new DelegateCommand(
          p =>
          {
            var testItem = SelectedItem.CodeItem;
            _editorContext.NavigateToMethod(testItem.GetParent(CodeItemKind.Project).Name, testItem.Parent.FullName, testItem.Name);
          },
          p => SelectedItem != null && (SelectedItem.CodeItem.Kind == CodeItemKind.Method || SelectedItem.CodeItem.Kind == CodeItemKind.Class),
          p => ExecuteOnPropertyChange(p, nameof(SelectedItem)));
      }
    }

    public CoverageDetailsViewModel(IEditorContext editorContext)
    {
      _editorContext = editorContext;
    }
  }
}
