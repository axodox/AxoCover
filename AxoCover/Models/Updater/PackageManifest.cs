using System.IO;
using System.Xml.Linq;

namespace AxoCover.Models.Updater
{
  public class PackageManifest
  {
    public string Name { get; private set; }

    public string Description { get; private set; }

    public string Version { get; private set; }

    public string Publisher { get; private set; }

    public string Icon { get; private set; }

    public string License { get; private set; }

    public string ReleaseNotes { get; private set; }

    public string WebSite { get; private set; }

    public static PackageManifest FromFile(string path)
    {
      var root = Path.GetDirectoryName(path);
      var document = XDocument.Load(path);

      var namePrefix = "{http://schemas.microsoft.com/developer/vsx-schema/2011}";
      var manifestNode = document.Element(namePrefix + "PackageManifest");
      var metadataNode = manifestNode.Element(namePrefix + "Metadata");
      var identityNode = metadataNode.Element(namePrefix + "Identity");

      var manifest = new PackageManifest()
      {
        Name = metadataNode.Element(namePrefix + "DisplayName").Value,
        Description = metadataNode.Element(namePrefix + "Description").Value,
        Version = identityNode.Attribute("Version").Value,
        Publisher = identityNode.Attribute("Publisher").Value,
        Icon = metadataNode.Element(namePrefix + "Icon").Value,
        License = File.ReadAllText(Path.Combine(root, metadataNode.Element(namePrefix + "License").Value)),
        ReleaseNotes = File.ReadAllText(Path.Combine(root, metadataNode.Element(namePrefix + "ReleaseNotes").Value)),
        WebSite = metadataNode.Element(namePrefix + "MoreInfo").Value
      };

      return manifest;
    }
  }
}
