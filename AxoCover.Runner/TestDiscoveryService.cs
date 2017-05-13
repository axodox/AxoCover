using AxoCover.Common.Extensions;
using AxoCover.Common.Models;
using AxoCover.Common.Runner;
using AxoCover.Runner.Settings;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
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
    private List<ITestDiscoverer> _testDiscoverers = new List<ITestDiscoverer>();
    private ITestDiscoveryMonitor _monitor;

    public void Initialize()
    {
      _monitor = OperationContext.Current.GetCallbackChannel<ITestDiscoveryMonitor>();
    }

    private void LoadDiscoverers(string adapterSource)
    {
      try
      {
        _monitor.RecordMessage(TestMessageLevel.Informational, $"Loading assembly from {adapterSource}...");
        var assembly = Assembly.Load(AssemblyName.GetAssemblyName(adapterSource));
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

    public TestCase[] DiscoverTests(string[] adapterSources, IEnumerable<string> testSourcePaths, string runSettingsPath)
    {
      Thread.CurrentThread.Name = "Test discoverer";
      Thread.CurrentThread.IsBackground = true;

      foreach (var adapterSource in adapterSources)
      {
        LoadDiscoverers(adapterSource);
      }

      _monitor.RecordMessage(TestMessageLevel.Informational, $"Discovering tests...");
      if (!string.IsNullOrEmpty(runSettingsPath))
      {
        _monitor.RecordMessage(TestMessageLevel.Informational, $"Using run settings  {runSettingsPath}.");
      }

      try
      {
        var runSettings = new RunSettings(string.IsNullOrEmpty(runSettingsPath) ? null : File.ReadAllText(runSettingsPath));
        var context = new TestDiscoveryContext(_monitor, runSettings);

        foreach (var testDiscoverer in _testDiscoverers)
        {
          _monitor.RecordMessage(TestMessageLevel.Informational, $"Running discoverer: {testDiscoverer.GetType().FullName}...");
          foreach (var testSourcePath in testSourcePaths)
          {
            _monitor.RecordMessage(TestMessageLevel.Informational, $"Checking {testSourcePath}...");
            try
            {
              testDiscoverer.DiscoverTests(new[] { testSourcePath }, context, context, context);
            }
            catch (Exception e)
            {
              _monitor.RecordMessage(TestMessageLevel.Warning, e.GetDescription());
            }
          }
          _monitor.RecordMessage(TestMessageLevel.Informational, $"Discoverer finished.");
        }
        
        _monitor.RecordMessage(TestMessageLevel.Informational, $"Test discovery finished.");
        return context.TestCases.Convert();
      }
      catch (Exception e)
      {
        _monitor.RecordMessage(TestMessageLevel.Error, $"Could not discover tests.\r\n{e.GetDescription()}");
        return new TestCase[0];
      }
    }

    public void Shutdown()
    {
      Program.Exit();
    }
  }
}
