using AxoCover.Models.Data;
using AxoCover.Models.Extensions;
using System;

namespace AxoCover.ViewModels
{
  public class CoverageItemViewModel : CodeItemViewModel<CoverageItemViewModel, CoverageItem>
  {
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

    public string IconPath
    {
      get
      {
        return AxoCoverPackage.ResourcesPath + CodeItem.Kind + ".png";
      }
    }

    public CoverageItemViewModel(CoverageItemViewModel parent, CoverageItem coverageItem)
      : base(parent, coverageItem)
    {

    }

    protected override void AddChild(CoverageItem coverageItem)
    {
      var child = new CoverageItemViewModel(this, coverageItem);
      Children.OrderedAdd(child, (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.CodeItem.Name, b.CodeItem.Name));
    }

    protected override void OnUpdated()
    {
      base.OnUpdated();
      NotifyPropertyChanged(nameof(SequenceCoverage));
      NotifyPropertyChanged(nameof(UncoveredSequencePoints));
      NotifyPropertyChanged(nameof(BranchCoverage));
      NotifyPropertyChanged(nameof(UncoveredBranchPoints));
    }
  }
}
