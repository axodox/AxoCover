using AxoCover.Models.Data;
using AxoCover.Models.Extensions;
using System;

namespace AxoCover.ViewModels
{
  public class ResultItemViewModel : CodeItemViewModel<ResultItemViewModel, ResultItem>
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

    public ResultItemViewModel(ResultItemViewModel parent, ResultItem resultItem)
      : base(parent, resultItem)
    {

    }

    protected override void AddChild(ResultItem testItem)
    {
      var child = new ResultItemViewModel(this, testItem);
      Children.OrderedAdd(child, (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.CodeItem.Name, b.CodeItem.Name));
    }
  }
}
