using AxoCover.Common.Extensions;
using AxoCover.Models;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace AxoCover.ViewModels
{
  public class TerminalExceptionViewModel : ViewModel
  {
    public enum TelemetryUploadState
    {
      Disabled,
      InProgress,
      Failed,
      Completed
    }

    private IOptions _options;
    private ITelemetryManager _telemetryManager;

    public bool IsTelemetryEnabled
    {
      get
      {
        return _options.IsTelemetryEnabled;
      }
    }

    private Exception _exception;
    public Exception Exception
    {
      get
      {
        return _exception;
      }
      set
      {
        _exception = value;
        NotifyPropertyChanged(nameof(Exception));
        NotifyPropertyChanged(nameof(ExceptionDescription));

        if (IsTelemetryEnabled)
        {
          UploadException();
        }
      }
    }

    public string ExceptionDescription
    {
      get
      {
        return Exception.GetDescription();
      }
    }

    private TelemetryUploadState _state = TelemetryUploadState.Disabled;
    public TelemetryUploadState State
    {
      get
      {
        return _state;
      }
      set
      {
        _state = value;
        NotifyPropertyChanged(nameof(State));
        NotifyPropertyChanged(nameof(StateDescription));
      }
    }

    public string StateDescription
    {
      get
      {
        switch (State)
        {
          case TelemetryUploadState.Disabled:
            return Resources.TelemetryUploadingDisabled;
          case TelemetryUploadState.InProgress:
            return Resources.TelemetryUploadingInProgress;
          case TelemetryUploadState.Completed:
            return Resources.TelemetryUploadingSucceeded;
          case TelemetryUploadState.Failed:
            return Resources.TelemetryUploadingFailed;
          default:
            throw new NotImplementedException();
        }
      }
    }

    public ICommand PushExceptionCommand
    {
      get
      {
        return new DelegateCommand(
          p => UploadException(),
          p => Exception != null && (State == TelemetryUploadState.Disabled || State == TelemetryUploadState.Failed),
          p => ExecuteOnPropertyChange(p, nameof(Exception), nameof(State)));
      }
    }

    public ICommand BrowseIssuesCommand
    {
      get
      {
        return new DelegateCommand(p => Process.Start(_options.IssuesUrl));
      }
    }

    public ICommand RestartCommand
    {
      get
      {
        return new DelegateCommand(p =>
        {
          Process.Start(new ProcessStartInfo("cmd", "/c " + Environment.CommandLine)
          {
            CreateNoWindow = true,
            UseShellExecute = false
          });

          Process.GetCurrentProcess().Kill();
        });
      }
    }

    public TerminalExceptionViewModel(IOptions options, ITelemetryManager telemetryManager)
    {
      _options = options;
      _telemetryManager = telemetryManager;
    }

    private async void UploadException()
    {
      if (Exception == null) return;

      State = TelemetryUploadState.InProgress;
      if (await _telemetryManager.UploadExceptionAsync(Exception, true))
      {
        State = TelemetryUploadState.Completed;
      }
      else
      {
        State = TelemetryUploadState.Failed;
      }
    }
  }
}
