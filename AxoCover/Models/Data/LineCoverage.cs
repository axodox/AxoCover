namespace AxoCover.Models.Data
{
  public class LineCoverage
  {
    public static readonly LineCoverage Empty = new LineCoverage(0, CoverageState.Unknown);

    public int VisitCount { get; private set; }

    public CoverageState State { get; private set; }

    public LineCoverage(int visitCount, CoverageState state)
    {
      VisitCount = visitCount;
      State = state;
    }
  }
}
