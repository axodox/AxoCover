using AxoCover.Common.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AxoCover.Models.Updater
{
  public class Release
  {
    public string Branch { get; private set; }

    public Version Version { get; private set; }

    public string Uri { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public string Description { get; private set; }

    public Dictionary<string, string> Properties { get; private set; }

    public string MergedTo
    {
      get
      {
        return Properties.TryGetValue("MergedTo");
      }
    }

    public string AlternativeUri
    {
      get
      {
        return Properties.TryGetValue("Uri");
      }
    }

    [JsonConstructor]
    public Release(string branch, Version version, string uri, DateTime createdAt, string description, Dictionary<string, string> properties)
    {
      Branch = branch;
      Version = version;
      Uri = uri;
      CreatedAt = createdAt;
      Description = description;
      Properties = properties;
    }

    public override string ToString()
    {
      return Branch + "-" + Version;
    }
  }
}
