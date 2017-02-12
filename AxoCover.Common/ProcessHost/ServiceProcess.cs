using AxoCover.Common.Events;
using System;
using System.Diagnostics;

namespace AxoCover.Common.ProcessHost
{
  public abstract class ServiceProcess : IDisposable
  {
    public event EventHandler Loaded;
    public event EventHandler<EventArgs<string>> OutputReceived;
    private const string _serviceStartedMessage = "Service started at: ";
    private Process _process;
    private bool _isDisposed;

    public ServiceProcess(string filePath, string arguments)
    {
      _process = new Process()
      {
        StartInfo = new ProcessStartInfo(filePath, arguments)
        {
          RedirectStandardOutput = true,
          UseShellExecute = false,
          CreateNoWindow = true
        }
      };

      _process.OutputDataReceived += OnOutputDataReceived;

      _process.Start();
      _process.BeginOutputReadLine();
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
      OutputReceived?.Invoke(this, new EventArgs<string>(e.Data));

      if (e.Data.StartsWith(_serviceStartedMessage))
      {
        var address = e.Data.Substring(_serviceStartedMessage.Length);

      }
    }

    public static void PrintServiceStarted(Uri address)
    {
      Console.WriteLine(_serviceStartedMessage + address);
    }

    public void Dispose()
    {
      if (!_isDisposed)
      {
        _isDisposed = true;
        _process.OutputDataReceived -= OnOutputDataReceived;
      }
    }
  }
}
