using AxoCover.Common.Models;
using AxoCover.Models.Data;

namespace AxoCover.Models.TestCaseProcessors
{
  public interface ITestCaseProcessor
  {
    bool CanProcessCase(TestCase testCase);

    void ProcessCase(TestCase testCase, ref CodeItemKind testItemKind, ref string testItemPath, ref string displayName);
  }
}
