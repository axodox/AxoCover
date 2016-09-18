using AxoCover.Models.Data.CoverageReport;
using AxoCover.Models.Extensions;

namespace AxoCover.Models.Data
{
  public class CoverageItem : CodeItem<CoverageItem>
  {
    public int SequencePoints { get; private set; }
    public int VisitedSequencePoints { get; private set; }
    public int BranchPoints { get; private set; }
    public int VisitedBranchPoints { get; private set; }

    public string SourceFile { get; set; }
    public int SourceLine { get; set; }

    public CoverageItem(CoverageItem parent,
      string name, CodeItemKind kind,
      Summary summary)
      : this(parent, name, kind,
        summary.SequencePoints, summary.VisitedSequencePoints,
        summary.BranchPoints, summary.VisitedBranchPoints)
    {

    }

    public CoverageItem(CoverageItem parent,
      string name, CodeItemKind kind)
      : this(parent, name, kind, 0, 0, 0, 0)
    {

    }

    public CoverageItem(CoverageItem parent,
      string name, CodeItemKind kind,
      int sequencePoints, int visitedSequencePoints,
      int branchPoints, int visitedBranchPoints)
      : base(parent, name, kind)
    {
      SequencePoints = sequencePoints;
      VisitedSequencePoints = visitedSequencePoints;
      BranchPoints = branchPoints;
      VisitedBranchPoints = visitedBranchPoints;

      foreach (var parentItem in this.Crawl(p => p.Parent))
      {
        parentItem.SequencePoints += SequencePoints;
        parentItem.VisitedSequencePoints += VisitedSequencePoints;
        parentItem.BranchPoints += BranchPoints;
        parentItem.VisitedBranchPoints += VisitedBranchPoints;
      }
    }
  }
}
