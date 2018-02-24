using AxoCover.Models.Testing.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace AxoCover.Models.Extensions
{
  public static class ModelExtensions
  {
    private const string _typeArgumentPattern = @"(?><(?>[^<>]|(?<n><)|(?<-n>>))*>(?(n)(?!)))";

    private static readonly Regex _pathRegex = new Regex(@"(?:^|[.+])(?>[^.+()<>]+|" + _typeArgumentPattern + @"|\((?>[^'""()]|'.'|""(?>[^""]|(?<=\\)"")*"")*\))+", RegexOptions.Compiled);

    private static readonly Regex _typeArgumentRegex = new Regex(_typeArgumentPattern, RegexOptions.Compiled);

    private static readonly Regex _nameRegex = new Regex(@"^[.+]?\w*" + _typeArgumentPattern + @"?", RegexOptions.Compiled);

    public static bool IsTest(this TestItem testItem)
    {
      return (testItem.Kind == CodeItemKind.Method && !testItem.Children.Any()) || testItem.Kind == CodeItemKind.Data;
    }

    public static string[] SplitPath(this string path, bool includeDot = true)
    {
      return _pathRegex
        .Matches(path)
        .OfType<Match>()
        .Select(p => includeDot ? p.Value : p.Value.TrimStart('.'))
        .ToArray();
    }

    public static string CleanPath(this string path, bool removeGenericEnding = false)
    {
      var pathParts = path.SplitPath().Select(p => p.CleanName()).ToArray();
      if (removeGenericEnding)
      {
        pathParts[pathParts.Length - 1] = RemoveGenericEnding(pathParts[pathParts.Length - 1]);
      }
      return string.Join(string.Empty, pathParts);
    }

    private static string RemoveGenericEnding(string name)
    {
      if (name.IndexOf('<') != -1)
      {
        return name.Substring(0, name.IndexOf('<'));
      }

      if (name.IndexOf('`') != -1)
      {
        return name.Substring(0, name.IndexOf('`'));
      }

      return name;
    }

    public static string CleanName(this string name, bool removeGenericEnding = false)
    {
      name = _nameRegex.Match(name).Value;
      if (removeGenericEnding && name.IndexOf('<') != -1)
      {
        name = RemoveGenericEnding(name);
      }
      else
      {
        name = _typeArgumentRegex.Replace(name, p => "`" + (p.Value.Where(q => q == ',').Count() + 1));
      }
      return name;
    }
  }
}
