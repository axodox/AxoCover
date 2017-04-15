using System.Runtime.Serialization;

namespace AxoCover.Common.Models
{
  [DataContract]
  public enum TestOutcome
  {
    [EnumMember]
    None,

    [EnumMember]
    Passed,

    [EnumMember]
    Failed,

    [EnumMember]
    Skipped,

    [EnumMember]
    NotFound
  }
}
