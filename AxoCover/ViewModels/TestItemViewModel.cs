using AxoCover.Models.Data;
using AxoCover.Models.Extensions;
using System.Linq;

namespace AxoCover.ViewModels
{
  public class TestItemViewModel : CodeItemViewModel<TestItemViewModel, TestItem>
  {
    private TestState _state;
    public TestState State
    {
      get
      {
        return _state;
      }
      set
      {
        _state = value;
        IsStateUpToDate = true;
        NotifyPropertyChanged(nameof(State));
        NotifyPropertyChanged(nameof(IconPath));
        NotifyPropertyChanged(nameof(OverlayIconPath));

        foreach (var parent in this.Crawl(p => p.Parent))
        {
          parent.RefreshStateCounts();

          if (!parent.IsStateUpToDate && parent.State < _state)
          {
            parent.State = _state;
          }

          if (parent.State == TestState.Scheduled && parent.Children.All(p => p.State != TestState.Scheduled))
          {
            parent.State = parent.Children.Where(p => p.IsStateUpToDate).Max(p => p.State);
          }
        }
      }
    }

    private bool _isStateUpToDate;
    public bool IsStateUpToDate
    {
      get
      {
        return _isStateUpToDate;
      }
      set
      {
        _isStateUpToDate = value;
        NotifyPropertyChanged(nameof(IsStateUpToDate));
      }
    }

    public TestSolutionViewModel Owner { get; private set; }

    public bool IsCoverOnBuild
    {
      get
      {
        return Owner.AutoCoverTarget == this;
      }
      set
      {
        if (value)
        {
          Owner.AutoCoverTarget = this;
        }
        else if (IsCoverOnBuild)
        {
          Owner.AutoCoverTarget = null;
        }
        NotifyPropertyChanged(nameof(IsCoverOnBuild));
      }
    }

    public string IconPath
    {
      get
      {
        if (CodeItem.Kind == CodeItemKind.Method)
        {
          if (State != TestState.Unknown)
          {
            return AxoCoverPackage.ResourcesPath + State + ".png";
          }
          else
          {
            return AxoCoverPackage.ResourcesPath + "test.png";
          }
        }
        else
        {
          return AxoCoverPackage.ResourcesPath + CodeItem.Kind + ".png";
        }
      }
    }

    public string OverlayIconPath
    {
      get
      {
        if (CodeItem.Kind != CodeItemKind.Method)
        {
          if (State != TestState.Unknown)
          {
            return AxoCoverPackage.ResourcesPath + State + ".png";
          }
          else
          {
            return AxoCoverPackage.ResourcesPath + "test.png";
          }
        }
        else
        {
          return null;
        }
      }
    }

    private TestResult _result;
    public TestResult Result
    {
      get
      {
        return _result;
      }
      set
      {
        _result = value;
        if (Result != null)
        {
          State = value.Outcome;
        }
        NotifyPropertyChanged(nameof(Result));
      }
    }

    public int NamespaceCount
    {
      get
      {
        return this.Flatten(p => p.Children).Count(p => p.CodeItem.Kind == CodeItemKind.Namespace);
      }
    }

    public int ClassCount
    {
      get
      {
        return this.Flatten(p => p.Children).Count(p => p.CodeItem.Kind == CodeItemKind.Class);
      }
    }

    public int TestCount
    {
      get
      {
        return this.Flatten(p => p.Children).Count(p => p.CodeItem.Kind == CodeItemKind.Method);
      }
    }

    public int PassedCount
    {
      get
      {
        return CodeItem.Kind == CodeItemKind.Method && State == TestState.Passed ? 1 : Children.Sum(p => p.PassedCount);
      }
    }

    public int WarningCount
    {
      get
      {
        return CodeItem.Kind == CodeItemKind.Method && State == TestState.Skipped ? 1 : Children.Sum(p => p.WarningCount);
      }
    }

    public int FailedCount
    {
      get
      {
        return CodeItem.Kind == CodeItemKind.Method && State == TestState.Failed ? 1 : Children.Sum(p => p.FailedCount);
      }
    }

    public bool CanDebugged
    {
      get
      {
        return CodeItem.Kind == CodeItemKind.Method;
      }
    }

    public TestItemViewModel(TestItemViewModel parent, TestItem testItem)
      : base(parent, testItem, CreateViewModel)
    {
      Owner = (this.Crawl(p => p.Parent).LastOrDefault() ?? this) as TestSolutionViewModel;
    }

    private static TestItemViewModel CreateViewModel(TestItemViewModel parent, TestItem testItem)
    {
      switch (testItem.Kind)
      {
        case CodeItemKind.Solution:
          return new TestSolutionViewModel(testItem as TestSolution);
        case CodeItemKind.Project:
          return new TestProjectViewModel(parent, testItem as TestProject);
        default:
          return new TestItemViewModel(parent, testItem);
      }
    }

    public void ResetAll()
    {
      IsStateUpToDate = false;

      foreach (var child in Children)
      {
        child.ResetAll();
      }
    }

    public void ScheduleAll()
    {
      State = TestState.Scheduled;

      foreach (var child in Children)
      {
        child.ScheduleAll();
      }
    }

    private void RefreshStateCounts()
    {
      NotifyPropertyChanged(nameof(PassedCount));
      NotifyPropertyChanged(nameof(WarningCount));
      NotifyPropertyChanged(nameof(FailedCount));
    }

    protected override void OnUpdated()
    {
      base.OnUpdated();
      NotifyPropertyChanged(nameof(NamespaceCount));
      NotifyPropertyChanged(nameof(TestCount));
      NotifyPropertyChanged(nameof(ClassCount));
    }

    protected override void OnRemoved()
    {
      if (IsCoverOnBuild)
      {
        Owner.AutoCoverTarget = null;
      }
      base.OnRemoved();
    }
  }
}
