namespace AxoCover.Models
{
  public interface ITestAssemblyScanner
  {
    string[] ScanAssemblyForTests(string assemblyPath);
  }
}