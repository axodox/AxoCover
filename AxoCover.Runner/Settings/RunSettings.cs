using AxoCover.Common.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System.Xml;
using System.Xml.Serialization;

namespace AxoCover.Runner.Settings
{
  public class RunSettings :
    IRunSettings,
    ISettingsProvider
  {

    private string _settingsXml;
    [XmlIgnore]
    public string SettingsXml
    {
      get
      {
        return _settingsXml;
      }
      private set
      {
        _settingsXml = value;
      }
    }

    [XmlElement]
    public RunConfiguration Execution { get; set; }

    public RunSettings()
    {
      Execution = new RunConfiguration();
      RefreshConfiguration();
    }

    public RunSettings(string settingsXml)
      : this()
    {
      if (settingsXml != null)
      {
        SettingsXml = settingsXml;
      }
    }

    public void RefreshConfiguration()
    {
      SettingsXml = this.ToXml();
    }

    public ISettingsProvider GetSettings(string settingsName)
    {
      return this;
    }

    public void Load(XmlReader reader)
    {
      SettingsXml = reader.ToString();
    }
  }
}
