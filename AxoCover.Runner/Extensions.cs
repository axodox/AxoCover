using System;
using System.Linq;

namespace AxoCover.Runner
{
  public static class Extensions
  {
    public static string PadLinesLeft(this string text, string prefix)
    {
      return string.Join(Environment.NewLine, text
        .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
        .Select(p => prefix + p));
    }
  }
}
