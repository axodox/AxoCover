using AxoCover.Models.Data;
using System;

namespace AxoCover.Models.Events
{
  public class TestExecutedEventArgs : EventArgs
  {
    public string Path { get; private set; }
    public TestState Outcome { get; private set; }

    public TestExecutedEventArgs(string path, TestState outcome)
    {
      Path = path;
      Outcome = outcome;
    }
  }

  public delegate void TestExecutedEventHandler(object sender, TestExecutedEventArgs e);
}
