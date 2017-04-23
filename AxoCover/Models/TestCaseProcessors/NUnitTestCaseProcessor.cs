using AxoCover.Common.Extensions;
using AxoCover.Common.Models;
using AxoCover.Models.Data;
using AxoCover.Models.Extensions;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AxoCover.Models.TestCaseProcessors
{
  public class NUnitTestCaseProcessor : ITestCaseProcessor
  {
    private Regex _nameWithArgumentsRegex = new Regex(@"^(?>(?<name>[\w.+<>]*)(?<arguments>(?>\((?>[^()'""]|'.'|""([^""]|(?<=\\)"")*""|(?<n>\()|(?<-n>\)))*\)(?(n)(?!)))))(?>\|[\d-]*)?$");

    public bool CanProcessCase(TestCase testCase)
    {
      return testCase.ExecutorUri.ToString().Contains("nunit", StringComparison.OrdinalIgnoreCase);
    }

    public void ProcessCase(TestCase testCase, ref CodeItemKind testItemKind, ref string testItemPath, ref string displayName)
    {
      var pathItems = testItemPath.SplitPath();
      var nameWithArgumentsMatch = _nameWithArgumentsRegex.Match(pathItems.Last());
      if (nameWithArgumentsMatch.Success)
      {
        testItemKind = CodeItemKind.Data;
        displayName = nameWithArgumentsMatch.Groups["arguments"].Value;
        testItemPath = string.Join(string.Empty, pathItems.Take(pathItems.Length - 1)) + nameWithArgumentsMatch.Groups["name"].Value + ".Instance" + testCase.Id.ToString("N");
      }
    }
  }
}
