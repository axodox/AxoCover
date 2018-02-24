using System.Xml.Serialization;

namespace AxoCover.Models.Testing.Data.CoverageReport
{
  public class File : FileRef
  {
    [XmlAttribute("fullPath")]
    public string FullPath { get; set; }
  }
}
