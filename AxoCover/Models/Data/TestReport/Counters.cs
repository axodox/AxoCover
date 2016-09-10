using System.Xml.Serialization;

namespace AxoCover.Models.Data.TestReport
{
  public class Counters
  {
    [XmlAttribute("total")]
    public int Total { get; set; }

    [XmlAttribute("executed")]
    public int Executed { get; set; }

    [XmlAttribute("passed")]
    public int Passed { get; set; }

    [XmlAttribute("error")]
    public int Error { get; set; }

    [XmlAttribute("failed")]
    public int Failed { get; set; }

    [XmlAttribute("timeout")]
    public int Timeout { get; set; }

    [XmlAttribute("aborted")]
    public int Aborted { get; set; }

    [XmlAttribute("inconclusive")]
    public int Inconclusive { get; set; }

    [XmlAttribute("passedButRunAborted")]
    public int PassedButRunAborted { get; set; }

    [XmlAttribute("notRunnable")]
    public int NotRunnable { get; set; }

    [XmlAttribute("notExecuted")]
    public int NotExecuted { get; set; }

    [XmlAttribute("disconnected")]
    public int Disconnected { get; set; }

    [XmlAttribute("warning")]
    public int Warning { get; set; }

    [XmlAttribute("completed")]
    public int Completed { get; set; }

    [XmlAttribute("inProgress")]
    public int InProgress { get; set; }

    [XmlAttribute("pending")]
    public int Pending { get; set; }
  }
}