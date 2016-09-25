using AxoCover.Models.Data;
using AxoCover.Models.Extensions;
using System;
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

    public TestItemViewModel(TestItemViewModel parent, TestItem testItem)
      : base(parent, testItem)
    {

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

    protected override void AddChild(TestItem testItem)
    {
      TestItemViewModel child;
      switch (testItem.Kind)
      {
        case CodeItemKind.Project:
          child = new TestProjectViewModel(this, testItem as TestProject);
          break;
        default:
          child = new TestItemViewModel(this, testItem);
          break;
      }

      Children.OrderedAdd(child, (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.CodeItem.Name, b.CodeItem.Name));
    }
  }
}
