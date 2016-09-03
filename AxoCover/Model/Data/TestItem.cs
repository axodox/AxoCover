using System.Collections.Generic;

namespace AxoCover.Model.Data
{
  abstract class TestItem
  {
    public string Name { get; private set; }

    public TestItem Parent { get; private set; }

    public IEnumerable<TestItem> Items { get { return _items; } }

    private List<TestItem> _items;

    public TestItem(TestItem parent, string name)
    {
      Name = name;
      _items = new List<TestItem>();
      Parent = parent;
      if (parent != null)
      {
        parent._items.Add(this);
      }
    }
  }
}
