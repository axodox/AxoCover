using AxoCover.Common.Events;
using AxoCover.Common.Extensions;
using AxoCover.Common.Models;
using AxoCover.Common.ProcessHost;
using AxoCover.Common.Runner;
using AxoCover.Models.Storage;
using System;

namespace AxoCover.Models.Testing.Discovery
{
  public class DiscoveryProcess : TestProcess<ITestDiscoveryService>, ITestDiscoveryMonitor
  {
    public event EventHandler<EventArgs<string>> MessageReceived;

    private DiscoveryProcess(IHostProcessInfo hostProcess, string[] testPlatformAssemblies, IOptions options) :
      base(hostProcess.Embed(new ServiceProcessInfo(RunnerMode.Discovery, options.TestProtocol, options.IsDebugModeEnabled, testPlatformAssemblies)), options) { }
    
    public static DiscoveryProcess Create(string[] testPlatformAssemblies, IOptions options)
    {
      var hostProcess = new PlatformProcessInfo(options.TestPlatform);
      return new DiscoveryProcess(hostProcess, testPlatformAssemblies, options);
    }

    void ITestDiscoveryMonitor.RecordMessage(TestMessageLevel testMessageLevel, string message)
    {
      MessageReceived?.Invoke(this, new EventArgs<string>(message));
    }

    public TestCase[] DiscoverTests(TestDiscoveryTask[] testDiscoveryTasks, string runSettingsPath)
    {
      return TestService.DiscoverTests(testDiscoveryTasks, runSettingsPath);
    }
  }
}
