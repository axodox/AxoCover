using AxoCover.Models.Data;
using AxoCover.Models.Extensions;
using EnvDTE;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
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

    private void LoadTests(TestProject testProject, TestCase[] testCases)
    {
      var testItems = new Dictionary<string, TestItem>()
      {
        { "", testProject }
      };

      var index = 0;
      foreach (var testCase in testCases)
      {
        AddTestItem(testItems, CodeItemKind.Method, testCase.FullyQualifiedName, index++, testCase);
      }
    }

    private static TestItem AddTestItem(Dictionary<string, TestItem> items, CodeItemKind itemKind, string itemPath, int index, TestCase testCase)
    {
      var nameParts = itemPath.Split('.');
      var parentName = string.Join(".", nameParts.Take(nameParts.Length - 1));
      var itemName = nameParts[nameParts.Length - 1];

      TestItem parent;
      if (!items.TryGetValue(parentName, out parent))
      {
        if (itemKind == CodeItemKind.Method)
        {
          parent = AddTestItem(items, CodeItemKind.Class, parentName, index, testCase);
        }
        else
        {
          parent = AddTestItem(items, CodeItemKind.Namespace, parentName, index, testCase);
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
            Case = testCase
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
