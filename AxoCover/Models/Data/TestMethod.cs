namespace AxoCover.Models.Data
{
  public class TestMethod : TestItem
  {
    public int Index { get; set; }

    public TestMethod(TestClass parent, string name)
      : base(parent, name, CodeItemKind.Method)
    {

    }
  }
}
