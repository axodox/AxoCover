using AxoCover.Models.Events;

namespace AxoCover.Models
{
  public class CoverageProvider : ICoverageProvider
  {
    private ITestRunner _testRunner;

    public CoverageProvider(ITestRunner testRunner)
    {
      _testRunner = testRunner;
      _testRunner.TestsFinished += OnTestsFinished;
    }

    private void OnTestsFinished(object sender, TestFinishedEventArgs e)
    {

    }
  }
}
