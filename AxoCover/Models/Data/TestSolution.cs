using System.Collections.Generic;

namespace AxoCover.Models.Data
{
  public class TestSolution : TestNamespace
  {
    public List<string> CodeAssemblies { get; private set; }

    public List<string> TestAssemblies { get; private set; }

    public TestSolution(string name)
      : base(null, name, CodeItemKind.Solution)
    {
      CodeAssemblies = new List<string>();
      TestAssemblies = new List<string>();
    }
  }
}
