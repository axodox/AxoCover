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

    public void Initialize()
    {
      _monitor = OperationContext.Current.GetCallbackChannel<ITestDiscoveryMonitor>();
    }

    public string[] TryLoadAdaptersFromAssembly(string filePath)
    {
      try
      {
        _monitor.SendMessage(TestMessageLevel.Informational, $"Loading assembly from {filePath}...");
        var discovererNames = new List<string>();
        var assembly = Assembly.LoadFrom(filePath);
        var implementers = assembly.FindImplementers<ITestDiscoverer>();
        foreach (var implementer in implementers)
        {
          try
          {
            var testDiscoverer = Activator.CreateInstance(implementer) as ITestDiscoverer;
            discovererNames.Add(testDiscoverer.GetType().FullName);
            _testDiscoverers.Add(testDiscoverer);
          }
          catch (Exception e)
          {
            _monitor.SendMessage(TestMessageLevel.Warning, $"Could not instantiate discoverer {implementer.FullName}.\r\n{e.GetDescription()}");
          }
        }

        _monitor.SendMessage(TestMessageLevel.Informational, $"Assembly loaded.");
        return discovererNames.ToArray();
      }
      catch (Exception e)
      {
        _monitor.SendMessage(TestMessageLevel.Error, $"Could not load assembly {filePath}.\r\n{e.GetDescription()}");
        return new string[0];
      }
    }

    public TestCase[] DiscoverTests(IEnumerable<string> testSourcePaths, string runSettingsPath)
    {
      _monitor.SendMessage(TestMessageLevel.Informational, $"Discovering tests...");
      if (runSettingsPath != null)
      {
        _monitor.SendMessage(TestMessageLevel.Informational, $"Using run settings  {runSettingsPath}.");
      }

      try
      {
        var runSettings = new RunSettings(runSettingsPath == null ? null : File.ReadAllText(runSettingsPath));
        var context = new TestDiscoveryContext(_monitor, runSettings);

        foreach (var testDiscoverer in _testDiscoverers)
        {
          _monitor.SendMessage(TestMessageLevel.Informational, $"Running discoverer: {testDiscoverer.GetType().FullName}...");
          try
          {
            testDiscoverer.DiscoverTests(testSourcePaths, context, context, context);
          }
          catch (Exception e)
          {
            _monitor.SendMessage(TestMessageLevel.Warning, e.GetDescription());
          }
          _monitor.SendMessage(TestMessageLevel.Informational, $"Discoverer finished.");
        }

        _monitor.SendMessage(TestMessageLevel.Informational, $"Test discovery finished.");

        return context.TestCases;
      }
      catch (Exception e)
      {
        _monitor.SendMessage(TestMessageLevel.Error, $"Could not discover tests.\r\n{e.GetDescription()}");
        return new TestCase[0];
      }
    }

    public void Shutdown()
    {
      GenericExtensions.Exit();
    }
  }
}
