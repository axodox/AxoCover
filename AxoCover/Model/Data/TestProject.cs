namespace AxoCover.Model.Data
{
  class TestProject : TestNamespace
  {
    public string OutputFilePath { get; private set; }

    public TestProject(string name, string outputFilePath)
      : base(null, name)
    {
      OutputFilePath = outputFilePath;
    }
  }
}
