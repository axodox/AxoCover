using System.Xml.Serialization;

namespace AxoCover.Models.Data.TestReport
{
  public class ResultSummary
  {
    [XmlAttribute("outcome")]
    public string Outcome { get; set; }

    public Counters Counters { get; set; }
  }
}