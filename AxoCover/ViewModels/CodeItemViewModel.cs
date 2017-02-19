using AxoCover.Common.Extensions;
using AxoCover.Models.Data;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

namespace AxoCover.ViewModels
{
  public abstract class CodeItemViewModel<T, U> : ViewModel
    where T : CodeItemViewModel<T, U>
    where U : CodeItem<U>
  {
    private U _codeItem;
    public U CodeItem
    {
      get
      {
        return _codeItem;
      }
      private set
      {
        _codeItem = value;
        NotifyPropertyChanged(nameof(CodeItem));
      }
    }

    public T Parent { get; private set; }

    public ObservableCollection<T> Children { get; private set; }

    private Func<T, U, T> _viewModelFactory;

    private bool _isExpanded;
    public bool IsExpanded
    {
      get
      {
        return _isExpanded;
      }
      set
      {
        _isExpanded = value;
        NotifyPropertyChanged(nameof(IsExpanded));
        if (Children.Count == 1)
        {
          Children.First().IsExpanded = value;
        }
      }
    }

    private bool _isSelected;
    public bool IsSelected
    {
      get
      {
        return _isSelected;
      }
      set
      {
        _isSelected = value;
        NotifyPropertyChanged(nameof(IsSelected));
      }
    }

    public bool CanGoToSource
    {
      get
      {
        return CodeItem.Kind == CodeItemKind.Data || CodeItem.Kind == CodeItemKind.Method || CodeItem.Kind == CodeItemKind.Class;
      }
    }

    public ICommand ToggleExpansionCommand
    {
      get
      {
        return new DelegateCommand(p => IsExpanded = !IsExpanded);
      }
    }

    public CodeItemViewModel(T parent, U codeItem, Func<T, U, T> viewModelFactory)
    {
      if (codeItem == null)
        throw new ArgumentNullException(nameof(codeItem));

      if (viewModelFactory == null)
        throw new ArgumentNullException(nameof(viewModelFactory));

      _viewModelFactory = viewModelFactory;

      CodeItem = codeItem;
      Parent = parent;
      _isExpanded = parent == null;
      Children = new ObservableCollection<T>();
      Children.CollectionChanged += OnChildrenChanged;
      foreach (var childItem in codeItem.Children)
      {
        AddChild(childItem);
      }      
    }

    private void OnChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
      {
        foreach (CodeItemViewModel<T, U> child in e.OldItems)
        {
          child.OnRemoved();
        }
      }
    }

    private void AddChild(U childItem)
    {
      var child = _viewModelFactory(this as T, childItem);
      Children.OrderedAdd(child, (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.CodeItem.Name, b.CodeItem.Name));
    }

    public void UpdateItem(U codeItem)
    {
      CodeItem = codeItem;
      NotifyPropertyChanged(nameof(CodeItem));

      var childrenToUpdate = Children.ToList();
      foreach (var childItem in codeItem.Children)
      {
        var childToUpdate = childrenToUpdate.FirstOrDefault(p => p.CodeItem == childItem);
        if (childToUpdate != null)
        {
          childToUpdate.UpdateItem(childItem);
          childrenToUpdate.Remove(childToUpdate);
        }
        else
        {
          AddChild(childItem);
        }
      }

      foreach (var childToDelete in childrenToUpdate)
      {
        Children.Remove(childToDelete);
      }

      OnUpdated();
    }

    protected virtual void OnUpdated() { }

    public void CollapseAll()
    {
      IsExpanded = false;
      foreach (var child in Children)
      {
        child.CollapseAll();
      }
    }

    public void ExpandAll()
    {
      IsExpanded = true;
      foreach (var child in Children)
      {
        child.ExpandAll();
      }
    }

    public void ExpandParents()
    {
      foreach (var parent in this.Crawl(p => p.Parent))
      {
        parent.IsExpanded = true;
      }
    }

    public T FindChild(string fullName)
    {
      var itemPath = fullName.Split('.');

      var itemName = string.Empty;
      var codeItemViewModel = this as T;
      foreach (var part in itemPath)
      {
        if (itemName != string.Empty)
        {
          itemName += ".";
        }
        itemName += part;

        var childItem = codeItemViewModel.Children.FirstOrDefault(p => p.CodeItem.Name == itemName);

        if (childItem != null)
        {
          itemName = string.Empty;
          codeItemViewModel = childItem;
        }
      }

      if (codeItemViewModel != null && itemName == string.Empty)
      {
        return codeItemViewModel;
      }
      else
      {
        return null;
      }
    }

    protected virtual void OnRemoved()
    {

    }
  }
}
