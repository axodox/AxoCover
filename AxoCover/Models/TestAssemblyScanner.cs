using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AxoCover.Models
{
  public class TestAssemblyScanner : MarshalByRefObject, ITestAssemblyScanner
  {
    public string[] ScanAssemblyForTests(string assemblyPath)
    {
      var testItems = new List<string>();

      try
      {
        var assembly = Assembly.LoadFrom(assemblyPath);
        var testClasses = FilterByAttribute(assembly.ExportedTypes, "TestClassAttribute");

        foreach (var testClass in testClasses)
        {
          var testMethods = FilterByAttribute(testClass.GetMethods(), "TestMethodAttribute");
          var testClassName = testClass.FullName;

          foreach (var testMethod in testMethods)
          {
            testItems.Add(testClassName + "." + testMethod.Name);
          }
        }
      }
      catch
      {

      }

      testItems.Sort();
      return testItems.ToArray();
    }

    private static IEnumerable<T> FilterByAttribute<T>(IEnumerable<T> members, string attributeName)
      where T : MemberInfo
    {
      foreach (var member in members)
      {
        if (member.GetCustomAttributesData().Any(p => p.AttributeType.Name == attributeName))
        {
          yield return member;
        }
      }
    }
  }
}
