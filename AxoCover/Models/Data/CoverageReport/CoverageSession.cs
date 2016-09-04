namespace AxoCover.Models.Data.CoverageReport
{
  public class CoverageSession
  {
    public Module[] Modules { get; set; }

    public CoverageSession()
    {
      Modules = new Module[0];
    }
  }
}
