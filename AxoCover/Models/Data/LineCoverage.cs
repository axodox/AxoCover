using System.Collections.Generic;

namespace AxoCover.Models.Data
{
  public class LineCoverage
  {
    public static readonly LineCoverage Empty = new LineCoverage(0, CoverageState.Unknown, CoverageState.Unknown, new bool[0][], new LineSection[0], new Dictionary<string, int>(), new HashSet<string>[0][]);

    public int VisitCount { get; private set; }

    public CoverageState SequenceCoverageState { get; private set; }

    public CoverageState BranchCoverageState { get; private set; }

    public bool[][] BranchesVisited { get; private set; }

    public LineSection[] UncoveredSections { get; private set; }

    public Dictionary<string, int> LineVisitors { get; private set; }

    public HashSet<string>[][] BranchVisitors { get; private set; }

    public LineCoverage(int visitCount, CoverageState sequenceCoverageState, CoverageState branchCoverageState, bool[][] branchesVisited, LineSection[] uncoveredSections, Dictionary<string, int> lineVisitors, HashSet<string>[][] branchVisitors)
    {
      VisitCount = visitCount;
      SequenceCoverageState = sequenceCoverageState;
      BranchCoverageState = branchCoverageState;
      BranchesVisited = branchesVisited;
      UncoveredSections = uncoveredSections;
      LineVisitors = lineVisitors;
      BranchVisitors = branchVisitors;
    }
  }
}
