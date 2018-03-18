using AxoCover.Common.Extensions;
using AxoCover.Common.Settings;
using AxoCover.Models.Editor;
using AxoCover.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace AxoCover.Models.Storage
{
  public class Options : INotifyPropertyChanged, IOptions
  {
    public event PropertyChangedEventHandler PropertyChanged;

    #region Test settings
    public string TestRunner
    {
      get { return Settings.Default.TestRunner; }
      set { Settings.Default.TestRunner = value; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public TestPlatform TestPlatform
    {
      get { return Settings.Default.TestPlatform; }
      set { Settings.Default.TestPlatform = value; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public TestApartmentState TestApartmentState
    {
      get { return Settings.Default.TestApartmentState; }
      set { Settings.Default.TestApartmentState = value; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public TestAdapterMode TestAdapterMode
    {
      get { return Settings.Default.TestAdapterMode; }
      set { Settings.Default.TestAdapterMode = value; }
    }

    public bool IsRedirectingFrameworkAssemblies
    {
      get { return Settings.Default.IsRedirectingFrameworkAssemblies; }
      set { Settings.Default.IsRedirectingFrameworkAssemblies = value; }
    }

    public string TestSettings
    {
      get { return Settings.Default.TestSettings; }
      set { Settings.Default.TestSettings = value; }
    }

    [JsonIgnore]
    public CommunicationProtocol TestProtocol
    {
      get { return Settings.Default.TestProtocol; }
      set { Settings.Default.TestProtocol = value; }
    }
    
    [JsonIgnore]
    public int StartupTimeout
    {
      get { return Settings.Default.StartupTimeout; }
      set { Settings.Default.StartupTimeout = value; }
    }

    [JsonIgnore]
    public int DebuggerTimeout
    {
      get { return Settings.Default.DebuggerTimeout; }
      set { Settings.Default.DebuggerTimeout = value; }
    }

    [JsonIgnore]
    public bool IsDebugModeEnabled
    {
      get { return Settings.Default.IsDebugModeEnabled; }
      set { Settings.Default.IsDebugModeEnabled = value; }
    }

    [JsonIgnore]
    public bool IsAutoBuildEnabled
    {
      get { return Settings.Default.IsAutoBuildEnabled; }
      set { Settings.Default.IsAutoBuildEnabled = value; }
    }
    #endregion

    #region Coverage settings
    public string ExcludeAttributes
    {
      get { return Settings.Default.ExcludeAttributes; }
      set { Settings.Default.ExcludeAttributes = value; }
    }

    public string ExcludeFiles
    {
      get { return Settings.Default.ExcludeFiles; }
      set { Settings.Default.ExcludeFiles = value; }
    }

    public string ExcludeDirectories
    {
      get { return Settings.Default.ExcludeDirectories; }
      set { Settings.Default.ExcludeDirectories = value; }
    }

    public string Filters
    {
      get { return Settings.Default.Filters; }
      set { Settings.Default.Filters = value; }
    }

    public bool IsIncludingSolutionAssemblies
    {
      get { return Settings.Default.IsIncludingSolutionAssemblies; }
      set { Settings.Default.IsIncludingSolutionAssemblies = value; }
    }

    public bool IsExcludingTestAssemblies
    {
      get { return Settings.Default.IsExcludingTestAssemblies; }
      set { Settings.Default.IsExcludingTestAssemblies = value; }
    }

    public bool IsCoveringByTest
    {
      get { return Settings.Default.IsCoveringByTest; }
      set { Settings.Default.IsCoveringByTest = value; }
    }

    public bool IsMergingByHash
    {
      get { return Settings.Default.IsMergingByHash; }
      set { Settings.Default.IsMergingByHash = value; }
    }

    public bool IsSkippingAutoProps
    {
      get { return Settings.Default.IsSkippingAutoProps; }
      set { Settings.Default.IsSkippingAutoProps = value; }
    }

    public bool IsVisitorCountLimited
    {
      get { return Settings.Default.IsVisitorRecordLimited; }
      set { Settings.Default.IsVisitorRecordLimited = value; }
    }

    public int VisitorCountLimit
    {
      get { return Settings.Default.VisitorCountLimit; }
      set { Settings.Default.VisitorCountLimit = value; }
    }
    #endregion

    #region Visualization settings
    [JsonIgnore]
    public bool IsShowingLineCoverage
    {
      get { return Settings.Default.IsShowingLineCoverage; }
      set { Settings.Default.IsShowingLineCoverage = value; }
    }

    [JsonIgnore]
    public bool IsShowingBranchCoverage
    {
      get { return Settings.Default.IsShowingBranchCoverage; }
      set { Settings.Default.IsShowingBranchCoverage = value; }
    }

    [JsonIgnore]
    public bool IsShowingExceptions
    {
      get { return Settings.Default.IsShowingExceptions; }
      set { Settings.Default.IsShowingExceptions = value; }
    }

    [JsonIgnore]
    public bool IsShowingPartialCoverage
    {
      get { return Settings.Default.IsShowingPartialCoverage; }
      set { Settings.Default.IsShowingPartialCoverage = value; }
    }

    [JsonIgnore]
    public Color CoveredColor
    {
      get { return Settings.Default.CoveredColor; }
      set { Settings.Default.CoveredColor = value; }
    }

    [JsonIgnore]
    public Color MixedColor
    {
      get { return Settings.Default.MixedColor; }
      set { Settings.Default.MixedColor = value; }
    }

    [JsonIgnore]
    public Color UncoveredColor
    {
      get { return Settings.Default.UncoveredColor; }
      set { Settings.Default.UncoveredColor = value; }
    }

    [JsonIgnore]
    public Color SelectedColor
    {
      get { return Settings.Default.SelectedColor; }
      set { Settings.Default.SelectedColor = value; }
    }

    [JsonIgnore]
    public Color ExceptionOriginColor
    {
      get { return Settings.Default.ExceptionOriginColor; }
      set { Settings.Default.ExceptionOriginColor = value; }
    }

    [JsonIgnore]
    public Color ExceptionTraceColor
    {
      get { return Settings.Default.ExceptionTraceColor; }
      set { Settings.Default.ExceptionTraceColor = value; }
    }
    #endregion

    #region Application settings
    [JsonIgnore]
    public string IssuesUrl
    {
      get { return Settings.Default.IssuesUrl; }
    }

    [JsonIgnore]
    public string FeedbackEmail
    {
      get { return Settings.Default.FeedbackEmail; }
    }

    [JsonIgnore]
    public string TelemetryKey
    {
      get { return Settings.Default.TelemetryKey; }
    }

    [JsonIgnore]
    public Guid InstallationId
    {
      get { return Settings.Default.InstallationId; }
      set { Settings.Default.InstallationId = value; }
    }

    [JsonIgnore]
    public bool IsTelemetryEnabled
    {
      get { return Settings.Default.IsTelemetryEnabled; }
      set { Settings.Default.IsTelemetryEnabled = value; }
    }

    [JsonIgnore]
    public bool IsTelemetryModeSelected
    {
      get { return Settings.Default.IsTelemetryModeSelected; }
      set { Settings.Default.IsTelemetryModeSelected = value; }
    }

    [JsonIgnore]
    public string SourceCodeUrl
    {
      get { return Settings.Default.SourceCodeUrl; }
    }
    #endregion

    private readonly IEditorContext _editorContext;
    private readonly IStorageController _storageController;

    [JsonIgnore]
    public string SolutionSettingsPath
    {
      get
      {
        if (_storageController.AxoCoverRoot != null)
        {
          return Path.Combine(_storageController.AxoCoverRoot, "settings.json");
        }
        else
        {
          return null;
        }
      }
    }

    public Options(IEditorContext editorContext, IStorageController storageController)
    {
      Settings.Default.SettingChanging += OnSettingChanging;
      _editorContext = editorContext;
      _storageController = storageController;

      editorContext.SolutionOpened += OnSolutionOpened;
    }

    private void OnSolutionOpened(object sender, EventArgs e)
    {
      TryLoadFrom(SolutionSettingsPath);
    }

    private bool _isFileLoading = false;

    private bool TryLoadFrom(string path)
    {
      if (File.Exists(path))
      {
        try
        {
          _isFileLoading = true;
          var text = File.ReadAllText(path);
          JsonConvert.PopulateObject(text, this);          
          return true;
        }
        catch (Exception e)
        {
          _editorContext.WriteToLog($"Could not load solution settings from: {path}.\r\n" + e.GetDescription());
        }
        finally
        {
          _isFileLoading = false;
        }
      }

      return false;
    }

    private bool TrySaveTo(string path)
    {
      if (path != null)
      {
        try
        {
          Directory.CreateDirectory(Path.GetDirectoryName(path));

          var fileInfo = new FileInfo(path);
          if (fileInfo.Exists && fileInfo.Attributes.HasFlag(FileAttributes.ReadOnly))
          {
            _editorContext.WriteToLog($"Could not save solution settings to: {path}. The settings file is read only.");
          }
          else
          {
            var text = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(path, text);
            return true;
          }
        }
        catch (Exception e)
        {
          _editorContext.WriteToLog($"Could not save solution settings to: {path}.\r\n" + e.GetDescription());
        }
      }

      return false;
    }

    private void OnSettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
    {
      if (!Equals(Settings.Default[e.SettingName], e.NewValue))
      {
        var isFileLoading = _isFileLoading;
        Application.Current.Dispatcher.BeginInvoke(() => OnSettingChanged(e.SettingName, isFileLoading));
      }
    }

    private void OnSettingChanged(string settingName, bool isFileLoading)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(settingName));

      if (!isFileLoading)
      {
        Settings.Default.Save();

        var propertyInfo = GetType().GetProperty(settingName);
        if (propertyInfo != null && propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>() == null)
        {
          TrySaveTo(SolutionSettingsPath);
        }
      }
    }
  }
}
