using AxoCover.Models.Data.CoverageReport;
using System;

namespace AxoCover.Models.Events
{
  public class TestFinishedEventArgs : EventArgs
  {
    public CoverageSession Report { get; private set; }

    public TestFinishedEventArgs(CoverageSession report)
    {
      Report = report;
    }
  }

  public delegate void TestFinishedEventHandler(object sender, TestFinishedEventArgs e);
}
