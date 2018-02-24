using System;

namespace AxoCover.Models.Testing.Data
{
  [Flags]
  public enum TestState
  {
    Unknown = 0,
    Passed = 1,
    Skipped = 2,
    NotExecuted = 2,
    Inconclusive = 2,
    Failed = 3,
    Error = 3,
    Scheduled = 4
  }
}
