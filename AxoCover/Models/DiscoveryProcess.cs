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
  public class DiscoveryProcess : ServiceProcess, ITestDiscoveryMonitor
  {
    private readonly ManualResetEvent _serviceStartedEvent = new ManualResetEvent(false);
    private ITestDiscoveryService _testDiscoveryService;

    public event EventHandler<EventArgs<string>> MessageReceived;

    private DiscoveryProcess(string[] testPlatformAssemblies) :
      base(new ServiceProcessInfo(RunnerMode.Discovery, testPlatformAssemblies))
    {
      Exited += OnExited;
      _serviceStartedEvent.WaitOne();
    }

    private void OnExited(object sender, EventArgs e)
    {
      if(_testDiscoveryService != null)
      {
        (_testDiscoveryService as ICommunicationObject).Abort();
      }
    }

    public static DiscoveryProcess Create(string[] testPlatformAssemblies)
    {
      var discoveryProcess = new DiscoveryProcess(testPlatformAssemblies);

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

    public TestCase[] DiscoverTests(IEnumerable<string> testSourcePaths, string runSettingsPath, string[] testAdapterAssemblies)
    {
      return _testDiscoveryService.DiscoverTests(testAdapterAssemblies, testSourcePaths, runSettingsPath);
    }
  }
}
