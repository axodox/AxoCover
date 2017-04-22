using EnvDTE;
using System;

namespace AxoCover.Models
{
  public interface IEditorContext
  {
    bool IsBuilding { get; }
    Solution Solution { get; }
    string RootPath { get; }
    string Version { get; }

    event EventHandler BuildFinished;
    event EventHandler BuildStarted;
    event EventHandler SolutionClosing;
    event EventHandler SolutionOpened;

    void BuildSolution();
    void WriteToLog(string message);
    void ActivateLog();
    void ClearLog();
    void Restart();

    void NavigateToClass(string projectName, string className);
    void NavigateToMethod(string projectName, string className, string methodName);
    void NavigateToFile(string path, int? line = null);
    void DebugContextualTest();
    void OpenPathInExplorer(string path);
    bool AttachToProcess(int pid);
    bool DetachFromProcess(int pid);
    void WaitForDetach();
  }
}