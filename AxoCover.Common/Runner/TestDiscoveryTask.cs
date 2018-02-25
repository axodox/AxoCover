using System.Runtime.Serialization;

namespace AxoCover.Common.Runner
{
  [DataContract]
  public class TestDiscoveryTask
  {
    [DataMember]
    public string[] TestAssemblyPaths { get; set; }

    [DataMember]
    public TestAdapterOptions TestAdapterOptions { get; set; }
  }
}
