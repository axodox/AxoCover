using AxoCover.Model.Data;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VSLangProj;
using VSLangProj80;

namespace AxoCover.Model
{
  class TestProvider
  {
    private const string _unitTestReference = "Microsoft.VisualStudio.QualityTools.UnitTestFramework";
    private readonly DTE _context;

    public TestProvider(DTE context)
    {
      if (context == null)
        throw new ArgumentNullException(nameof(context));

      _context = context;
    }

    public IEnumerable<TestProject> GetTests()
    {
      foreach (Project project in _context.Solution.Projects)
      {
        if (!IsDotNetUnitTestProject(project))
          continue;

        var outputFilePath = GetOutputDllPath(project);

        var testProject = new TestProject(project.Name, outputFilePath);

        LoadTests(testProject);

        yield return testProject;
      }
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

    private static void LoadTests(TestProject testProject)
    {
      if (!File.Exists(testProject.OutputFilePath))
        return;

      using (var testAssemblyScanner = new Isolated<TestAssemblyScanner>())
      {
        var testItemPaths = testAssemblyScanner.Value.ScanAssemblyForTests(testProject.OutputFilePath);
        var testItems = new Dictionary<string, TestItem>()
        {
          { "", testProject }
        };

        foreach (var testPath in testItemPaths)
        {
          AddTestItem(testItems, TestItemKind.Method, testPath);
        }
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
      return item;
    }
  }
}
