using System.Collections;
using System.IO;

namespace AxoCover.Models.Data
{
  public class TestProject : TestNamespace
  {
    public string OutputFilePath { get; private set; }

    public string[] TestAdapters { get; private set; }  

    public string OutputDirectory
    {
      get
      {
        return Path.GetDirectoryName(OutputFilePath);
      }
    }

    public TestProject(TestSolution parent, string name, string outputFilePath, string[] testAdapters)
      : base(parent, name, CodeItemKind.Project)
    {
      OutputFilePath = Path.GetFullPath(outputFilePath);
      TestAdapters = testAdapters;
    }
  }
}
