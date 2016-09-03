using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;

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

    public bool IsBuilding { get; private set; }

    private DTE _context;

    public EditorContext()
    {
      _context = Package.GetGlobalService(typeof(DTE)) as DTE;
      _context.Events.SolutionEvents.Opened += () => SolutionOpened?.Invoke(this, EventArgs.Empty);
      _context.Events.SolutionEvents.BeforeClosing += () => SolutionClosing?.Invoke(this, EventArgs.Empty);
      _context.Events.BuildEvents.OnBuildBegin += OnBuildBegin; ;
      _context.Events.BuildEvents.OnBuildDone += OnBuildDone;
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
  }
}
