using System.IO;

namespace AxoCover.Models.Data
{
  public class TestProject : TestNamespace
  {
    public string OutputFilePath { get; private set; }

    public string OutputDirectory
    {
      get
      {
        return Path.GetDirectoryName(OutputFilePath);
      }
    }

    public TestProject(TestSolution parent, string name, string outputFilePath)
      : base(parent, name, TestItemKind.Project)
    {
      OutputFilePath = outputFilePath;
    }
  }
}
