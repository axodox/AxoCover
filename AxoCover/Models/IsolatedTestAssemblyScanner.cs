namespace AxoCover.Models
{
  public class IsolatedTestAssemblyScanner : ITestAssemblyScanner
  {
    public string[] ScanAssemblyForTests(string assemblyPath)
    {
      using (var testAssemblyScanner = new Isolated<TestAssemblyScanner>())
      {
        return testAssemblyScanner.Value.ScanAssemblyForTests(assemblyPath);
      }
    }
  }
}
