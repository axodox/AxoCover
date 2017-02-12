using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace AxoCover.Models.Data
{
  public class TestMethod : TestItem
  {
    public int Index { get; set; }

    public bool IsIgnored { get; set; }

    public TestCase Case { get; set; }

    public string Path
    {
      get
      {
        return this.GetParent<TestProject>().Name + "." + FullName;
      }
    }

    public TestMethod(TestClass parent, string name)
      : base(parent, name, CodeItemKind.Method)
    {

    }
  }
}
