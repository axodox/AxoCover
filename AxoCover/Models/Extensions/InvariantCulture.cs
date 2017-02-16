using System;
using System.Globalization;
using System.Threading;

namespace AxoCover.Models.Extensions
{
  public class InvariantCulture : IDisposable
  {
    private CultureInfo _lastCulture;
    private CultureInfo _lastUICulture;

    public InvariantCulture()
    {
      _lastCulture = Thread.CurrentThread.CurrentCulture;
      _lastUICulture = Thread.CurrentThread.CurrentUICulture;

      Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
      Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentUICulture;
    }

    public void Dispose()
    {
      Thread.CurrentThread.CurrentUICulture = _lastUICulture;
      Thread.CurrentThread.CurrentCulture = _lastCulture;
    }
  }
}
