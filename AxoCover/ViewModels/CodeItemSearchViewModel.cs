using AxoCover.Common.Extensions;
using AxoCover.Models.Testing.Data;
using AxoCover.Models.Ui;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace AxoCover.ViewModels
{
  public class CodeItemSearchViewModel<T, U> : ViewModel
    where T : CodeItemViewModel<T, U>
    where U : CodeItem<U>
  {
    private readonly ObservableCollection<T> _codeItemList;
    public OrderedFilteredCollection<T> CodeItemList
    {
      get;
      private set;
    }

    private T _solution;
    public T Solution
    {
      get
      {
        return _solution;
      }
      set
      {
        if (_solution != null)
        {
          _solution.Children.CollectionChanged -= OnTestItemCollectionChanged;
          RemoveItems(_solution.Flatten(p => p.Children, false));
        }
        _solution = value;
        if (_solution != null)
        {
          AddItems(_solution.Flatten(p => p.Children, false));
          _solution.Children.CollectionChanged += OnTestItemCollectionChanged;
        }
        NotifyPropertyChanged(nameof(Solution));
      }
    }

    private string _filterText = string.Empty;
    public string FilterText
    {
      get
      {
        return _filterText;
      }
      set
      {
        _filterText = value ?? string.Empty;
        NotifyPropertyChanged(nameof(FilterText));
        var filterText = _filterText.ToLower();
        CodeItemList.ApplyFilter(p => p.CodeItem.DisplayName.ToLower().Contains(filterText));
      }
    }

    private bool _isResultTrimmed;
    public bool IsResultTrimmed
    {
      get { return _isResultTrimmed; }
      private set
      {
        if (_isResultTrimmed != value)
        {
          _isResultTrimmed = value;
          NotifyPropertyChanged(nameof(IsResultTrimmed));
        }
      }
    }

    public int ResultLimit
    {
      get { return 100; }
    }

    public CodeItemSearchViewModel()
    {
      _codeItemList = new ObservableCollection<T>();
      CodeItemList = new OrderedFilteredCollection<T>(_codeItemList, (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.CodeItem.DisplayName, b.CodeItem.DisplayName));
      CodeItemList.ResultLimit = ResultLimit;
      CodeItemList.CollectionChanged += OnResultsChanged;
    }

    private void OnResultsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      IsResultTrimmed = CodeItemList.Count >= CodeItemList.ResultLimit;
    }

    private void OnTestItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
      {
        RemoveItems(e.OldItems.OfType<T>().SelectMany(p => p.Flatten(q => q.Children)));
      }

      if (e.NewItems != null)
      {
        AddItems(e.NewItems.OfType<T>().SelectMany(p => p.Flatten(q => q.Children)));
      }
    }

    private void RemoveItems(IEnumerable<T> items)
    {
      foreach (var item in items)
      {
        if (_codeItemList.Remove(item))
        {
          item.Children.CollectionChanged -= OnTestItemCollectionChanged;
        }
      }
    }

    private void AddItems(IEnumerable<T> items)
    {
      foreach (var item in items)
      {
        _codeItemList.Add(item);
        item.Children.CollectionChanged += OnTestItemCollectionChanged;
      }
    }
  }
}
