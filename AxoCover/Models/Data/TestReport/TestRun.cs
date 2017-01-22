using AxoCover.Models.Extensions;
using System.Xml.Serialization;

namespace AxoCover.Models.Data.TestReport
{
  //[XmlType(Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
  [XmlRoot("TestRun", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010", IsNullable = false)]
  public class TestRun : IFileSource
  {
    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlAttribute("runUser")]
    public string User { get; set; }

    public TestSettings TestSettings { get; set; }

    public Times Times { get; set; }

    public ResultSummary ResultSummary { get; set; }

    public UnitTest[] TestDefinitions { get; set; }

    public UnitTestResult[] Results { get; set; }

    public string FilePath { get; set; }
  }
}
