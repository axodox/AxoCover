using System;

namespace AxoCover.Models.Testing.Data
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
