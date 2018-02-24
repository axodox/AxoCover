namespace AxoCover.Models.Testing.Data
{
  public class TestNamespace : TestItem
  {
    public TestNamespace(TestNamespace parent, string name)
      : base(parent, name, CodeItemKind.Namespace)
    {

    }

    protected TestNamespace(TestNamespace parent, string name, CodeItemKind kind)
      : base(parent, name, kind)
    {

    }
  }
}
