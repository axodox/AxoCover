using EnvDTE;
using System;

namespace AxoCover.Models
{
  public interface IEditorContext
  {
    bool IsBuilding { get; }
    Solution Solution { get; }
    string MsTestPath { get; }

    event EventHandler BuildFinished;
    event EventHandler BuildStarted;
    event EventHandler SolutionClosing;
    event EventHandler SolutionOpened;

    void BuildSolution();
    void WriteToLog(string message);
    void ActivateLog();
  }
}