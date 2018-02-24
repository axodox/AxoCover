using AxoCover.Common.Models;
using System.Runtime.Serialization;

namespace AxoCover.Common.Runner
{
  [DataContract]
  public class TestAdapterOptions
  {
    [DataMember]
    public string AssemblyPath { get; set; }

    [DataMember]
    public string[] RedirectedAssemblies { get; set; } = new string[0];

    [DataMember]
    public string ExtensionUri { get; set; }

    [DataMember]
    public FileRedirectionOptions RedirectionOptions { get; set; } = FileRedirectionOptions.None;

    [DataMember]
    public bool IsRedirectingAssemblies { get; set; }
  }
}
