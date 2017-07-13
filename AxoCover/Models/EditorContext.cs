using AxoCover.Models.Extensions;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Linq;
using System.Windows;

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

    public string RootPath
    {
      get
      {
        return Path.GetDirectoryName(_context.FullName);
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
      if (!string.IsNullOrEmpty(Solution.FileName))
      {
        SolutionOpened?.Invoke(this, EventArgs.Empty);
      }
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

    public void NavigateToClass(string projectName, string className)
    {
      var classElement = FindClass(projectName, className);
      NavigateToCodeElement(classElement);
    }

    public void NavigateToMethod(string projectName, string className, string methodName)
    {
      CodeElement classElement = FindClass(projectName, className);

      //Handle generic methods
      methodName = (className + "." + methodName).CleanPath();

      var methodElement = classElement?
        .GetMethods()
        .FirstOrDefault(p => p.FullName.CleanPath() == methodName);

      NavigateToCodeElement(methodElement);
    }

    private CodeElement FindClass(string projectName, string className)
    {
      //Handle generic and parametrized classes 
      className = className.CleanPath();

      return _context.Solution
              .GetProjects()
              .FirstOrDefault(p => p.Name == projectName)?
              .GetSourceFiles()
              .SelectMany(p => p.CodeElements.GetClasses())
              .FirstOrDefault(p => p.FullName.CleanPath() == className);
    }

    private void NavigateToCodeElement(CodeElement codeElement)
    {
      if (codeElement != null)
      {
        var path = codeElement.GetFilePath();
        var line = codeElement.StartPoint.Line;
        NavigateToFile(path, line);
      }
    }

    public void NavigateToFile(string path, int? line = null)
    {
      try
      {
        _context.ItemOperations.OpenFile(path);

        if (line != null)
        {
          try
          {
            _context.ExecuteCommand("GotoLn", line.ToString());
          }
          catch
          {
            //In some cases the go to line command is not available
          }
        }
      }
      catch
      {
        MessageBox.Show(Application.Current.MainWindow, string.Format(Resources.CannotOpenFile, path), Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    public void DebugContextualTest()
    {
      try
      {
        _context.ExecuteCommand("TestExplorer.DebugAllTestsInContext");
      }
      catch
      {
        //In some cases the go to line command is not available
      }
    }

    public void OpenPathInExplorer(string path)
    {
      try
      {
        System.Diagnostics.Process.Start("explorer.exe", path);
      }
      catch
      {
        MessageBox.Show(Application.Current.MainWindow, string.Format(Resources.CannotOpenPath, path), Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    public string Version
    {
      get
      {
        return $"{_context.Name} {_context.Version} {_context.Edition}";
      }
    }

    public bool AttachToProcess(int pid)
    {
      var process = _context.Debugger.LocalProcesses
        .OfType<Process>()
        .FirstOrDefault(p => p.ProcessID == pid);

      if (process != null)
      {
        process.Attach();
        return true;
      }
      else
      {
        return false;
      }
    }

    public bool DetachFromProcess(int pid)
    {
      var process = _context.Debugger.LocalProcesses
        .OfType<Process>()
        .FirstOrDefault(p => p.ProcessID == pid);

      if (process != null)
      {
        try
        {
          process.Detach();
          return true;
        }
        catch { }
      }

      return false;
    }

    public void WaitForDetach()
    {
      while (_context.Debugger.CurrentProcess != null)
      {
        System.Threading.Thread.Sleep(1000);
      }
    }

    public void Restart()
    {
      var executableFile = _context.FullName;
      var solutionFile = Solution?.FullName;
      _context.Solution?.Close(true);

      if (solutionFile != null)
      {
        System.Diagnostics.Process.Start(executableFile, $"\"{solutionFile}\"");
      }
      else
      {
        System.Diagnostics.Process.Start(executableFile);
      }
      _context.Quit();
    }
  }
}
