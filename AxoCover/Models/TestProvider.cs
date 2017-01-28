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
    public event EventHandler ScanningStarted;

    public event EventHandler ScanningFinished;

    private readonly ITestAssemblyScanner _testAssemblyScanner;

    public TestProvider(ITestAssemblyScanner testAssemblyScanner)
    {
      _testAssemblyScanner = testAssemblyScanner;
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
        foreach (TestProject testProject in testSolution.Children.ToArray())
        {
          LoadTests(testProject);

          var isEmpty = !testProject
            .Flatten<TestItem>(p => p.Children, false)
            .Any(p => p.Kind == CodeItemKind.Method);
          if (isEmpty)
          {
            testProject.Remove();
          }
        }
      });

      ScanningFinished?.Invoke(this, EventArgs.Empty);

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

      var index = 0;
      foreach (var testPath in testItemPaths)
      {
        var itemPath = testPath;
        var isIgnored = itemPath.StartsWith(TestAssemblyScanner.IgnorePrefix);
        if (isIgnored)
        {
          itemPath = itemPath.Substring(TestAssemblyScanner.IgnorePrefix.Length);
        }
        AddTestItem(testItems, CodeItemKind.Method, itemPath, index++, isIgnored);
      }
    }

    private static TestItem AddTestItem(Dictionary<string, TestItem> items, CodeItemKind itemKind, string itemPath, int index, bool isIgnored)
    {
      var nameParts = itemPath.Split('.');
      var parentName = string.Join(".", nameParts.Take(nameParts.Length - 1));
      var itemName = nameParts[nameParts.Length - 1];

      TestItem parent;
      if (!items.TryGetValue(parentName, out parent))
      {
        if (itemKind == CodeItemKind.Method)
        {
          parent = AddTestItem(items, CodeItemKind.Class, parentName, index, isIgnored);
        }
        else
        {
          parent = AddTestItem(items, CodeItemKind.Namespace, parentName, index, isIgnored);
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
          item = new TestMethod(parent as TestClass, itemName)
          {
            Index = index,
            IsIgnored = isIgnored
          };
          break;
        default:
          throw new NotImplementedException();
      }
      items.Add(itemPath, item);

      return item;
    }
  }
}
