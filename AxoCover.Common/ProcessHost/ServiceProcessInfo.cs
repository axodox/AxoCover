using AxoCover.Common.Extensions;
using System.Diagnostics;
using System.Linq;

namespace AxoCover.Common.ProcessHost
{
  public class ServiceProcessInfo : IProcessInfo
  {
    public string Arguments { get; private set; }

    public string FilePath { get; private set; }

    public ServiceProcessInfo(RunnerMode mode, CommunicationProtocol protocol, bool debugMode, params string[] assemblies)
    {
      FilePath = "AxoCover.Runner.exe".ToAbsolutePath();
      var assemblyArgs = string.Join(" ", assemblies.Select(p => "\"" + p + "\""));
      Arguments = string.Join(" ", mode, Process.GetCurrentProcess().Id, protocol.ToString(), debugMode.ToString(), assemblyArgs);
    }
  }
}
