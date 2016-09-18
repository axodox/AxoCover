using AxoCover.Models.Data.CoverageReport;
using AxoCover.Models.Extensions;
using System;
using System.Collections.Generic;

namespace AxoCover.Models.Data
{
  public class TestItemResult
  {
    public string Name { get; private set; }

    public TestItemKind Kind { get; private set; }

    public TestItemResult Parent { get; private set; }

    private List<TestItemResult> _children = new List<TestItemResult>();
    public IEnumerable<TestItemResult> Children { get { return _children; } }

    public int SequencePoints { get; set; }
    public int VisitedSequencePoints { get; set; }
    public int BranchPoints { get; set; }
    public int VisitedBranchPoints { get; set; }

    public TestItemResult(TestItemResult parent,
      TestItemKind kind, string name,
      Summary summary)
      : this(parent, kind, name,
          summary.SequencePoints, summary.VisitedSequencePoints,
          summary.BranchPoints, summary.VisitedBranchPoints)
    {

    }

    public TestItemResult(TestItemResult parent,
      TestItemKind kind, string name)
      : this(parent, kind, name, 0, 0, 0, 0)
    {

    }

    public TestItemResult(TestItemResult parent,
      TestItemKind kind, string name,
      int sequencePoints, int visitedSequencePoints,
      int branchPoints, int visitedBranchPoints)
    {
      if (parent == null && kind != TestItemKind.Solution)
        throw new ArgumentNullException(nameof(parent));

      Name = name;
      Kind = kind;
      Parent = parent;
      if (parent != null)
      {
        parent._children.OrderedAdd(this, (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name));
      }

      SequencePoints = sequencePoints;
      VisitedSequencePoints = visitedSequencePoints;
      BranchPoints = branchPoints;
      VisitedSequencePoints = visitedBranchPoints;
    }

    public static bool operator ==(TestItemResult a, TestItemResult b)
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

    public static bool operator !=(TestItemResult a, TestItemResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is TestItemResult && this == obj as TestItemResult;
    }

    public override int GetHashCode()
    {
      return Name.GetHashCode() ^ Kind.GetHashCode();
    }
  }
}
