namespace AxoCover.Models.Testing.Data.CoverageReport
{
  public class Class
  {
    public Summary Summary { get; set; }

    public string FullName { get; set; }

    public Method[] Methods { get; set; }

    public Class()
    {
      Methods = new Method[0];
    }
  }
}
