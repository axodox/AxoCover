using AxoCover.Common.Runner;
using AxoCover.Runner.Settings;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System.Collections.Concurrent;

namespace AxoCover.Runner
{
  public class TestDiscoveryContext :
    IDiscoveryContext,
    ITestCaseDiscoverySink,
    IMessageLogger
  {
    private readonly ITestDiscoveryMonitor _monitor;
    private readonly ConcurrentBag<TestCase> _testCases = new ConcurrentBag<TestCase>();

    public IRunSettings RunSettings { get; private set; }

    public TestCase[] TestCases
    {
      get
      {
        return _testCases.ToArray();
      }
    }

    public TestDiscoveryContext(ITestDiscoveryMonitor monitor, RunSettings runSettings = null)
    {
      _monitor = monitor;
      RunSettings = runSettings ?? new RunSettings();
    }

    public void SendTestCase(TestCase discoveredTest)
    {
      _testCases.Add(discoveredTest);
    }

    public void SendMessage(TestMessageLevel testMessageLevel, string message)
    {
      _monitor.SendMessage(testMessageLevel, message);
    }
  }
}
