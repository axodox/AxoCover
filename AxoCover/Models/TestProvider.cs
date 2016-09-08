using AxoCover.Models.Data;
using AxoCover.Models.Extensions;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public class TestProvider : ITestProvider
  {
    private readonly ITestAssemblyScanner _testAssemblyScanner;

    public TestProvider(ITestAssemblyScanner testAssemblyScanner)
    {
      _testAssemblyScanner = testAssemblyScanner;
    }

    public async Task<TestSolution> GetTestSolutionAsync(Solution solution)
    {
      var testSolution = new TestSolution(solution.Properties.Item("Name").Value as string);

      var projects = solution.GetProjects();
      foreach (Project project in projects)
      {
        if (!project.IsDotNetUnitTestProject())
          continue;

        var outputFilePath = project.GetOutputDllPath();

        var testProject = new TestProject(testSolution, project.Name, outputFilePath);
      }

      await Task.Run(() =>
      {
        foreach (TestProject testProject in testSolution.Children.ToArray())
        {
          LoadTests(testProject);

          if (testProject.TestCount == 0)
          {
            testProject.Remove();
          }
        }
      });

      return testSolution;
    }

    private void LoadTests(TestProject testProject)
    {
      if (!File.Exists(testProject.OutputFilePath))
        return;

      var testItemPaths = _testAssemblyScanner.ScanAssemblyForTests(testProject.OutputFilePath);
      var testItems = new Dictionary<string, TestItem>()
      {
        { "", testProject }
      };

      foreach (var testPath in testItemPaths)
      {
        AddTestItem(testItems, TestItemKind.Method, testPath);
      }
    }

    private static TestItem AddTestItem(Dictionary<string, TestItem> items, TestItemKind itemKind, string itemPath)
    {
      var nameparts = itemPath.Split('.');

      var parentName = string.Join(".", nameparts.Take(nameparts.Length - 1));
      var itemName = nameparts[nameparts.Length - 1];

      TestItem parent;
      if (!items.TryGetValue(parentName, out parent))
      {
        if (itemKind == TestItemKind.Method)
        {
          parent = AddTestItem(items, TestItemKind.Class, parentName);
        }
        else
        {
          parent = AddTestItem(items, TestItemKind.Namespace, parentName);
        }
      }

      TestItem item = null;
      switch (itemKind)
      {
        case TestItemKind.Namespace:
          item = new TestNamespace(parent as TestNamespace, itemName);
          break;
        case TestItemKind.Class:
          item = new TestClass(parent as TestNamespace, itemName);
          break;
        case TestItemKind.Method:
          item = new TestMethod(parent as TestClass, itemName);
          break;
        default:
          throw new NotImplementedException();
      }
      items.Add(itemPath, item);

      return item;
    }
  }
}
