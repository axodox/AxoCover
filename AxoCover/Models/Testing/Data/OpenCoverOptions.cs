using System.Collections.Generic;

namespace AxoCover.Models.Testing.Data
{
  public class OpenCoverOptions
  {
    public IEnumerable<string> CodeAssemblies { get; set; }
    public IEnumerable<string> TestAssemblies { get; set; }
    public string CoverageReportPath { get; set; }
    public bool IsCoveringByTest { get; set; }
    public bool IsIncludingSolutionAssemblies { get; set; }
    public bool IsExcludingTestAssemblies { get; set; }
    public bool IsMergingByHash { get; set; }
    public bool IsSkippingAutoProps { get; set; }
    public string ExcludeAttributes { get; set; }
    public string ExcludeFiles { get; set; }
    public string ExcludeDirectories { get; set; }
    public string Filters { get; set; }
    public bool IsVisitorCountLimited { get; set; }
    public int VisitorCountLimit { get; set; }
  }
}
