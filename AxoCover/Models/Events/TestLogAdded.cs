using System;

namespace AxoCover.Models.Events
{
  public class TestLogAddedEventArgs : EventArgs
  {
    public string Text { get; private set; }

    public TestLogAddedEventArgs(string text)
    {
      Text = text;
    }
  }

  public delegate void TestLogAddedEventHandler(object sender, TestLogAddedEventArgs e);
}
