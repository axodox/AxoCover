using System.Xml.Serialization;

namespace AxoCover.Models.Data.CoverageReport
{
  public class File : FileRef
  {
    [XmlAttribute("fullPath")]
    public string FullPath { get; set; }
  }
}
