using AxoCover.Models.Data;
using AxoCover.Models.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AxoCover.ViewModels
{
  public class ResultItemViewModel : ViewModel
  {
    public TestItemResult TestItemResult { get; private set; }

    public ResultItemViewModel Parent { get; private set; }

    public ObservableCollection<ResultItemViewModel> Children { get; private set; }

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

    public double SequenceCoverage
    {
      get
      {
        if (TestItemResult.SequencePoints > 0)
        {
          return TestItemResult.VisitedSequencePoints * 100d / TestItemResult.SequencePoints;
        }
        else
        {
          return 100d;
        }
      }
    }

    public int UncoveredSequencePoints
    {
      get
      {
        return TestItemResult.SequencePoints - TestItemResult.VisitedSequencePoints;
      }
    }

    public double BranchCoverage
    {
      get
      {
        if (TestItemResult.BranchPoints > 0)
        {
          return TestItemResult.VisitedBranchPoints * 100d / TestItemResult.BranchPoints;
        }
        else
        {
          return 100d;
        }
      }
    }

    public int UncoveredBranchPoints
    {
      get
      {
        return TestItemResult.BranchPoints - TestItemResult.VisitedBranchPoints;
      }
    }

    public ResultItemViewModel(ResultItemViewModel parent, TestItemResult testItemResult)
    {
      if (testItemResult == null)
        throw new ArgumentNullException(nameof(testItemResult));

      TestItemResult = testItemResult;
      Parent = parent;
      Children = new ObservableCollection<ResultItemViewModel>();
      foreach (var childItem in testItemResult.Children)
      {
        AddChild(childItem);
      }
    }

    public void UpdateItem(TestItemResult testItemResult)
    {
      TestItemResult = testItemResult;
      NotifyPropertyChanged(nameof(TestItem));

      var childrenToUpdate = Children.ToList();
      foreach (var childItem in testItemResult.Children)
      {
        var childToUpdate = childrenToUpdate.FirstOrDefault(p => p.TestItemResult == childItem);
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
    }

    private void AddChild(TestItemResult testItem)
    {
      var child = new ResultItemViewModel(this, testItem);
      Children.OrderedAdd(child, (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.TestItemResult.Name, b.TestItemResult.Name));
    }
  }
}
