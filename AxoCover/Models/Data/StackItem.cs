using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AxoCover.Models.Data
{
  public class StackItem
  {
    private const string _methodPattern = @"[\w\.<>`\[\],]+\([^\)]*\)";
    private static readonly Regex _methodRegex = new Regex(_methodPattern, RegexOptions.Compiled);

    private const string _filePathPattern = @"(?:[a-zA-Z]:|\\)(?:\\[^<>:""\/\\|\?\*]*)+\.\w+";
    private static readonly Regex _filePathRegex = new Regex(_filePathPattern, RegexOptions.Compiled);

    private const string _lineNumberPattern = @"\d+";
    private static readonly Regex _lineNumberRegex = new Regex(_lineNumberPattern, RegexOptions.Compiled);

    public static StackItem[] FromStackTrace(string stackTrace)
    {
      if (stackTrace == null)
        return new StackItem[0];

      var items = new List<StackItem>();
      var lines = stackTrace.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
      foreach (var line in lines)
      {
        var methodMatch = _methodRegex.Match(line);
        if (!methodMatch.Success) continue;

        var item = new StackItem() { Method = methodMatch.Value };

        var filePathMatch = _filePathRegex.Match(line);
        if (filePathMatch.Success)
        {
          item.SourceFile = filePathMatch.Value;

          var lineNumberMatch = _lineNumberRegex.Match(line.Substring(filePathMatch.Index + filePathMatch.Length));
          if (lineNumberMatch.Success)
          {
            item.Line = int.Parse(lineNumberMatch.Value);
          }
        }

        items.Add(item);
      }
      return items.ToArray();
    }

    public string Method { get; set; }

    public string SourceFile { get; set; }

    public int Line { get; set; }

    public bool HasValidFileReference
    {
      get
      {
        return !string.IsNullOrEmpty(SourceFile) && File.Exists(SourceFile);
      }
    }
  }
}
