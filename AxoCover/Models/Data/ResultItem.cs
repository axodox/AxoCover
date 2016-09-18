using AxoCover.Models.Data.CoverageReport;

namespace AxoCover.Models.Data
{
  public class ResultItem : CodeItem<ResultItem>
  {
    public int SequencePoints { get; set; }
    public int VisitedSequencePoints { get; set; }
    public int BranchPoints { get; set; }
    public int VisitedBranchPoints { get; set; }

    public ResultItem(ResultItem parent,
      CodeItemKind kind, string name,
      Summary summary)
      : this(parent, kind, name,
          summary.SequencePoints, summary.VisitedSequencePoints,
          summary.BranchPoints, summary.VisitedBranchPoints)
    {

    }

    public ResultItem(ResultItem parent,
      CodeItemKind kind, string name)
      : this(parent, kind, name, 0, 0, 0, 0)
    {

    }

    public ResultItem(ResultItem parent,
      CodeItemKind kind, string name,
      int sequencePoints, int visitedSequencePoints,
      int branchPoints, int visitedBranchPoints)
      : base(parent, name, kind)
    {
      SequencePoints = sequencePoints;
      VisitedSequencePoints = visitedSequencePoints;
      BranchPoints = branchPoints;
      VisitedSequencePoints = visitedBranchPoints;
    }
  }
}
