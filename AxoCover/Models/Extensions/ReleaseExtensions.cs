using AxoCover.Models.Data;
using System.Collections.Generic;
using System.Linq;

namespace AxoCover.Models.Extensions
{
  public static class ReleaseExtensions
  {
    public static Release GetLatest(this IEnumerable<Release> releases, string branch)
    {
      return releases
        .Where(p => p.Branch == branch)
        .OrderBy(p => p.Version)
        .LastOrDefault();
    }
  }
}
