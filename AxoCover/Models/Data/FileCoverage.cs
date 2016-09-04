using System;
using System.Collections.Generic;

namespace AxoCover.Models.Data
{
  public class FileCoverage
  {
    public static readonly FileCoverage Empty = new FileCoverage(new Dictionary<int, LineCoverage>());

    private Dictionary<int, LineCoverage> _lines;

    public FileCoverage(Dictionary<int, LineCoverage> lines)
    {
      if (lines == null)
        throw new ArgumentNullException(nameof(lines));

      _lines = lines;
    }

    public LineCoverage this[int i]
    {
      get
      {
        LineCoverage lineCoverage;
        if (_lines.TryGetValue(i, out lineCoverage))
        {
          return lineCoverage;
        }
        else
        {
          return LineCoverage.Empty;
        }
      }
    }
  }
}
