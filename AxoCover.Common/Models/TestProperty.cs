using System.Runtime.Serialization;

namespace AxoCover.Common.Models
{
  [DataContract]
  public class TestProperty
  {
    [DataMember]
    public TestPropertyAttributes Attributes { get; set; }

    [DataMember]
    public string Category { get; set; }

    [DataMember]
    public string Description { get; set; }

    [DataMember]
    public string Id { get; set; }

    [DataMember]
    public string Label { get; set; }

    [DataMember]
    public string ValueType { get; set; }

    [DataMember]
    public string Value { get; set; }
  }
}
