using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        var testClasses = FilterByAttribute(assembly.ExportedTypes, nameof(TestClassAttribute));

        foreach (var testClass in testClasses)
        {
          var testMethods = FilterByAttribute(testClass.GetMethods(), nameof(TestMethodAttribute));
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

      return testItems.ToArray();
    }

    private static IEnumerable<T> FilterByAttribute<T>(IEnumerable<T> members, string includedAttributeName)
      where T : MemberInfo
    {
      foreach (var member in members)
      {
        var attributes = member.GetCustomAttributesData();
        if (attributes.Any(p => p.AttributeType.Name == includedAttributeName))
        {
          yield return member;
        }
      }
    }
  }
}
