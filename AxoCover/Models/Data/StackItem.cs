using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AxoCover.Models.Data
{
  public class StackItem
  {
    private const string _stackItemPattern = "at (?<method>[^(]*\\([^)]*\\))( in (?<file>(\\w:|\\\\\\\\)[^:]*):line (?<line>\\d+))?";

    private static readonly Regex _stackItemRegex = new Regex(_stackItemPattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    public static StackItem[] FromStackTrace(string stackTrace)
    {
      if (stackTrace == null)
        return new StackItem[0];

      var items = new List<StackItem>();
      foreach (Match stackItemMatch in _stackItemRegex.Matches(stackTrace))
      {
        var item = new StackItem()
        {
          Method = stackItemMatch.Groups["method"].Value,
          SourceFile = stackItemMatch.Groups["file"].Value,
          Line = stackItemMatch.Groups["line"].Success ? int.Parse(stackItemMatch.Groups["line"].Value) : 0
        };
        items.Add(item);
      }
      return items.ToArray();
    }

    public string Method { get; set; }

    public string SourceFile { get; set; }

    public int Line { get; set; }

    public bool HasFileReference
    {
      get
      {
        return !string.IsNullOrEmpty(SourceFile);
      }
    }
  }
}
