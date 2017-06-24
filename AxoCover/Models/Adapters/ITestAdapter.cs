using AxoCover.Common.Models;
using AxoCover.Common.Settings;
using AxoCover.Models.Data;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxoCover.Models.Adapters
{
  public interface ITestAdapter
  {
    string Name { get; }

    TestAdapterMode Mode { get; }

    bool IsTestSource(Project project);

    bool CanProcessCase(TestCase testCase);

    void ProcessCase(TestCase testCase, ref CodeItemKind testItemKind, ref string testItemPath, ref string displayName);

    AdapterLoadingOptions GetLoadingOptions(Project project);
  }
}
