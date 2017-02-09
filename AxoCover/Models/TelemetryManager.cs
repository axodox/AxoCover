using AxoCover.Models.Extensions;
using AxoCover.Properties;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AxoCover.Models
{
  public abstract class TelemetryManager : ITelemetryManager
  {
    protected IEditorContext _editorContext;

    public bool IsTelemetryEnabled
    {
      get { return Settings.Default.IsTelemetryEnabled; }
      set { Settings.Default.IsTelemetryEnabled = value; }
    }

    public TelemetryManager(IEditorContext editorContext)
    {
      _editorContext = editorContext;

      Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
      var description = e.Exception.GetDescription();
      if (description.Contains(nameof(AxoCover)))
      {
        _editorContext.WriteToLog(Resources.ExceptionEncountered);
        _editorContext.WriteToLog(description);
        if (Debugger.IsAttached)
        {
          Debugger.Break();
        }
        else
        {
          UploadExceptionAsync(e.Exception);
        }
        e.Handled = true;
      }
    }

    public abstract Task<bool> UploadExceptionAsync(Exception exception);
  }
}
