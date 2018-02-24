using AxoCover.Common.Models;
using AxoCover.Common.Runner;
using AxoCover.Common.Settings;
using AxoCover.Models.Testing.Data;
using EnvDTE;

namespace AxoCover.Models.Testing.Adapters
{
  public interface ITestAdapter
  {
    string Name { get; }

    string ExecutorUri { get; }

    TestAdapterMode Mode { get; }

    bool IsTestSource(Project project);

    bool CanProcessCase(TestCase testCase);

    void ProcessCase(TestCase testCase, ref CodeItemKind testItemKind, ref string testItemPath, ref string displayName);

    TestAdapterOptions GetLoadingOptions();
  }
}
