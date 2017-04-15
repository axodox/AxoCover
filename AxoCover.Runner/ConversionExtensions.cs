using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AxoCover.Runner
{
  public static class ConversionExtensions
  {
    private static readonly HashSet<string> _ignoredTestCaseProperties =
      new HashSet<string>(typeof(TestCase).GetProperties().Select(p => "TestCase." + p.Name).Concat(new[] { "TestObject.Traits" }));

    private static readonly ConcurrentDictionary<string, Type> _typeCache = new ConcurrentDictionary<string, Type>();

    public static Common.Models.TestMessageLevel Convert(this TestMessageLevel testMessageLevel)
    {
      return (Common.Models.TestMessageLevel)testMessageLevel;
    }

    public static Common.Models.TestCase[] Convert(this IEnumerable<TestCase> testCases)
    {
      return testCases.Select(Convert).ToArray();
    }

    public static Common.Models.TestCase Convert(this TestCase testCase)
    {
      return new Common.Models.TestCase()
      {
        CodeFilePath = testCase.CodeFilePath,
        DisplayName = testCase.DisplayName,
        ExecutorUri = testCase.ExecutorUri,
        FullyQualifiedName = testCase.FullyQualifiedName,
        Id = testCase.Id,
        LineNumber = testCase.LineNumber,
        Source = testCase.Source,
        AdditionalProperties = testCase.Properties
          .Where(p => !_ignoredTestCaseProperties.Contains(p.Id))
          .Select(p => p.Convert(testCase.GetPropertyValue(p)))
          .ToList()
      };
    }

    private static Common.Models.TestProperty Convert(this TestProperty testProperty, object value)
    {
      return new Common.Models.TestProperty()
      {
        Attributes = testProperty.Attributes.Convert(),
        Category = testProperty.Category,
        Description = testProperty.Description,
        Id = testProperty.Id,
        Label = testProperty.Label,
        ValueType = testProperty.ValueType,
        Value = JsonConvert.SerializeObject(value)
      };
    }

    private static TestProperty Convert(this Common.Models.TestProperty testProperty)
    {
      var result = TestProperty.Find(testProperty.Id);
      if (result == null)
      {
        var type = GetType(testProperty.ValueType);
        if (type == null) return null;

        result = TestProperty.Register(testProperty.Id, testProperty.Label, type, testProperty.Attributes.Convert(), typeof(ConversionExtensions));
        result.Description = testProperty.Description;
        result.Category = testProperty.Category;
      }
      return result;
    }

    private static Type GetType(string typeName)
    {
      Type result;
      if (_typeCache.TryGetValue(typeName, out result))
      {
        return result;
      }
      else
      {
        _typeCache[typeName] = result = Type.GetType(typeName) ?? Type.GetType(typeName + ", System") ?? Type.GetType(typeName + ", mscorlib");
        return result;
      }
    }

    public static Common.Models.TestPropertyAttributes Convert(this TestPropertyAttributes testPropertyAttributes)
    {
      return (Common.Models.TestPropertyAttributes)testPropertyAttributes;
    }

    public static TestPropertyAttributes Convert(this Common.Models.TestPropertyAttributes testPropertyAttributes)
    {
      return (TestPropertyAttributes)testPropertyAttributes;
    }

    public static TestCase[] Convert(this IEnumerable<Common.Models.TestCase> testCases)
    {
      return testCases.Select(Convert).ToArray();
    }

    public static TestCase Convert(this Common.Models.TestCase testCase)
    {
      var result = new TestCase(testCase.FullyQualifiedName, testCase.ExecutorUri, testCase.Source)
      {
        CodeFilePath = testCase.CodeFilePath,
        DisplayName = testCase.DisplayName,
        ExecutorUri = testCase.ExecutorUri,
        FullyQualifiedName = testCase.FullyQualifiedName,
        Id = testCase.Id,
        LineNumber = testCase.LineNumber
      };

      foreach (var property in testCase.AdditionalProperties)
      {
        var propertyDescription = property.Convert();
        if (propertyDescription == null) continue;
        result.SetPropertyValue(propertyDescription, JsonConvert.DeserializeObject(property.Value, GetType(property.ValueType)));
      }

      return result;
    }

    public static Common.Models.TestOutcome Convert(this TestOutcome testOutcome)
    {
      return (Common.Models.TestOutcome)testOutcome;
    }

    public static Common.Models.TestResult Convert(this TestResult testResult)
    {
      return new Common.Models.TestResult()
      {
        ComputerName = testResult.ComputerName,
        DisplayName = testResult.DisplayName,
        Duration = testResult.Duration,
        EndTime = testResult.EndTime,
        ErrorMessage = testResult.ErrorMessage,
        ErrorStackTrace = testResult.ErrorStackTrace,
        Outcome = testResult.Outcome.Convert(),
        StartTime = testResult.StartTime,
        TestCase = testResult.TestCase.Convert()
      };
    }
  }
}
