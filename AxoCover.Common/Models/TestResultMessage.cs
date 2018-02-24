using System.Runtime.Serialization;

namespace AxoCover.Common.Models
{
  [DataContract]
  public class TestResultMessage
  {
    [DataMember]
    public string Category { get; set; }

    [DataMember]
    public string Text { get; set; }
  }
}
