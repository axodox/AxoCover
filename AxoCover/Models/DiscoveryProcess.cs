using AxoCover.Common.Events;
using AxoCover.Common.Extensions;
using AxoCover.Common.ProcessHost;
using AxoCover.Common.Runner;
using AxoCover.Models.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;

namespace AxoCover.Models
{
  public class DiscoveryProcess : ServiceProcess, ITestDiscoveryMonitor
  {
    private readonly ManualResetEvent _serviceStartedEvent = new ManualResetEvent(false);
    private ITestDiscoveryService _testDiscoveryService;

    public event EventHandler<EventArgs<string>> MessageReceived;
    public event EventHandler<EventArgs<TestCase[]>> DiscoveryCompleted;

    private DiscoveryProcess() :
      base(new ServiceProcessInfo(RunnerMode.Discovery, AdapterExtensions.GetTestPlatformPath()))
    {
      _serviceStartedEvent.WaitOne();
    }

    public static DiscoveryProcess Create()
    {
      var discoveryProcess = new DiscoveryProcess();

      if (discoveryProcess._testDiscoveryService == null)
      {
        throw new Exception("Could not create service.");
      }
      else
      {
        return discoveryProcess;
      }
    }

    protected override void OnServiceStarted()
    {
      var channelFactory = new DuplexChannelFactory<ITestDiscoveryService>(this, NetworkingExtensions.GetServiceBinding());
      _testDiscoveryService = channelFactory.CreateChannel(new EndpointAddress(ServiceUri));
      try
      {
        _testDiscoveryService.Initialize();
      }
      catch
      {
        _testDiscoveryService = null;
      }
      _serviceStartedEvent.Set();
    }

    protected override void OnServiceFailed()
    {
      _serviceStartedEvent.Set();
    }

    void ITestDiscoveryMonitor.RecordMessage(TestMessageLevel testMessageLevel, string message)
    {
      var text = testMessageLevel.GetShortName() + " " + message;
      MessageReceived?.Invoke(this, new EventArgs<string>(text));
    }

    void ITestDiscoveryMonitor.RecordResults(TestCase[] testCases)
    {
      DiscoveryCompleted?.Invoke(this, new EventArgs<TestCase[]>(testCases));
    }

    public void DiscoverTestsAsync(IEnumerable<string> testSourcePaths, string runSettingsPath)
    {
      var adapters = AdapterExtensions.GetAdapters();
      _testDiscoveryService.DiscoverTestsAsync(adapters, testSourcePaths, runSettingsPath);
    }
  }
}
