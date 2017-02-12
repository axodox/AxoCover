using AxoCover.Common.Events;
using AxoCover.Common.Extensions;
using AxoCover.Common.ProcessHost;
using AxoCover.Common.Runner;
using AxoCover.Models.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;

namespace AxoCover.Models
{
  public class DiscoveryProcess : ServiceProcess, ITestDiscoveryMonitor
  {
    private readonly ManualResetEvent _serviceStartedEvent = new ManualResetEvent(false);
    private ITestDiscoveryService _testDiscoveryService;
    private bool _isDisposed;

    public event EventHandler<EventArgs<string>> MessageReceived;

    private DiscoveryProcess() :
      base("AxoCover.Runner.exe", string.Join(" ", RunnerMode.Discovery, Process.GetCurrentProcess().Id, "\"" + AdapterExtensions.GetTestPlatformPath() + "\""))
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

    protected override void OnServiceStarted(Uri address)
    {
      var channelFactory = new DuplexChannelFactory<ITestDiscoveryService>(this, NetworkingExtensions.GetServiceBinding());
      _testDiscoveryService = channelFactory.CreateChannel(new EndpointAddress(address));
      _testDiscoveryService.Initialize();

      var adapters = AdapterExtensions.GetAdapters();
      foreach (var adapter in adapters)
      {
        _testDiscoveryService.TryLoadAdaptersFromAssembly(adapter);
      }

      _serviceStartedEvent.Set();
    }

    protected override void OnServiceFailed()
    {
      _serviceStartedEvent.Set();
    }

    void ITestDiscoveryMonitor.SendMessage(TestMessageLevel testMessageLevel, string message)
    {
      var text = testMessageLevel.GetShortName() + " " + message;
      MessageReceived?.Invoke(this, new EventArgs<string>(text));
    }

    public TestCase[] DiscoverTests(IEnumerable<string> testSourcePaths, string runSettingsPath)
    {
      return _testDiscoveryService.DiscoverTests(testSourcePaths, runSettingsPath);
    }

    public override void Dispose()
    {
      if (!_isDisposed)
      {
        _isDisposed = true;
        try
        {
          _testDiscoveryService.Shutdown();
        }
        catch { }

        base.Dispose();
      }
    }
  }
}
