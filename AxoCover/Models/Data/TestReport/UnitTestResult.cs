using System;
using System.Xml.Serialization;

namespace AxoCover.Models.Data.TestReport
{
  public class UnitTestResult
  {
    [XmlAttribute("testId")]
    public string TestId { get; set; }

    [XmlAttribute("testName")]
    public string TestName { get; set; }

    [XmlAttribute("startTime")]
    public DateTime StartTime { get; set; }

    [XmlAttribute("endTime")]
    public DateTime EndTime { get; set; }

    [XmlIgnore]
    public TimeSpan Duration
    {
      get
      {
        return EndTime - StartTime;
      }
    }

    [XmlAttribute("outcome")]
    public TestState Outcome { get; set; }

    [XmlElement("Output", typeof(Output))]
    public object[] Items { get; set; }
  }
}