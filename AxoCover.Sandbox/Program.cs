using AxoCover.Models;

namespace AxoCover.Sandbox
{
  class Program
  {
    static void Main(string[] args)
    {
      var containers = new[]
      {
        @"D:\Documents\Visual Studio 2015\Projects\CoverageTest\MsUnitTests\bin\Debug\MsUnitTests.dll",
        @"D:\Documents\Visual Studio 2015\Projects\CoverageTest\NUnitTests\bin\Debug\NUnitTests.dll",
        @"D:\Documents\Visual Studio 2015\Projects\CoverageTest\xUnitTests\bin\Debug\xUnitTests.dll"
      };

      var discoveryProcess = DiscoveryProcess.Create();
      var tests = discoveryProcess.DiscoverTests(containers, null);

      var executionProcess = ExecutionProcess.Create();
      executionProcess.RunTests(tests, null);
      //Thread.Sleep(Timeout.Infinite);
    }
  }
}
