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

    private readonly ObservableCollection<T> _children = new ObservableCollection<T>();
    public ObservableCollection<T> Children
    {
      get
      {
        return _children;
      }
    }

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

    public bool HasChildren
    {
      get
      {
        return Children.Count > 0;
      }
    }

    public bool CanGoToSource
    {
      get
      {
        return CodeItem.Kind == CodeItemKind.Data || CodeItem.Kind == CodeItemKind.Method || CodeItem.Kind == CodeItemKind.Class;
      }
    }

    public bool IsFlattened
    {
      get
      {
        return CodeItem.Kind == CodeItemKind.Namespace && Children.Count == 1 && Children[0].CodeItem.Kind == CodeItemKind.Namespace;
      }
    }

    public string FlattenedName
    {
      get
      {
        return Parent != null && Parent.IsFlattened ? Parent.FlattenedName + "." + CodeItem.DisplayName : CodeItem.DisplayName;
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
      if (Parent != null)
      {
        Parent.Children.OrderedAdd(this as T, (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.CodeItem.DisplayName, b.CodeItem.DisplayName));
      }
      _isExpanded = parent == null;
      Children.CollectionChanged += OnChildrenChanged;
      foreach (var childItem in codeItem.Children)
      {
        _viewModelFactory(this as T, childItem);
      }
    }

    private void OnChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      NotifyPropertyChanged(nameof(HasChildren));
      NotifyPropertyChanged(nameof(IsFlattened));
      if (e.OldItems != null)
      {
        foreach (CodeItemViewModel<T, U> child in e.OldItems)
        {
          child.OnRemoved();
        }
      }

      if (e.NewItems != null && e.NewItems.OfType<T>().Any(p => p.Parent != this))
      {
        throw new InvalidOperationException("The children added must correspond to this object.");
      }
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
          _viewModelFactory(this as T, childItem);
        }
      }

      foreach (var childToDelete in childrenToUpdate)
      {
        childToDelete.Remove();
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

    public T FindChild(CodeItem<U> codeItem)
    {
      if (codeItem == null)
        throw new ArgumentNullException(nameof(codeItem));

      var itemPath = codeItem
        .Crawl(p => p.Parent, true)
        .TakeWhile(p => CodeItem != codeItem)
        .Reverse()
        .Skip(1)
        .ToArray();

      var codeItemViewModel = this as T;
      foreach (var part in itemPath)
      {
        codeItemViewModel = codeItemViewModel.Children.FirstOrDefault(p => p.CodeItem == part);
        if (codeItemViewModel == null)
        {
          break;
        }
      }

      return codeItemViewModel;
    }

    public void Remove()
    {
      Parent.Children.Remove(this as T);
      Parent = null;
    }

    protected virtual void OnRemoved()
    {
      foreach (var child in Children)
      {
        child.OnRemoved();
      }
    }
  }
}
