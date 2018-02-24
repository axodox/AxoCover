using AxoCover.Common.Extensions;
using AxoCover.Common.Models;
using AxoCover.Common.Runner;
using AxoCover.Runner.Properties;
using AxoCover.Runner.Settings;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public int Initialize()
    {
      _monitor = OperationContext.Current.GetCallbackChannel<ITestDiscoveryMonitor>();
      return Process.GetCurrentProcess().Id;
    }

    private ITestDiscoverer[] LoadDiscoverers(string adapterSource)
    {
      var testDiscoverers = new List<ITestDiscoverer>();
      try
      {
        _monitor.RecordMessage(TestMessageLevel.Informational, $">> Loading assembly from {adapterSource}...");
        var assembly = Assembly.Load(AssemblyName.GetAssemblyName(adapterSource));
        var implementers = assembly.FindImplementers<ITestDiscoverer>();
        foreach (var implementer in implementers)
        {
          try
          {
            var testDiscoverer = Activator.CreateInstance(implementer) as ITestDiscoverer;
            testDiscoverers.Add(testDiscoverer);

            _monitor.RecordMessage(TestMessageLevel.Informational, $"|| Loaded discoverer: {implementer.FullName}.");
          }
          catch (Exception e)
          {
            _monitor.RecordMessage(TestMessageLevel.Warning, $"|| Could not load discoverer {implementer.FullName}!\r\n{e.GetDescription().PadLinesLeft("|| ")}");
          }
        }

        _monitor.RecordMessage(TestMessageLevel.Informational, $"<< Assembly loaded.");
      }
      catch (Exception e)
      {
        _monitor.RecordMessage(TestMessageLevel.Error, $"|| Could not load assembly {adapterSource}!\r\n{e.GetDescription().PadLinesLeft("|| ")}");
      }
      return testDiscoverers.ToArray();
    }

    public TestCase[] DiscoverTests(TestDiscoveryTask[] discoveryTasks, string runSettingsPath)
    {
      _monitor.RecordMessage(TestMessageLevel.Informational, Resources.Branding);      

      Thread.CurrentThread.Name = "Test discoverer";
      Thread.CurrentThread.IsBackground = true;

      _monitor.RecordMessage(TestMessageLevel.Informational, $"> Discovering tests...");
      _monitor.RecordMessage(TestMessageLevel.Informational, $"| Runner version is {Assembly.GetExecutingAssembly().GetName().Version}.");
      _monitor.RecordMessage(TestMessageLevel.Informational, $"| We are on {(Environment.Is64BitProcess ? "x64" : "x86")} platform.");
      if (!string.IsNullOrEmpty(runSettingsPath))
      {
        _monitor.RecordMessage(TestMessageLevel.Informational, $"| Using run settings: {runSettingsPath}.");
      }

      try
      {
        var runSettings = new RunSettings(string.IsNullOrEmpty(runSettingsPath) ? null : File.ReadAllText(runSettingsPath));
        var context = new TestDiscoveryContext(_monitor, runSettings);

        foreach(var discoveryTask in discoveryTasks)
        {
          NativeServices.ExecuteWithFileRedirection(discoveryTask.TestAdapterOptions, () =>
          {
            var testDiscoverers = LoadDiscoverers(discoveryTask.TestAdapterOptions.AssemblyPath);

            foreach (var testDiscoverer in testDiscoverers)
            {
              _monitor.RecordMessage(TestMessageLevel.Informational, $">> Running discoverer: {testDiscoverer.GetType().FullName}...");
              foreach (var testSourcePath in discoveryTask.TestAssemblyPaths)
              {
                _monitor.RecordMessage(TestMessageLevel.Informational, $"|| Checking {testSourcePath}...");
                try
                {
                  testDiscoverer.DiscoverTests(new[] { testSourcePath }, context, context, context);
                }
                catch (Exception e)
                {
                  _monitor.RecordMessage(TestMessageLevel.Warning, e.GetDescription());
                }
              }
              _monitor.RecordMessage(TestMessageLevel.Informational, $"<< Discoverer finished.");
            }
          }, (level, message) => _monitor.RecordMessage(level, "| " + message));
        }
        
        _monitor.RecordMessage(TestMessageLevel.Informational, $"< Test discovery finished.");
        return context.TestCases.Convert();
      }
      catch (Exception e)
      {
        _monitor.RecordMessage(TestMessageLevel.Error, $"< Could not discover tests!\r\n{e.GetDescription().PadLinesLeft("< ")}");
        return new TestCase[0];
      }
    }

    public void Shutdown()
    {
      Program.Exit();
    }
  }
}
