using EnvDTE;
using Microsoft.VisualStudio.Shell;
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

    public EditorContext()
    {
      _context = Package.GetGlobalService(typeof(DTE)) as DTE;
      _solutionEvents = _context.Events.SolutionEvents;
      _buildEvents = _context.Events.BuildEvents;
      _solutionEvents.Opened += OnSolutionOpened;
      _solutionEvents.BeforeClosing += OnSolutionClosing;
      _buildEvents.OnBuildBegin += OnBuildBegin;
      _buildEvents.OnBuildDone += OnBuildDone;
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
  }
}
