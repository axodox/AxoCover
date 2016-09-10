using System.Xml.Serialization;

namespace AxoCover.Models.Data.TestReport
{
  public class UnitTest
  {
    [XmlAttribute("id")]
    public string Id { get; set; }

    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlAttribute("storage")]
    public string Storage { get; set; }

    public TestMethod TestMethod { get; set; }
  }
}