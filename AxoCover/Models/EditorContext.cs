using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;

namespace AxoCover.Models
{
  public class EditorContext : IEditorContext
  {
    public event EventHandler SolutionOpened, SolutionClosing;
    public event EventHandler BuildStarted, BuildFinished;

    public Solution Solution
    {
      get
      {
        return _context.Solution;
      }
    }

    public string MsTestPath
    {
      get
      {
        return Path.Combine(Path.GetDirectoryName(_context.FullName), @"mstest.exe");
      }
    }

    public bool IsBuilding { get; private set; }

    private DTE _context;

    private SolutionEvents _solutionEvents;

    private BuildEvents _buildEvents;

    private IVsOutputWindow _outputWindow;
    private IVsOutputWindowPane _outputPane;

    public EditorContext()
    {
      //Initialize events
      _context = Package.GetGlobalService(typeof(DTE)) as DTE;
      _solutionEvents = _context.Events.SolutionEvents;
      _buildEvents = _context.Events.BuildEvents;
      _solutionEvents.Opened += OnSolutionOpened;
      _solutionEvents.BeforeClosing += OnSolutionClosing;
      _buildEvents.OnBuildBegin += OnBuildBegin;
      _buildEvents.OnBuildDone += OnBuildDone;

      //Initialize log pane
      _outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
      _outputWindow.CreatePane(VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "AxoCover", 1, 1);
      _outputWindow.GetPane(VSConstants.OutputWindowPaneGuid.GeneralPane_guid, out _outputPane);
    }

    private void OnSolutionOpened()
    {
      SolutionOpened?.Invoke(this, EventArgs.Empty);
    }

    private void OnSolutionClosing()
    {
      SolutionClosing?.Invoke(this, EventArgs.Empty);
    }

    private void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action)
    {
      IsBuilding = true;
      BuildStarted?.Invoke(this, EventArgs.Empty);
    }

    private void OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
    {
      IsBuilding = false;
      BuildFinished?.Invoke(this, EventArgs.Empty);
    }

    public void BuildSolution()
    {
      _context.ExecuteCommand("Build.BuildSolution");
    }

    public void WriteToLog(string message)
    {
      _outputPane?.OutputStringThreadSafe(message + Environment.NewLine);
    }

    public void ActivateLog()
    {
      _outputPane?.Activate();
    }

    public void ClearLog()
    {
      _outputPane?.Clear();
    }
  }
}
