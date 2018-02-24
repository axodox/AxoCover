namespace AxoCover.Models.Testing.Data
{
  public abstract class TestItem : CodeItem<TestItem>
  {
    public TestItem(TestItem parent, string name, CodeItemKind kind)
      : base(parent, name, kind)
    {

    }
  }
}
