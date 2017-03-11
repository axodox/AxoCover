using AxoCover.Common.Extensions;
using AxoCover.Common.Runner;
using AxoCover.Runner.Settings;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.Threading;

namespace AxoCover.Runner
{
  [ServiceBehavior(
    InstanceContextMode = InstanceContextMode.PerSession,
    ConcurrencyMode = ConcurrencyMode.Multiple,
    AddressFilterMode = AddressFilterMode.Any,
    IncludeExceptionDetailInFaults = true)]
  public class TestDiscoveryService : ITestDiscoveryService
  {
    private ITestDiscoveryMonitor _monitor;
    private List<ITestDiscoverer> _testDiscoverers = new List<ITestDiscoverer>();
    private bool _isShuttingDown = false;

    public void Initialize()
    {
      _monitor = OperationContext.Current.GetCallbackChannel<ITestDiscoveryMonitor>();
      var monitorObject = _monitor as ICommunicationObject;
      monitorObject.Closed += OnMonitorShutdown;
      monitorObject.Faulted += OnMonitorShutdown;
    }

    private void OnMonitorShutdown(object sender, EventArgs e)
    {
      if (_isShuttingDown)
      {
        Program.Exit();
      }
    }

    private void LoadDiscoverers(string adapterSource)
    {
      try
      {
        _monitor.RecordMessage(TestMessageLevel.Informational, $"Loading assembly from {adapterSource}...");
        var assembly = Assembly.LoadFrom(adapterSource);
        var implementers = assembly.FindImplementers<ITestDiscoverer>();
        foreach (var implementer in implementers)
        {
          try
          {
            var testDiscoverer = Activator.CreateInstance(implementer) as ITestDiscoverer;
            _testDiscoverers.Add(testDiscoverer);
          }
          catch (Exception e)
          {
            _monitor.RecordMessage(TestMessageLevel.Warning, $"Could not instantiate discoverer {implementer.FullName}.\r\n{e.GetDescription()}");
          }
        }

        _monitor.RecordMessage(TestMessageLevel.Informational, $"Assembly loaded.");
      }
      catch (Exception e)
      {
        _monitor.RecordMessage(TestMessageLevel.Error, $"Could not load assembly {adapterSource}.\r\n{e.GetDescription()}");
      }
    }

    public void DiscoverTestsAsync(string[] adapterSources, IEnumerable<string> testSourcePaths, string runSettingsPath)
    {
      var thread = new Thread(() => DiscoverTests(adapterSources, testSourcePaths, runSettingsPath));
      thread.Start();
    }

    private void DiscoverTests(string[] adapterSources, IEnumerable<string> testSourcePaths, string runSettingsPath)
    {
      Thread.CurrentThread.Name = "Test discoverer";
      Thread.CurrentThread.IsBackground = true;

      foreach (var adapterSource in adapterSources)
      {
        LoadDiscoverers(adapterSource);
      }

      _monitor.RecordMessage(TestMessageLevel.Informational, $"Discovering tests...");
      if (runSettingsPath != null)
      {
        _monitor.RecordMessage(TestMessageLevel.Informational, $"Using run settings  {runSettingsPath}.");
      }

      try
      {
        var runSettings = new RunSettings(runSettingsPath == null ? null : File.ReadAllText(runSettingsPath));
        var context = new TestDiscoveryContext(_monitor, runSettings);

        foreach (var testDiscoverer in _testDiscoverers)
        {
          _monitor.RecordMessage(TestMessageLevel.Informational, $"Running discoverer: {testDiscoverer.GetType().FullName}...");
          try
          {
            testDiscoverer.DiscoverTests(testSourcePaths, context, context, context);
          }
          catch (Exception e)
          {
            _monitor.RecordMessage(TestMessageLevel.Warning, e.GetDescription());
          }
          _monitor.RecordMessage(TestMessageLevel.Informational, $"Discoverer finished.");
        }

        _monitor.RecordMessage(TestMessageLevel.Informational, $"Test discovery finished.");
        _monitor.RecordResults(context.TestCases);
      }
      catch (Exception e)
      {
        _monitor.RecordMessage(TestMessageLevel.Error, $"Could not discover tests.\r\n{e.GetDescription()}");
        _monitor.RecordResults(new TestCase[0]);
      }
    }

    public void Shutdown()
    {
      _isShuttingDown = true;
    }
  }
}
