using AxoCover.Models.Data;
using AxoCover.Models.Extensions;
using EnvDTE;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public class TestProvider : ITestProvider
  {
    public event EventHandler ScanningStarted;

    public event EventHandler ScanningFinished;

    private IEditorContext _editorContext;

    public TestProvider(IEditorContext editorContext)
    {
      _editorContext = editorContext;
    }

    public async Task<TestSolution> GetTestSolutionAsync(Solution solution)
    {
      ScanningStarted?.Invoke(this, EventArgs.Empty);

      var testSolution = new TestSolution(solution.Properties.Item("Name").Value as string);

      var projects = solution.GetProjects();
      foreach (Project project in projects)
      {
        var assemblyName = project.GetAssemblyName();

        if (!project.IsDotNetUnitTestProject())
        {
          if (assemblyName != null)
          {
            testSolution.CodeAssemblies.Add(assemblyName);
          }
          continue;
        }

        if (assemblyName != null)
        {
          testSolution.TestAssemblies.Add(assemblyName);
        }
        var outputFilePath = project.GetOutputDllPath();
        var testProject = new TestProject(testSolution, project.Name, outputFilePath);
      }

      await Task.Run(() =>
      {
        var assemblyPaths = testSolution
          .Children
          .OfType<TestProject>()
          .Select(p => p.OutputFilePath)
          .Where(p => File.Exists(p))
          .ToArray();

        _editorContext.ActivateLog();
        using (var discoveryProcess = DiscoveryProcess.Create())
        {
          discoveryProcess.MessageReceived += (o, e) => _editorContext.WriteToLog(e.Value);

          var testCasesByAssembly = discoveryProcess.DiscoverTests(assemblyPaths, null)
            .GroupBy(p => p.Source)
            .ToDictionary(p => p.Key, p => p.ToArray(), StringComparer.OrdinalIgnoreCase);

          foreach (TestProject testProject in testSolution.Children.ToArray())
          {
            var testCases = testCasesByAssembly.TryGetValue(testProject.OutputFilePath);
            if (testCases != null)
            {
              LoadTests(testProject, testCases);
            }

            var isEmpty = !testProject
              .Flatten<TestItem>(p => p.Children, false)
              .Any(p => p.Kind == CodeItemKind.Method);
            if (isEmpty)
            {
              testProject.Remove();
            }
          }
        }
      });

      ScanningFinished?.Invoke(this, EventArgs.Empty);

      return testSolution;
    }

    private Regex _xUnitDisplayNameRegex = new Regex(@"(?>(?<path>[\w\.]*))(?>(?<arguments>.+))");
    private Regex _xUnitFullyQualifiedNameRegex = new Regex(@"(?>(?<path>[\w\.]*)) \((?>(?<id>\w+))\)");

    private void LoadTests(TestProject testProject, TestCase[] testCases)
    {
      var testItems = new Dictionary<string, TestItem>()
      {
        { "", testProject }
      };

      foreach (var testCase in testCases)
      {
        var testItemKind = CodeItemKind.Method;
        var testItemPath = testCase.FullyQualifiedName;
        var displayName = null as string;
        if (testCase.ExecutorUri.ToString().Contains("xunit", StringComparison.OrdinalIgnoreCase))
        {
          var fullyQualifiedNameMatch = _xUnitFullyQualifiedNameRegex.Match(testCase.FullyQualifiedName);
          if (fullyQualifiedNameMatch.Success)
          {
            var displayNameMatch = _xUnitDisplayNameRegex.Match(testCase.DisplayName);
            if (displayNameMatch.Success)
            {
              testItemKind = CodeItemKind.Data;
              displayName = displayNameMatch.Groups["arguments"].Value;
              testItemPath = fullyQualifiedNameMatch.Groups["path"].Value + "." + fullyQualifiedNameMatch.Groups["id"].Value;
            }
            else
            {
              testItemPath = fullyQualifiedNameMatch.Groups["path"].Value;
            }
          }
        }

        AddTestItem(testItems, testItemKind, testItemPath, testCase, displayName);
      }
    }

    private static TestItem AddTestItem(Dictionary<string, TestItem> items, CodeItemKind itemKind, string itemPath, TestCase testCase = null, string displayName = null)
    {
      var nameParts = itemPath.Split('.');
      var parentName = string.Join(".", nameParts.Take(nameParts.Length - 1));
      var itemName = nameParts[nameParts.Length - 1];

      TestItem parent;
      if (!items.TryGetValue(parentName, out parent))
      {
        switch (itemKind)
        {
          case CodeItemKind.Data:
            parent = AddTestItem(items, CodeItemKind.Method, parentName);
            break;
          case CodeItemKind.Method:
            parent = AddTestItem(items, CodeItemKind.Class, parentName);
            break;
          default:
            parent = AddTestItem(items, CodeItemKind.Namespace, parentName);
            break;
        }
      }

      TestItem item = null;
      switch (itemKind)
      {
        case CodeItemKind.Namespace:
          item = new TestNamespace(parent as TestNamespace, itemName);
          break;
        case CodeItemKind.Class:
          item = new TestClass(parent as TestNamespace, itemName);
          break;
        case CodeItemKind.Method:
          item = new TestMethod(parent as TestClass, itemName, testCase);
          break;
        case CodeItemKind.Data:
          item = new TestMethod(parent as TestMethod, itemName, displayName, testCase);
          break;
        default:
          throw new NotImplementedException();
      }
      items.Add(itemPath, item);

      return item;
    }
  }
}
