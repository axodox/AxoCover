using System;
using System.Windows;

namespace AxoCover.Controls
{
  internal static class SharedDictionaryManager
  {
    private static readonly ResourceDictionary _sharedDictionary;

    public static ResourceDictionary SharedDictionary
    {
      get
      {
        return _sharedDictionary;
      }
    }

    static SharedDictionaryManager()
    {
      var resourceLocater = new Uri("/AxoCover;component/Controls/Resources.xaml", UriKind.Relative);
      _sharedDictionary = (ResourceDictionary)Application.LoadComponent(resourceLocater);
    }
  }
}
