using System.Xml.Serialization;

namespace AxoCover.Models.Data.TestReport
{
  public class TestMethod
  {
    [XmlAttribute("codeBase")]
    public string CodeBase { get; set; }

    [XmlAttribute("className")]
    public string ClassName { get; set; }

    [XmlAttribute("name")]
    public string MethodName { get; set; }
  }
}
