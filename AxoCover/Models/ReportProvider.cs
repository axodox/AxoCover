using AxoCover.Models.Events;
using AxoCover.Models.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AxoCover.Models
{
  public class ReportProvider : IReportProvider
  {
    private readonly Dispatcher _dispatcher = Application.Current.Dispatcher;

    private const string _runnerName = @"ReportGenerator\ReportGenerator.exe";
    protected readonly static string _runnerPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _runnerName);

    public event LogAddedEventHandler LogAdded;

    private Task<string> _reportTask;

    private Process _reportProcess;

    private bool _isAborting;

    public bool IsBusy
    {
      get
      {
        return _reportTask != null;
      }
    }

    public async Task<string> GenerateReportAsync(string coverageFile, string outputDirectory)
    {
      if (IsBusy)
      {
        throw new InvalidOperationException("The report generator is busy. Please wait for report generation to complete or abort.");
      }

      _reportTask = Task.Run(() => GenerateReport(coverageFile, outputDirectory));
      return await _reportTask;
    }

    private string GenerateReport(string coverageFile, string outputDirectory)
    {
      try
      {
        var arguments = $"\"-reports:{coverageFile}\" \"-targetdir:{outputDirectory}\"";

        _reportProcess = new Process()
        {
          StartInfo = new ProcessStartInfo(_runnerPath, arguments)
          {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
          }
        };

        _reportProcess.OutputDataReceived += (o, e) =>
        {
          if (e.Data == null) return;
          var text = e.Data;

          OnLogAdded(text);
        };

        _reportProcess.Start();
        _reportProcess.BeginOutputReadLine();

        while (!_reportProcess.HasExited)
        {
          _reportProcess.WaitForExit(1000);
        }

        if (_isAborting) return null;

        var path = Path.Combine(outputDirectory, "index.htm");
        return File.Exists(path) ? path : null;
      }
      catch
      {
        return null;
      }
      finally
      {
        _reportProcess.Dispose();
        _reportProcess = null;
        _reportTask = null;
      }
    }

    protected void OnLogAdded(string text)
    {
      _dispatcher.BeginInvoke(() => LogAdded?.Invoke(this, new LogAddedEventArgs(text)));
    }

    public Task AbortReportGenerationAsync()
    {
      if (IsBusy)
      {
        _isAborting = true;
        if (_reportProcess != null && !_reportProcess.HasExited)
        {
          _reportProcess.Kill();
        }
      }

      return _reportTask ?? new TaskCompletionSource<string>().Task;
    }
  }
}
