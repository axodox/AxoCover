using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxoCover.Models.Adapters
{
  public class NUnit2Adapter : NUnitAdapter
  {
    public override string ExecutorUri => "executor://NUnitTestExecutor";

    public NUnit2Adapter() 
      : base(2, "NUnit.VisualStudio.TestAdapter.dll")
    {

    }
  }
}
