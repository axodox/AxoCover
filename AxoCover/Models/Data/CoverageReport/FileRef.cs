using System.Xml.Serialization;

namespace AxoCover.Models.Data.CoverageReport
{
  public class FileRef
  {
    [XmlAttribute("uid")]
    public int Id { get; set; }
  }
}
