using AxoCover.Common.Extensions;
using AxoCover.Models.Data.CoverageReport;

namespace AxoCover.Models.Data
{
  public sealed class CoverageItem : CodeItem<CoverageItem>
  {
    public int Classes { get; private set; }
    public int VisitedClasses { get; private set; }
    public int Methods { get; private set; }
    public int VisitedMethods { get; private set; }
    public int SequencePoints { get; private set; }
    public int VisitedSequencePoints { get; private set; }
    public int BranchPoints { get; private set; }
    public int VisitedBranchPoints { get; private set; }

    public string SourceFile { get; set; }
    public int SourceLine { get; set; }

    public CoverageItem(CoverageItem parent,
      string name, CodeItemKind kind,
      Summary summary, string displayName = null)
      : this(parent, name, kind,
          summary.Classes, summary.VisitedClasses,
          summary.Methods, summary.VisitedMethods,
          summary.SequencePoints, summary.VisitedSequencePoints,
          summary.BranchPoints, summary.VisitedBranchPoints)
    {
      DisplayName = displayName ?? name;
    }

    public CoverageItem(CoverageItem parent,
      string name, CodeItemKind kind)
      : this(parent, name, kind, 0, 0, 0, 0, 0, 0, 0, 0)
    {

    }

    private CoverageItem(CoverageItem parent,
      string name, CodeItemKind kind,
      int classes, int visitedClasses,
      int methods, int visitedMethods,
      int sequencePoints, int visitedSequencePoints,
      int branchPoints, int visitedBranchPoints)
      : base(parent, name, kind)
    {
      switch (Kind)
      {
        case CodeItemKind.Class:
          Classes = classes;
          VisitedClasses = visitedClasses;
          break;
        case CodeItemKind.Method:
          Methods = methods;
          VisitedMethods = visitedMethods;
          SequencePoints = sequencePoints;
          VisitedSequencePoints = visitedSequencePoints;
          BranchPoints = branchPoints;
          VisitedBranchPoints = visitedBranchPoints;
          break;
      }

      foreach (var parentItem in this.Crawl(p => p.Parent))
      {
        if (parentItem.Kind != CodeItemKind.Method)
        {
          parentItem.Methods += Methods;
          parentItem.VisitedMethods += VisitedMethods;
          parentItem.SequencePoints += SequencePoints;
          parentItem.VisitedSequencePoints += VisitedSequencePoints;
          parentItem.BranchPoints += BranchPoints;
          parentItem.VisitedBranchPoints += VisitedBranchPoints;

          if (parentItem.Kind != CodeItemKind.Class)
          {
            parentItem.Classes += Classes;
            parentItem.VisitedClasses += VisitedClasses;
          }
        }
      }
    }
  }
}
