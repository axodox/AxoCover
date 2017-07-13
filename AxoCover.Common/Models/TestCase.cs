using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AxoCover.Common.Models
{
  [DataContract]
  public class TestCase
  {
    [DataMember]
    public string CodeFilePath { get; set; }

    [DataMember]
    public string DisplayName { get; set; }

    [DataMember]
    public Uri ExecutorUri { get; set; }

    [DataMember]
    public string FullyQualifiedName { get; set; }

    [DataMember]
    public Guid Id { get; set; }

    [DataMember]
    public int LineNumber { get; set; }

    [DataMember]
    public string Source { get; set; }

    [DataMember]
    public List<TestProperty> AdditionalProperties { get; set; }
  }
}
