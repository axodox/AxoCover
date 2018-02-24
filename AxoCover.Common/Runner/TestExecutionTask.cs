using AxoCover.Common.Models;
using System.Runtime.Serialization;

namespace AxoCover.Common.Runner
{
  [DataContract]
  public class TestExecutionTask
  {
    [DataMember]
    public TestCase[] TestCases { get; set; }

    [DataMember]
    public TestAdapterOptions TestAdapterOptions { get; set; }
  }
}
