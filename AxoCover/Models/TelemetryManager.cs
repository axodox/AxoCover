using AxoCover.Common.Extensions;
using AxoCover.Views;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AxoCover.Models
{
  public abstract class TelemetryManager : ITelemetryManager
  {
    private static readonly TimeSpan _terminationTimeout = TimeSpan.FromSeconds(2);
    protected IEditorContext _editorContext;
    protected IOptions _options;

    public bool IsTelemetryEnabled
    {
      get { return _options.IsTelemetryEnabled; }
      set { _options.IsTelemetryEnabled = value; }
    }

    public TelemetryManager(IEditorContext editorContext, IOptions options)
    {
      _editorContext = editorContext;
      _options = options;

      Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
      AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
      var description = e.Exception.GetDescription();
      if (description.Contains(nameof(AxoCover)))
      {
        _editorContext.WriteToLog(Resources.ExceptionEncountered);
        _editorContext.WriteToLog(description);
        if (!Debugger.IsAttached)
        {
          UploadExceptionAsync(e.Exception);
          e.Handled = true;
        }
      }
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      var exception = e.ExceptionObject as Exception;
      if (exception == null) return;

      Application.Current.Dispatcher.Invoke(() =>
      {
        var description = exception.GetDescription();
        if (description.Contains(nameof(AxoCover)))
        {
          var dialog = new ViewDialog<TerminalExceptionView>();
          dialog.View.ViewModel.Exception = exception;
          dialog.ShowDialog();
        }
      });
    }

    protected abstract Task<bool> UploadExceptionAsync(Exception exception);

    public async Task<bool> UploadExceptionAsync(Exception exception, bool force = false)
    {
      if (IsTelemetryEnabled || force)
      {
        return await UploadExceptionAsync(exception);
      }
      else
      {
        return false;
      }
    }
  }
}
