using AxoCover.Models.Testing.Data;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;

namespace AxoCover.ViewModels
{
  public class CoverageItemViewModel : CodeItemViewModel<CoverageItemViewModel, CoverageItem>
  {
    public double ClassCoverage
    {
      get
      {
        if (CodeItem.Classes > 0)
        {
          return CodeItem.VisitedClasses * 100d / CodeItem.Classes;
        }
        else
        {
          return 100d;
        }
      }
    }

    public int UncoveredClasses
    {
      get
      {
        return CodeItem.Classes - CodeItem.VisitedClasses;
      }
    }

    public double MethodCoverage
    {
      get
      {
        if (CodeItem.Methods > 0)
        {
          return CodeItem.VisitedMethods * 100d / CodeItem.Methods;
        }
        else
        {
          return 100d;
        }
      }
    }

    public int UncoveredMethods
    {
      get
      {
        return CodeItem.Methods - CodeItem.VisitedMethods;
      }
    }

    public double SequenceCoverage
    {
      get
      {
        if (CodeItem.SequencePoints > 0)
        {
          return CodeItem.VisitedSequencePoints * 100d / CodeItem.SequencePoints;
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
        return CodeItem.SequencePoints - CodeItem.VisitedSequencePoints;
      }
    }

    public double BranchCoverage
    {
      get
      {
        if (CodeItem.BranchPoints > 0)
        {
          return CodeItem.VisitedBranchPoints * 100d / CodeItem.BranchPoints;
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
        return CodeItem.BranchPoints - CodeItem.VisitedBranchPoints;
      }
    }

    public double UncoveredBranchPointRatio
    {
      get
      {
        return Parent == null || Parent.Children.Count == 1 ?
          0d : (double)UncoveredBranchPoints / Parent.Children.Max(p => p.UncoveredBranchPoints);
      }
    }

    public string IconPath
    {
      get
      {
        return AxoCoverPackage.ResourcesPath + CodeItem.Kind + ".png";
      }
    }

    private CoverageItemOrder _ordering;
    public CoverageItemOrder Ordering
    {
      get { return _ordering; }
      set
      {
        _ordering = value;
        switch(value)
        {
          case CoverageItemOrder.Default:
            Children.Comparison = DefaultComparison;
            break;
          case CoverageItemOrder.Coverage:
            Children.Comparison = (a, b) => -Comparer.Default.Compare(a.BranchCoverage, b.BranchCoverage);
            break;
          case CoverageItemOrder.UncoveredItems:
            Children.Comparison = (a, b) => -Comparer.Default.Compare(a.UncoveredBranchPoints, b.UncoveredBranchPoints);
            break;
        }

        foreach(var child in Children)
        {
          child.Ordering = value;
        }

        NotifyPropertyChanged(nameof(Ordering));
      }
    }

    public CoverageItemViewModel(CoverageItemViewModel parent, CoverageItem coverageItem)
      : base(parent, coverageItem, CreateViewModel)
    {

    }

    private static CoverageItemViewModel CreateViewModel(CoverageItemViewModel parent, CoverageItem coverageItem)
    {
      return new CoverageItemViewModel(parent, coverageItem);
    }

    protected override void OnUpdated()
    {
      base.OnUpdated();
      NotifyPropertyChanged(nameof(ClassCoverage));
      NotifyPropertyChanged(nameof(UncoveredClasses));
      NotifyPropertyChanged(nameof(MethodCoverage));
      NotifyPropertyChanged(nameof(UncoveredMethods));
      NotifyPropertyChanged(nameof(SequenceCoverage));
      NotifyPropertyChanged(nameof(UncoveredSequencePoints));
      NotifyPropertyChanged(nameof(BranchCoverage));
      NotifyPropertyChanged(nameof(UncoveredBranchPoints));
      NotifyPropertyChanged(nameof(UncoveredBranchPointRatio));
    }

    public void Sort()
    {
      Children.Sort();

      foreach (var child in Children)
      {
        child.Sort();
      }
    }
  }

  public enum CoverageItemOrder
  {
    [Description("Alphabetical")]
    Default,
    [Description("Uncovered Code")]
    UncoveredItems,
    [Description("Coverage")]
    Coverage
  }
}
