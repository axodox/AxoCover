using System;

namespace AxoCover.Models.Data
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
    Error = 4,
    Scheduled = 4
  }
}
