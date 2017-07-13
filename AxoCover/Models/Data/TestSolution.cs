using System.Collections.Generic;

namespace AxoCover.Models.Data
{
  public class TestSolution : TestNamespace
  {
    public List<string> CodeAssemblies { get; private set; }

    public List<string> TestAssemblies { get; private set; }

    public string FilePath { get; set; }

    public TestSolution(string name, string filePath)
      : base(null, name, CodeItemKind.Solution)
    {
      FilePath = filePath;
      CodeAssemblies = new List<string>();
      TestAssemblies = new List<string>();
    }
  }
}
