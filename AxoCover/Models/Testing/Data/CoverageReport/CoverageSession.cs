using AxoCover.Common.Extensions;

namespace AxoCover.Models.Testing.Data.CoverageReport
{
  public class CoverageSession : IFileSource
  {
    public Summary Summary { get; set; }

    public Module[] Modules { get; set; }

    public string FilePath { get; set; }

    public CoverageSession()
    {
      Modules = new Module[0];
    }
  }
}
