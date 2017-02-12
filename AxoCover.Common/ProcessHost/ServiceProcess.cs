using AxoCover.Common.Events;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace AxoCover.Common.ProcessHost
{
  public abstract class ServiceProcess : IDisposable
  {
    public event EventHandler Loaded;
    public event EventHandler<EventArgs<string>> OutputReceived;
    private const string _serviceStartedMessage = "Service started at: ";
    private const string _serviceFailedMessage = "Service failed.";
    private Process _process;
    private bool _isDisposed;

    public ServiceProcess(string filePath, string arguments)
    {
      if (!Path.IsPathRooted(filePath))
      {
        filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filePath);
      }

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
      if (e.Data == null) return;

      OutputReceived?.Invoke(this, new EventArgs<string>(e.Data));

      if (e.Data.StartsWith(_serviceStartedMessage))
      {
        var address = new Uri(e.Data.Substring(_serviceStartedMessage.Length));
        OnServiceStarted(address);
      }

      if (e.Data.StartsWith(_serviceFailedMessage))
      {
        OnServiceFailed();
      }
    }

    protected abstract void OnServiceStarted(Uri address);

    protected abstract void OnServiceFailed();

    public static void PrintServiceStarted(Uri address)
    {
      Console.WriteLine(_serviceStartedMessage + address);
    }

    public static void PrintServiceFailed()
    {
      Console.WriteLine(_serviceFailedMessage);
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
