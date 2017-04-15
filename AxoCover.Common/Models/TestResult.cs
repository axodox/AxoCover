using System;
using System.Runtime.Serialization;

namespace AxoCover.Common.Models
{
  [DataContract]
  public class TestResult
  {
    [DataMember]
    public string ComputerName { get; set; }

    [DataMember]
    public string DisplayName { get; set; }

    [DataMember]
    public TimeSpan Duration { get; set; }

    [DataMember]
    public DateTimeOffset EndTime { get; set; }

    [DataMember]
    public string ErrorMessage { get; set; }

    [DataMember]
    public string ErrorStackTrace { get; set; }

    [DataMember]
    public TestOutcome Outcome { get; set; }

    [DataMember]
    public DateTimeOffset StartTime { get; set; }

    [DataMember]
    public TestCase TestCase { get; set; }
  }
}
