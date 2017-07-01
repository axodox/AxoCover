using System;
using System.Runtime.Serialization;

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
