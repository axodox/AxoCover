using AxoCover.Common.Extensions;
using AxoCover.Models.Data;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AxoCover.Models.TestCaseProcessors
{
  public class NUnitTestCaseProcessor : ITestCaseProcessor
  {
    private Regex _fullyQualifiedNameRegex = new Regex(@"^(?>(?<path>[\w\.]*))(?<arguments>\(.*\))$");

    public bool CanProcessCase(TestCase testCase)
    {
      return testCase.ExecutorUri.ToString().Contains("nunit", StringComparison.OrdinalIgnoreCase);
    }

    public void ProcessCase(TestCase testCase, ref CodeItemKind testItemKind, ref string testItemPath, ref string displayName)
    {
      var fullyQualifiedNameMatch = _fullyQualifiedNameRegex.Match(testCase.FullyQualifiedName);
      if (fullyQualifiedNameMatch.Success)
      {
        testItemKind = CodeItemKind.Data;
        displayName = fullyQualifiedNameMatch.Groups["arguments"].Value;
        testItemPath = fullyQualifiedNameMatch.Groups["path"].Value + "." + testCase.Id;
      }
    }
  }
}
