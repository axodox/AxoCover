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

    private readonly ObservableCollection<TestItemViewModel> _testList = new ObservableCollection<TestItemViewModel>();
    public ObservableCollection<TestItemViewModel> TestList
    {
      get
      {
        return _testList;
      }
    }

    private void AddItems(IEnumerable<TestItemViewModel> items)
    {
      foreach (var item in items)
      {
        _testList.OrderedAdd(item, p => p.TestItem.Name, StringComparer.OrdinalIgnoreCase);
        item.Children.CollectionChanged += OnCollectionChanged;
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
  }
}
