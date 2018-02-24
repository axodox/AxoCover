using AxoCover.Models.Events;
using AxoCover.Models.Testing.Results;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace AxoCover.ViewModels
{
  public class ReportGeneratorViewModel : ViewModel
  {
    public const string ReportDirectory = "reports";

    private readonly IReportProvider _reportProvider;

    public event EventHandler Finished;

    private bool _isInProgress;
    public bool IsInProgress
    {
      get { return _isInProgress; }
      set
      {
        _isInProgress = value;
        NotifyPropertyChanged(nameof(IsInProgress));
      }
    }

    private bool _isFailed;
    public bool IsFailed
    {
      get { return _isFailed; }
      set
      {
        _isFailed = value;
        NotifyPropertyChanged(nameof(IsFailed));
      }
    }

    public ObservableCollection<string> Log { get; set; }

    public ReportGeneratorViewModel(IReportProvider reportProvider)
    {
      Log = new ObservableCollection<string>();
      _reportProvider = reportProvider;
      _reportProvider.LogAdded += OnLogAdded;
    }

    private void OnLogAdded(object sender, LogAddedEventArgs e)
    {
      Log.Add(e.Text);
    }

    public async void GenerateReport(string reportPath)
    {
      Log.Clear();
      var outputDirectory = Path.Combine(
        Path.GetDirectoryName(reportPath),
        ReportDirectory,
        Path.GetFileNameWithoutExtension(reportPath));

      IsInProgress = true;
      try
      {
        var indexPath = await _reportProvider.GenerateReportAsync(reportPath);

        if (indexPath != null)
        {
          Process.Start(indexPath);
        }
        else
        {
          IsFailed = true;
        }
      }
      catch
      {
        IsFailed = true;
      }
      IsInProgress = false;
      Finished?.Invoke(this, EventArgs.Empty);
    }

    public async void Abort()
    {
      if (IsInProgress)
      {
        await _reportProvider.AbortReportGenerationAsync();
      }
    }
  }
}
