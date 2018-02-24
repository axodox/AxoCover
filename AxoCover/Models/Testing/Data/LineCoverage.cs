using System.Collections.Generic;

namespace AxoCover.Models.Testing.Data
{
  public class LineCoverage
  {
    public static readonly LineCoverage Empty = new LineCoverage(0, CoverageState.Unknown, CoverageState.Unknown, new bool[0][], new LineSection[0], new HashSet<TestMethod>(), new HashSet<TestMethod>[0][]);

    public int VisitCount { get; private set; }

    public CoverageState SequenceCoverageState { get; private set; }

    public CoverageState BranchCoverageState { get; private set; }

    public bool[][] BranchesVisited { get; private set; }

    public LineSection[] UncoveredSections { get; private set; }

    public HashSet<TestMethod> LineVisitors { get; private set; }

    public HashSet<TestMethod>[][] BranchVisitors { get; private set; }

    public LineCoverage(int visitCount, CoverageState sequenceCoverageState, CoverageState branchCoverageState, bool[][] branchesVisited, LineSection[] uncoveredSections, HashSet<TestMethod> lineVisitors, HashSet<TestMethod>[][] branchVisitors)
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
