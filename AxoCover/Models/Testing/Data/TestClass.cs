namespace AxoCover.Models.Testing.Data
{
  public class TestClass : TestItem
  {
    public TestClass(TestNamespace parent, string name)
      : base(parent, name, CodeItemKind.Class)
    {

    }

    public TestClass(TestClass parent, string name)
      : base(parent, name, CodeItemKind.Class)
    {

    }
  }
}
