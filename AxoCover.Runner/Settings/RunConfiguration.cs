using System.Xml.Serialization;

namespace AxoCover.Runner.Settings
{
  public class RunConfiguration
  {
    [XmlElement]
    public int MaxCpuCount { get; set; }

    public RunConfiguration()
    {
      MaxCpuCount = 1;
    }
  }
}
