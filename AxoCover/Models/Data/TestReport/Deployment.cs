using System.Xml.Serialization;

namespace AxoCover.Models.Data.TestReport
{
  public class Deployment
  {
    [XmlAttribute("userDeploymentRoot")]
    public string UserDeploymentRoot { get; set; }

    [XmlAttribute("runDeploymentRoot")]
    public string RunDeploymentRoot { get; set; }
  }
}