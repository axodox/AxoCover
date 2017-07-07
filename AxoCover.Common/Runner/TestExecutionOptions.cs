using AxoCover.Common.Settings;
using System.Runtime.Serialization;

namespace AxoCover.Common.Runner
{
  [DataContract]
  public class TestExecutionOptions
  {
    [DataMember]
    public string SolutionPath { get; set; }

    [DataMember]
    public string OutputPath { get; set; }
    
    [DataMember]
    public string RunSettingsPath { get; set; }

    [DataMember]
    public TestApartmentState ApartmentState { get; set; }
  }
}
