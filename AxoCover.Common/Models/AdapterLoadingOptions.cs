using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxoCover.Common.Models
{
  public class AdapterLoadingOptions
  {
    public string AssemblyPath { get; set; }

    public string[] RedirectedAssemblies { get; set; } = new string[0];

    public RedirectionOptions RedirectionOptions { get; set; } = RedirectionOptions.None;

    public bool IsRedirectingAssemblies
    {
      get
      {
        return RedirectedAssemblies.Length > 0;
      }
    }
  }
}
