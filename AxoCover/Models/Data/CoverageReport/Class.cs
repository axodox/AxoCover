namespace AxoCover.Models.Data.CoverageReport
{
  public class Class
  {
    public Summary Summary { get; set; }

    public string FullName { get; set; }

    public Method[] Methods { get; set; }
  }
}
