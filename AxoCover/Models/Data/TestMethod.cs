namespace AxoCover.Models.Data
{
  public class TestMethod : TestItem
  {
    public TestMethod(TestClass parent, string name)
      : base(parent, name, TestItemKind.Method)
    {

    }
  }
}
