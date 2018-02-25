using System;

namespace AxoCover.Common.Models
{
  [Flags]
  public enum FileRedirectionOptions
  {
    None = 0,
    ExcludeNonexistentDirectories = 1,
    ExcludeNonexistentFiles = 2,
    IncludeBaseDirectory = 4
  }
}
