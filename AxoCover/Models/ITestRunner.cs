using AxoCover.Models.Data;

namespace AxoCover.Models
{
  public interface ITestRunner
  {
    void RunTests(TestItem testItem);
  }
}