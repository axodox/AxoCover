using System;

namespace AxoCover.Models.Events
{
  public class LogAddedEventArgs : EventArgs
  {
    public string Text { get; private set; }

    public LogAddedEventArgs(string text)
    {
      Text = text;
    }
  }

  public delegate void LogAddedEventHandler(object sender, LogAddedEventArgs e);
}
