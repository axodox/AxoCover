using System.Linq;

namespace AxoCover.Models.Data
{
  public abstract class TestItem : CodeItem<TestItem>
  {
    public int TestCount
    {
      get
      {
        return Kind == CodeItemKind.Method ? 1 : Children.Sum(p => p.TestCount);
      }
    }

    public TestItem(TestItem parent, string name, CodeItemKind kind)
      : base(parent, name, kind)
    {

    }
  }
}
