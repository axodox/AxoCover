using System.Xml.Serialization;

namespace AxoCover.Models.Data.CoverageReport
{
  public class TrackedMethodRef
  {
    [XmlAttribute("uid")]
    public int Id { get; set; }

    [XmlAttribute("vc")]
    public int VisitCount { get; set; }
  }
}