namespace AxoCover.Models.Data
{
  public class LineSection
  {
    public int Start { get; private set; }

    public int End { get; private set; }

    public LineSection(int start, int end)
    {
      Start = start;
      End = end;
    }
  }
}
