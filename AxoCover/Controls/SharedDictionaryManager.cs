using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace AxoCover.Controls
{
  internal static class SharedDictionaryManager
  {
    private static readonly List<ResourceDictionary> sharedResourceDictionaries = new List<ResourceDictionary>();

    static SharedDictionaryManager()
    {
      var resources = new[]
      {
        "/AxoCover;component/Controls/Styles.xaml",
        "/AxoCover;component/Controls/Icons.xaml"
      };

      foreach (var resource in resources)
      {
        sharedResourceDictionaries.Add((ResourceDictionary)Application.LoadComponent(new Uri(resource, UriKind.Relative)));
      }
    }

    public static void InitializeDictionaries(Collection<ResourceDictionary> resourceDictionaries)
    {
      foreach (var resourceDictionary in sharedResourceDictionaries)
      {
        resourceDictionaries.Add(resourceDictionary);
      }
    }
  }
}
