using AxoCover.Common.Events;
using AxoCover.Common.Extensions;
using AxoCover.Common.Models;
using AxoCover.Common.ProcessHost;
using AxoCover.Common.Runner;
using AxoCover.Models.Extensions;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;

namespace AxoCover.Models
{
  public class DiscoveryProcess : TestProcess<ITestDiscoveryService>, ITestDiscoveryMonitor
  {
    public event EventHandler<EventArgs<string>> MessageReceived;

    private DiscoveryProcess(string[] testPlatformAssemblies) :
      base(new ServiceProcessInfo(RunnerMode.Discovery, testPlatformAssemblies)) { }
    
    public static DiscoveryProcess Create(string[] testPlatformAssemblies)
    {
      return new DiscoveryProcess(testPlatformAssemblies);
    }

    void ITestDiscoveryMonitor.RecordMessage(TestMessageLevel testMessageLevel, string message)
    {
      var text = testMessageLevel.GetShortName() + " " + message;
      MessageReceived?.Invoke(this, new EventArgs<string>(text));
    }

    public TestCase[] DiscoverTests(TestDiscoveryTask[] testDiscoveryTasks, string runSettingsPath)
    {
      return TestService.DiscoverTests(testDiscoveryTasks, runSettingsPath);
    }
  }
}
