using System.Xml.Serialization;

namespace AxoCover.Models.Testing.Data.CoverageReport
{
  public class BranchPoint
  {
    [XmlAttribute("vc")]
    public int VisitCount { get; set; }

    [XmlAttribute("sl")]
    public int StartLine { get; set; }

    [XmlAttribute("path")]
    public int Path { get; set; }

    [XmlAttribute("offset")]
    public int Offset { get; set; }

    public TrackedMethodRef[] TrackedMethodRefs { get; set; }

    public BranchPoint()
    {
      TrackedMethodRefs = new TrackedMethodRef[0];
    }
  }
}
