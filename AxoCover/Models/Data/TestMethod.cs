using AxoCover.Common.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Linq;

namespace AxoCover.Models.Data
{
  public class TestMethod : TestItem
  {
    public TestCase Case { get; private set; }

    public string Path
    {
      get
      {
        return this.GetParent<TestProject>().Name + "." + FullName;
      }
    }

    public string ShortName
    {
      get
      {
        return string.Join(".", this
          .Crawl<TestItem>(p => p.Parent, true)
          .TakeWhile(p => p.Kind == CodeItemKind.Data || p.Kind == CodeItemKind.Method || p.Kind == CodeItemKind.Class)
          .Reverse()
          .Select(p => p.Name));
      }
    }

    public TestMethod(TestClass parent, string name, TestCase testCase)
      : base(parent, name, CodeItemKind.Method)
    {
      Case = testCase;
    }

    public TestMethod(TestMethod parent, string name, string displayName, TestCase testCase)
      : base(parent, name, CodeItemKind.Data)
    {
      DisplayName = parent.DisplayName + displayName;
      Case = testCase;
    }
  }
}
