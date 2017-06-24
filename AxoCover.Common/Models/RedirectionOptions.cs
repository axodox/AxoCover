using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxoCover.Common.Models
{
  [Flags]
  public enum RedirectionOptions
  {
    None = 0,
    ExcludeNonexistentDirectories = 1,
    ExcludeNonexistentFiles = 2,
    IncludeBaseDirectory = 4
  }
}
