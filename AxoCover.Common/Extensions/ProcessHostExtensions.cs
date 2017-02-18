using AxoCover.Common.ProcessHost;
using System.Linq;

namespace AxoCover.Common.Extensions
{
  public static class ProcessHostExtensions
  {
    public static IProcessInfo Embed(this IHostProcessInfo host, IProcessInfo guest)
    {
      if (host == null)
      {
        return guest;
      }
      else
      {
        host.Crawl(p => p.GuestProcess as IHostProcessInfo, true).Last().GuestProcess = guest;
        return host;
      }
    }
  }
}
