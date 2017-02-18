using AxoCover.Common.Extensions;
using System;
using System.Collections.Generic;

namespace AxoCover.Models.Data
{
  public abstract class CodeItem<T>
    where T : CodeItem<T>
  {
    public string DisplayName { get; protected set; }

    public string Name { get; private set; }

    public CodeItemKind Kind { get; private set; }

    public T Parent { get; private set; }

    private List<T> _children = new List<T>();
    public IEnumerable<T> Children { get { return _children; } }

    public string FullName
    {
      get
      {
        return Parent == null || Parent.Kind == CodeItemKind.Project ? Name : Parent.FullName + "." + Name;
      }
    }

    public CodeItem(T parent, string name, CodeItemKind kind)
    {
      if (parent == null && kind != CodeItemKind.Solution)
        throw new ArgumentNullException(nameof(parent));

      Name = name;
      DisplayName = name;
      Kind = kind;
      Parent = parent;
      if (parent != null)
      {
        parent._children.OrderedAdd(this as T, (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name));
      }
    }

    public void Remove()
    {
      Parent._children.Remove(this as T);
      Parent = null;
    }

    public U GetParent<U>()
      where U : T
    {
      var codeItem = this;
      while (codeItem != null && !(codeItem is U))
      {
        codeItem = codeItem.Parent;
      }
      return codeItem as U;
    }

    public T GetParent(CodeItemKind kind)
    {
      var codeItem = this as T;
      while (codeItem != null && codeItem.Kind != kind)
      {
        codeItem = codeItem.Parent;
      }
      return codeItem;
    }

    public static bool operator ==(CodeItem<T> a, CodeItem<T> b)
    {
      if ((object)a == null || (object)b == null)
      {
        return ReferenceEquals(a, b);
      }
      else
      {
        return a.Name == b.Name && a.Kind == b.Kind;
      }
    }

    public static bool operator !=(CodeItem<T> a, CodeItem<T> b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CodeItem<T> && this == obj as CodeItem<T>;
    }

    public override int GetHashCode()
    {
      return Name.GetHashCode() ^ Kind.GetHashCode();
    }
  }
}
