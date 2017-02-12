using AxoCover.Common.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System.Xml;
using System.Xml.Serialization;

namespace AxoCover.Runner
{
  public class RunSettings :
    IRunSettings,
    ISettingsProvider
  {
    [XmlIgnore]
    public string SettingsXml { get; private set; }

    public RunSettings()
    {
      SettingsXml = this.ToXml();
    }

    public RunSettings(string settingsXml)
    {
      if (settingsXml == null)
      {
        SettingsXml = this.ToXml();
      }
      else
      {
        SettingsXml = settingsXml;
      }
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
