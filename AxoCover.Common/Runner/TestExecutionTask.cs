using AxoCover.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
