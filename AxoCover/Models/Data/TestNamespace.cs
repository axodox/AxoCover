namespace AxoCover.Models.Data
{
  public class TestNamespace : TestItem
  {
    public TestNamespace(TestNamespace parent, string name)
      : base(parent, name, TestItemKind.Namespace)
    {

    }

    protected TestNamespace(TestNamespace parent, string name, TestItemKind kind)
      : base(parent, name, kind)
    {

    }
  }
}
