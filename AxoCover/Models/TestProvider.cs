using AxoCover.Common.Extensions;
using AxoCover.Common.Models;
using AxoCover.Common.Runner;
using AxoCover.Common.Settings;
using AxoCover.Models.Adapters;
using AxoCover.Models.Data;
using AxoCover.Models.Extensions;
using EnvDTE;
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
    private readonly ITestAdapterRepository _testAdapterRepository;
    private readonly IEqualityComparer<TestCase> _testCaseEqualityComparer = new DelegateEqualityComparer<TestCase>((a, b) => a.Id == b.Id, p => p.Id.GetHashCode());
    private readonly ITelemetryManager _telemetryManager;
    private readonly TimeSpan _discoveryTimeout = TimeSpan.FromSeconds(30);
    private int _sessionCount = 0;

    public bool IsActive
    {
      get
      {
        return _sessionCount > 0;
      }
    }

    public TestProvider(IEditorContext editorContext, ITestAdapterRepository testAdapterRepository, IOptions options, ITelemetryManager telemetryManager)
    {
      _editorContext = editorContext;
      _testAdapterRepository = testAdapterRepository;
      _options = options;
      _telemetryManager = telemetryManager;
    }

    public async Task<TestSolution> GetTestSolutionAsync(Solution solution, string testSettings)
    {
      try
      {
        Interlocked.Increment(ref _sessionCount);
        ScanningStarted?.Invoke(this, EventArgs.Empty);

        var testSolution = new TestSolution(solution.Properties.Item("Name").Value as string, solution.FileName);

        var testAdapters = new HashSet<ITestAdapter>();
        var testAdapterModes = new HashSet<TestAdapterMode>();
        var projects = solution.GetProjects();
        foreach (Project project in projects)
        {
          var assemblyName = project.GetAssemblyName();

          if (string.IsNullOrWhiteSpace(assemblyName)) continue;

          var isTestSource = false;
          var testAdapterNames = new List<string>();
          foreach (var testAdapter in _testAdapterRepository.Adapters.Values)
          {
            if (testAdapter.IsTestSource(project))
            {
              testAdapters.Add(testAdapter);
              testAdapterModes.Add(testAdapter.Mode);
              testAdapterNames.Add(testAdapter.Name);
              isTestSource = true;
            }
          }

          if (isTestSource)
          {
            var outputFilePath = project.GetOutputDllPath();
            var testProject = new TestProject(testSolution, project.Name, outputFilePath, testAdapterNames.ToArray());

            testSolution.TestAssemblies.Add(assemblyName);
          }
          else
          {
            testSolution.CodeAssemblies.Add(assemblyName);
          }
        }

        if (testAdapterModes.Count == 1)
        {
          _options.TestAdapterMode = testAdapterModes.First();
        }

        foreach (var testAdapter in testAdapters.ToArray())
        {
          if(testAdapter.Mode != _options.TestAdapterMode)
          {
            testAdapters.Remove(testAdapter);
          }
        }

        await Task.Run(() =>
        {
          try
          {
            var testDiscoveryTasks = testAdapters
             .Select(p => new TestDiscoveryTask()
             {
               TestAssemblyPaths = testSolution.Children
                 .OfType<TestProject>()
                 .Where(q => q.TestAdapters.Contains(p.Name))
                 .Select(q => q.OutputFilePath)
                 .ToArray(),
               TestAdapterOptions = p
                 .GetLoadingOptions()
                 .Do(q => q.IsRedirectingAssemblies = _options.IsRedirectingFrameworkAssemblies)
             })
             .ToArray();

            using (var discoveryProcess = DiscoveryProcess.Create(AdapterExtensions.GetTestPlatformAssemblyPaths(_options.TestAdapterMode)))
            {
              _editorContext.WriteToLog(Resources.TestDiscoveryStarted);
              discoveryProcess.MessageReceived += (o, e) => _editorContext.WriteToLog(e.Value);
              discoveryProcess.OutputReceived += (o, e) => _editorContext.WriteToLog(e.Value);

              var discoveryResults = discoveryProcess.DiscoverTests(testDiscoveryTasks, testSettings);

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
          }
          catch (RemoteException exception) when (exception.RemoteReason != null)
          {
            _editorContext.WriteToLog(Resources.TestDiscoveryFailed);
            _telemetryManager.UploadExceptionAsync(exception.RemoteReason);
          }
          catch (Exception exception)
          {
            _editorContext.WriteToLog(Resources.TestDiscoveryFailed);
            _telemetryManager.UploadExceptionAsync(exception);
          }
        });

        ScanningFinished?.Invoke(this, EventArgs.Empty);

        return testSolution;
      }
      finally
      {
        Interlocked.Decrement(ref _sessionCount);
      }
    }

    private void LoadTests(TestProject testProject, TestCase[] testCases)
    {
      var testItems = new Dictionary<string, TestItem>()
      {
        { "", testProject }
      };

      var testCaseProcessors = testProject.TestAdapters
        .Select(p => _testAdapterRepository.Adapters[p])
        .ToDictionary(p => p.ExecutorUri, StringComparer.OrdinalIgnoreCase);
      foreach (var testCase in testCases)
      {
        try
        {
          if (!testCaseProcessors.TryGetValue(testCase.ExecutorUri.ToString().TrimEnd('/'), out var testAdapter))
          {
            throw new Exception("Cannot find adapter for executor URI: " + testCase.ExecutorUri);
          }

          var testItemKind = CodeItemKind.Method;
          var testItemPath = testCase.FullyQualifiedName;
          var displayName = null as string;
          foreach (var testCaseProcessor in testCaseProcessors.Values)
          {
            if (testCaseProcessor.CanProcessCase(testCase))
            {
              testCaseProcessor.ProcessCase(testCase, ref testItemKind, ref testItemPath, ref displayName);
              break;
            }
          }

          AddTestItem(testItems, testItemKind, testItemPath, testCase, displayName, testAdapter.Name);
        }
        catch (Exception e)
        {
          _editorContext.WriteToLog($"Could not register test case {testCase.FullyQualifiedName}. Reason: {e.GetDescription()}");
        }
      }
    }

    private static TestItem AddTestItem(Dictionary<string, TestItem> items, CodeItemKind itemKind, string itemPath, TestCase testCase = null, string displayName = null, string testAdapterName = null)
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
          item = new TestMethod(parent as TestClass, name, testCase, testAdapterName);
          break;
        case CodeItemKind.Data:
          item = new TestMethod(parent as TestMethod, name, displayName, testCase, testAdapterName);
          break;
        default:
          throw new NotImplementedException();
      }
      items.Add(itemPath, item);

      return item;
    }
  }
}
