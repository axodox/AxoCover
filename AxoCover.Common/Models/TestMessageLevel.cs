using System.Runtime.Serialization;

namespace AxoCover.Common.Models
{
  [DataContract]
  public enum TestMessageLevel
  {
    [EnumMember]
    Informational,

    [EnumMember]
    Warning,

    [EnumMember]
    Error
  }
}
