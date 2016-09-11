using System;
using System.Collections.Generic;

namespace AxoCover.Models.Data
{
  public class FileResults
  {
    public static readonly FileResults Empty = new FileResults(new Dictionary<int, LineResult[]>());

    private Dictionary<int, LineResult[]> _lines;

    public FileResults(Dictionary<int, LineResult[]> lines)
    {
      if (lines == null)
        throw new ArgumentNullException(nameof(lines));

      _lines = lines;
    }

    public LineResult[] this[int i]
    {
      get
      {
        LineResult[] lineResult;
        if (_lines.TryGetValue(i, out lineResult))
        {
          return lineResult;
        }
        else
        {
          return new LineResult[0];
        }
      }
    }
  }
}
