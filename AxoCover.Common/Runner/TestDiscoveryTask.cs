using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
