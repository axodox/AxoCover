using AxoCover.Models.Data.CoverageReport;
using AxoCover.Models.Data.TestReport;
using System;

namespace AxoCover.Models.Events
{
  public class TestFinishedEventArgs : EventArgs
  {
    public CoverageSession CoverageReport { get; private set; }

    public TestRun TestReport { get; private set; }

    public TestFinishedEventArgs(CoverageSession coverageReport, TestRun testReport)
    {
      CoverageReport = coverageReport;
      TestReport = testReport;
    }
  }

  public delegate void TestFinishedEventHandler(object sender, TestFinishedEventArgs e);
}
