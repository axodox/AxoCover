using AxoCover.Models;
using AxoCover.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace AxoCover.ViewModels
{
  public class TestListViewModel : ViewModel
  {
    private TestItemViewModel _testSolution;
    public TestItemViewModel TestSolution
    {
      get
      {
        return _testSolution;
      }
      set
      {
        if (_testSolution != null)
        {
          RemoveItems(_testSolution.Flatten(p => p.Children));
        }
        _testSolution = value;
        if (_testSolution != null)
        {
          AddItems(_testSolution.Flatten(p => p.Children));
        }
      }
    }

    private readonly ObservableCollection<TestItemViewModel> _testList;
    public OrderedFilteredCollection<TestItemViewModel> TestList
    {
      get;
      private set;
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
        TestList.ApplyFilter(p => p.TestItem.Name.ToLower().Contains(filterText));
      }
    }

    public TestListViewModel()
    {
      _testList = new ObservableCollection<TestItemViewModel>();
      TestList = new OrderedFilteredCollection<TestItemViewModel>(_testList, (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.TestItem.Name, b.TestItem.Name));
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
      {
        RemoveItems(e.OldItems.OfType<TestItemViewModel>());
      }

      if (e.NewItems != null)
      {
        AddItems(e.NewItems.OfType<TestItemViewModel>());
      }
    }

    private void RemoveItems(IEnumerable<TestItemViewModel> items)
    {
      foreach (var item in items)
      {
        item.Children.CollectionChanged -= OnCollectionChanged;
        _testList.Remove(item);
      }
    }

    private void AddItems(IEnumerable<TestItemViewModel> items)
    {
      foreach (var item in items)
      {
        _testList.Add(item);
        item.Children.CollectionChanged += OnCollectionChanged;
      }
    }
  }
}
