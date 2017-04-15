using AxoCover.Common.Extensions;
using AxoCover.Common.Models;
using AxoCover.Common.Settings;
using AxoCover.Models.Data;
using AxoCover.Models.Extensions;
using AxoCover.Models.TestCaseProcessors;
using EnvDTE;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public class TestProvider : ITestProvider
  {
    public event EventHandler ScanningStarted;
    public event EventHandler ScanningFinished;
    private readonly IOptions _options;
    private readonly IEditorContext _editorContext;
    private readonly ITestCaseProcessor[] _testCaseProcessors;
    private readonly IEqualityComparer<TestCase> _testCaseEqualityComparer = new DelegateEqualityComparer<TestCase>((a, b) => a.Id == b.Id, p => p.Id.GetHashCode());
    private readonly TimeSpan _discoveryTimeout = TimeSpan.FromSeconds(30);

    public TestProvider(IEditorContext editorContext, IUnityContainer container, IOptions options)
    {
      _editorContext = editorContext;
      _testCaseProcessors = container.ResolveAll<ITestCaseProcessor>().ToArray();
      _options = options;
    }

    public async Task<TestSolution> GetTestSolutionAsync(Solution solution, string testSettings)
    {
      ScanningStarted?.Invoke(this, EventArgs.Empty);

      var testSolution = new TestSolution(solution.Properties.Item("Name").Value as string, solution.FileName);

      var testAdapterKinds = TestAdapterKinds.None;
      var projects = solution.GetProjects();
      foreach (Project project in projects)
      {
        var assemblyName = project.GetAssemblyName();

        var testAdapterKind = TestAdapterKinds.None;
        if (!project.IsDotNetUnitTestProject(out testAdapterKind))
        {
          if (assemblyName != null)
          {
            testSolution.CodeAssemblies.Add(assemblyName);
          }
          continue;
        }
        testAdapterKinds |= testAdapterKind;

        if (assemblyName != null)
        {
          testSolution.TestAssemblies.Add(assemblyName);
        }
        var outputFilePath = project.GetOutputDllPath();
        var testProject = new TestProject(testSolution, project.Name, outputFilePath);
      }

      if (testAdapterKinds == TestAdapterKinds.MSTestV1)
      {
        _options.TestAdapterMode = TestAdapterMode.Integrated;
      }

      if (!testAdapterKinds.HasFlag(TestAdapterKinds.MSTestV1))
      {
        _options.TestAdapterMode = TestAdapterMode.Standard;
      }

      await Task.Run(() =>
      {
        var assemblyPaths = testSolution
          .Children
          .OfType<TestProject>()
          .Select(p => p.OutputFilePath)
          .Where(p => File.Exists(p))
          .ToArray();

        using (var discoveryProcess = DiscoveryProcess.Create(AdapterExtensions.GetTestPlatformAssemblyPaths(_options.TestAdapterMode)))
        {
          try
          {
            var discoveryEvent = new ManualResetEvent(false);
            TestCase[] discoveryResults = null;
            _editorContext.WriteToLog(Resources.TestDiscoveryStarted);
            discoveryProcess.MessageReceived += (o, e) => _editorContext.WriteToLog(e.Value);
            discoveryProcess.DiscoveryCompleted += (o, e) => { discoveryResults = e.Value; discoveryEvent.Set(); };
            discoveryProcess.DiscoverTestsAsync(assemblyPaths, testSettings, AdapterExtensions.GetTestAdapterAssemblyPaths(_options.TestAdapterMode));

            if (!discoveryEvent.WaitOne(_discoveryTimeout))
            {
              throw new Exception("Test discovery timed out.");
            }

            var testCasesByAssembly = discoveryResults
              .Distinct(_testCaseEqualityComparer)
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

            _editorContext.WriteToLog(Resources.TestDiscoveryFinished);
          }
          catch (Exception e)
          {
            _editorContext.WriteToLog(Resources.TestExecutionFailed);
            _editorContext.WriteToLog(e.GetDescription());
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

      foreach (var testCase in testCases)
      {
        var testItemKind = CodeItemKind.Method;
        var testItemPath = testCase.FullyQualifiedName;
        var displayName = null as string;
        foreach (var testCaseProcessor in _testCaseProcessors)
        {
          if (testCaseProcessor.CanProcessCase(testCase))
          {
            testCaseProcessor.ProcessCase(testCase, ref testItemKind, ref testItemPath, ref displayName);
            break;
          }
        }

        AddTestItem(testItems, testItemKind, testItemPath, testCase, displayName);
      }
    }

    private static TestItem AddTestItem(Dictionary<string, TestItem> items, CodeItemKind itemKind, string itemPath, TestCase testCase = null, string displayName = null)
    {
      var nameParts = itemPath.SplitPath();
      var parentName = string.Join(string.Empty, nameParts.Take(nameParts.Length - 1));
      var itemName = nameParts[nameParts.Length - 1];

      TestItem parent;
      if (!items.TryGetValue(parentName, out parent))
      {
        switch (itemKind)
        {
          case CodeItemKind.Data:
            parent = AddTestItem(items, CodeItemKind.Method, parentName);
            break;
          case CodeItemKind.Class:
            if (itemName.StartsWith("+"))
            {
              parent = AddTestItem(items, CodeItemKind.Class, parentName);
            }
            else
            {
              parent = AddTestItem(items, CodeItemKind.Namespace, parentName);
            }
            break;
          case CodeItemKind.Method:
            parent = AddTestItem(items, CodeItemKind.Class, parentName);
            break;
          default:
            parent = AddTestItem(items, CodeItemKind.Namespace, parentName);
            break;
        }
      }

      var name = itemName.TrimStart('.', '+');
      TestItem item = null;
      switch (itemKind)
      {
        case CodeItemKind.Namespace:
          item = new TestNamespace(parent as TestNamespace, name);
          break;
        case CodeItemKind.Class:
          if (parent is TestClass)
          {
            item = new TestClass(parent as TestClass, name);
          }
          else
          {
            item = new TestClass(parent as TestNamespace, name);
          }
          break;
        case CodeItemKind.Method:
          item = new TestMethod(parent as TestClass, name, testCase);
          break;
        case CodeItemKind.Data:
          item = new TestMethod(parent as TestMethod, name, displayName, testCase);
          break;
        default:
          throw new NotImplementedException();
      }
      items.Add(itemPath, item);

      return item;
    }
  }
}
