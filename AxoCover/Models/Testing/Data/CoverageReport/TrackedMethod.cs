using System.Xml.Serialization;

namespace AxoCover.Models.Testing.Data.CoverageReport
{
  public class TrackedMethod
  {
    [XmlAttribute("uid")]
    public int Id { get; set; }

    [XmlAttribute("name")]
    public string Name { get; set; }
  }
}
