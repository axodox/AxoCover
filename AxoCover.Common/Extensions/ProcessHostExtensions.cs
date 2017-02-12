using AxoCover.Common.ProcessHost;

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
        host.GuestProcess = guest;
        return host;
      }
    }
  }
}
