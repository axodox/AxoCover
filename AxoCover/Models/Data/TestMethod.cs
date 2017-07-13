using AxoCover.Common.Extensions;
using AxoCover.Common.Models;
using System.Linq;

namespace AxoCover.Models.Data
{
  public class TestMethod : TestItem
  {
    public TestCase Case { get; private set; }

    public string TestAdapterName { get; private set; }

    public string ShortName
    {
      get
      {
        return string.Join(".", this
          .Crawl<TestItem>(p => p.Parent, true)
          .TakeWhile(p => p.Kind == CodeItemKind.Data || p.Kind == CodeItemKind.Method || p.Kind == CodeItemKind.Class)
          .Reverse()
          .Select(p => p.DisplayName));
      }
    }

    public TestMethod(TestClass parent, string name, TestCase testCase, string testAdapterName)
      : base(parent, name, CodeItemKind.Method)
    {
      Case = testCase;
      TestAdapterName = testAdapterName;
    }

    public TestMethod(TestMethod parent, string name, string displayName, TestCase testCase, string testAdapterName)
      : base(parent, name, CodeItemKind.Data)
    {
      DisplayName = parent.DisplayName + displayName;
      Case = testCase;
      TestAdapterName = testAdapterName;
    }
  }
}
