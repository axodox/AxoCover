namespace AxoCover.Models.Data.CoverageReport
{
  public class Method
  {
    public Summary Summary { get; set; }

    public string Name { get; set; }

    public FileRef FileRef { get; set; }

    public SequencePoint[] SequencePoints { get; set; }
  }
}
