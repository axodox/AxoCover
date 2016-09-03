using AxoCover.Models.Data;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VSLangProj;
using VSLangProj80;

namespace AxoCover.Models
{
  public class TestProvider : ITestProvider
  {
    private const string _unitTestReference = "Microsoft.VisualStudio.QualityTools.UnitTestFramework";

    private readonly ITestAssemblyScanner _testAssemblyScanner;

    public TestProvider(ITestAssemblyScanner testAssemblyScanner)
    {
      _testAssemblyScanner = testAssemblyScanner;
    }

    public TestSolution GetTestSolution(Solution solution)
    {
      var testSolution = new TestSolution(solution.Properties.Item("Name").Value as string);

      foreach (Project project in solution.Projects)
      {
        if (!IsDotNetUnitTestProject(project))
          continue;

        var outputFilePath = GetOutputDllPath(project);

        var testProject = new TestProject(testSolution, project.Name, outputFilePath);

        LoadTests(testProject);
      }

      return testSolution;
    }

    private static bool IsDotNetUnitTestProject(Project project)
    {
      var dotNetProject = project.Object as VSProject2;

      return dotNetProject != null && dotNetProject.References
        .OfType<Reference>()
        .Any(p => p.Name == _unitTestReference);
    }

    private static string GetOutputDllPath(Project project)
    {
      var outputDirectoryPath = project
        ?.ConfigurationManager
        ?.ActiveConfiguration
        ?.Properties
        .Item("OutputPath")
        .Value as string;

      if (outputDirectoryPath == null)
        return null;

      if (!Path.IsPathRooted(outputDirectoryPath))
      {
        outputDirectoryPath = Path.Combine(Path.GetDirectoryName(project.FullName), outputDirectoryPath);
      }

      var outputFileName = project.Properties.Item("OutputFileName").Value as string;
      return Path.Combine(outputDirectoryPath, outputFileName);
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
