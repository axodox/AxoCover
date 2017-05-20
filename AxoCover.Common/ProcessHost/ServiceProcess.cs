using AxoCover.Common.Events;
using AxoCover.Common.Extensions;
using AxoCover.Common.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace AxoCover.Common.ProcessHost
{
  public abstract class ServiceProcess : IDisposable
  {
    public event EventHandler Exited;
    public event EventHandler<EventArgs<string>> OutputReceived;
    private const string _serviceStartedMessage = "Service started at: ";
    private const string _serviceFailedMessage = "Service failed. See details at: ";
    private Process _process;
    private bool _isDisposed;

    public int ProcessId
    {
      get
      {
        return _process.Id;
      }
    }

    public Uri ServiceUri { get; private set; }

    public bool HasExited { get; private set; }

    public ServiceProcess(IProcessInfo processInfo)
    {
      var filePath = processInfo.FilePath;
      if (!Path.IsPathRooted(processInfo.FilePath))
      {
        filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filePath);
      }

      _process = new Process()
      {
        StartInfo = new ProcessStartInfo(filePath, processInfo.Arguments)
        {
          RedirectStandardOutput = true,
          UseShellExecute = false,
          CreateNoWindow = true
        }
      };
      _process.Exited += OnExited;
      _process.OutputDataReceived += OnOutputDataReceived;

      _process.Start();
      _process.BeginOutputReadLine();
    }

    private void OnExited(object sender, EventArgs e)
    {
      if (!HasExited)
      {
        HasExited = true;
        Exited?.Invoke(this, EventArgs.Empty);
      }
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
      if (e.Data == null) return;

      OutputReceived?.Invoke(this, new EventArgs<string>(e.Data));

      if (e.Data.StartsWith(_serviceStartedMessage))
      {
        var address = new Uri(e.Data.Substring(_serviceStartedMessage.Length));
        ServiceUri = address;
        OnServiceStarted();
      }

      if (e.Data.StartsWith(_serviceFailedMessage))
      {
        var crashFilePath = e.Data.Substring(_serviceFailedMessage.Length);
        var crashDetails = File.ReadAllText(crashFilePath);
        var exception = JsonConvert.DeserializeObject<SerializableException>(crashDetails);
        OnServiceFailed(exception);
      }
    }

    protected abstract void OnServiceStarted();

    protected abstract void OnServiceFailed(SerializableException exception);

    public static void PrintServiceStarted(Uri address)
    {
      Console.WriteLine(_serviceStartedMessage + address);
    }

    public static void PrintServiceFailed(string detailsFile)
    {
      Console.WriteLine(_serviceFailedMessage + detailsFile);
    }

    public void WaitForExit()
    {
      while (!_process.HasExited)
      {
        _process.WaitForExit(1000);
      }
    }

    public virtual void Dispose()
    {
      if (!_isDisposed)
      {
        _isDisposed = true;
        if (_process != null)
        {
          try
          {
            try
            {
              _process.OutputDataReceived -= OnOutputDataReceived;
              _process.KillWithChildren();
            }
            catch { }
            finally
            {
              OnExited(this, EventArgs.Empty);
              _process.Dispose();
            }
          }
          catch { }
        }
      }
    }

    ~ServiceProcess()
    {
      Dispose();
    }
  }
}
