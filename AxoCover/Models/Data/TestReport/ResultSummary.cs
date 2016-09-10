using System.Xml.Serialization;

namespace AxoCover.Models.Data.TestReport
{
  public class ResultSummary
  {
    [XmlAttribute("outcome")]
    public TestState Outcome { get; set; }

    public Counters Counters { get; set; }
  }
}