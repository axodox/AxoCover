using System;

namespace AxoCover.Models.Data
{
  public interface ITestResult
  {
    TimeSpan Duration { get; }
    string ErrorMessage { get; }
    TestMethod Method { get; }
    TestState Outcome { get; }
    StackItem[] StackTrace { get; }
  }
}