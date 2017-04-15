using System;
using System.Runtime.Serialization;

namespace AxoCover.Common.Models
{
  [DataContract]
  [Flags]
  public enum TestPropertyAttributes
  {
    [EnumMember]
    None = 0,

    [EnumMember]
    Hidden = 1,

    [EnumMember]
    Immutable = 2,

    [EnumMember]
    Trait = 4
  }
}
