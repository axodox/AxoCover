using System.Xml.Serialization;

namespace AxoCover.Models.Data.CoverageReport
{
  public class SequencePoint
  {
    [XmlAttribute("vc")]
    public int VisitCount { get; set; }

    [XmlAttribute("sl")]
    public int StartLine { get; set; }

    [XmlAttribute("sc")]
    public int StartColumn { get; set; }

    [XmlAttribute("el")]
    public int EndLine { get; set; }

    [XmlAttribute("ec")]
    public int EndColumn { get; set; }
  }
}
