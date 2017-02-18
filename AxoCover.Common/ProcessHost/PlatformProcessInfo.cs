using AxoCover.Common.Settings;
using System;

namespace AxoCover.Common.ProcessHost
{
  public class PlatformProcessInfo : IHostProcessInfo
  {
    public string Arguments
    {
      get
      {
        if (GuestProcess != null)
        {
          return "\"" + GuestProcess.FilePath + "\" " + string.Join(" ", GuestProcess.Arguments);
        }
        else
        {
          throw new InvalidOperationException("The guest process is not specified.");
        }
      }
    }

    public string FilePath
    {
      get
      {
        switch (_platform)
        {
          case TestPlatform.x86:
            return typeof(Host.x86.Program).Assembly.Location;
          case TestPlatform.x64:
            return typeof(Host.x64.Program).Assembly.Location;
          default:
            throw new NotImplementedException($"Platform {_platform} is not implemented.");
        }
      }
    }

    public IProcessInfo GuestProcess { get; set; }

    private TestPlatform _platform;
    public PlatformProcessInfo(TestPlatform platform)
    {
      _platform = platform;
    }
  }
}
