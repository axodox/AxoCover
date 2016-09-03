namespace AxoCover.Models.Data
{
  public class TestClass : TestItem
  {
    public TestClass(TestNamespace parent, string name)
      : base(parent, name, TestItemKind.Class)
    {

    }
  }
}
