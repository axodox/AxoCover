using Microsoft.VisualStudio.TestPlatform.ObjectModel;

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

    public TestMethod(TestClass parent, string name, TestCase testCase)
      : base(parent, name, CodeItemKind.Method)
    {
      Case = testCase;
    }

    public TestMethod(TestMethod parent, string name, string displayName, TestCase testCase)
      : base(parent, name, CodeItemKind.Data)
    {
      DisplayName = displayName;
      Case = testCase;
    }
  }
}
