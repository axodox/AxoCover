using System;

namespace AxoCover.Models.Events
{
  public class TestNavigatedEventArgs : EventArgs
  {
    public string Name { get; private set; }

    public TestNavigatedEventArgs(string name)
    {
      Name = name;
    }
  }

  public delegate void TestNavigatedEventHandler(object sender, TestNavigatedEventArgs e);
}
