using AxoCover.Common.Settings;
using System;
using System.ComponentModel;
using System.Windows.Media;

namespace AxoCover.Models
{
  public interface IOptions
  {
    Color CoveredColor { get; set; }
    Color ExceptionOriginColor { get; set; }
    Color ExceptionTraceColor { get; set; }
    string ExcludeAttributes { get; set; }
    string ExcludeDirectories { get; set; }
    string ExcludeFiles { get; set; }
    string FeedbackEmail { get; }
    string Filters { get; set; }
    Guid InstallationId { get; set; }
    bool IsCoveringByTest { get; set; }
    bool IsExcludingTestAssemblies { get; set; }
    bool IsIncludingSolutionAssemblies { get; set; }
    bool IsShowingBranchCoverage { get; set; }
    bool IsShowingExceptions { get; set; }
    bool IsShowingLineCoverage { get; set; }
    bool IsShowingPartialCoverage { get; set; }
    string IssuesUrl { get; }
    bool IsTelemetryEnabled { get; set; }
    bool IsTelemetryModeSelected { get; set; }
    Color MixedColor { get; set; }
    string TestSettings { get; set; }
    Color SelectedColor { get; set; }
    string SolutionSettingsPath { get; }
    string SourceCodeUrl { get; }
    string TelemetryKey { get; }
    TestApartmentState TestApartmentState { get; set; }
    TestPlatform TestPlatform { get; set; }
    TestAdapterMode TestAdapterMode { get; set; }
    string TestRunner { get; set; }
    Color UncoveredColor { get; set; }
    bool IsMergingByHash { get; set; }
    bool IsSkippingAutoProps { get; set; }

    event PropertyChangedEventHandler PropertyChanged;
  }
}