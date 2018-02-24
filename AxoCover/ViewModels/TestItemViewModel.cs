using AxoCover.Common.Events;
using AxoCover.Common.Extensions;
using AxoCover.Models.Extensions;
using AxoCover.Models.Testing.Data;
using System.Linq;
using System.Windows;

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

          if (!parent.IsStateUpToDate && parent.State <= _state)
          {
            parent.State = _state;
          }

          if (parent.State == TestState.Scheduled && 
            parent.Children.All(p => p.State != TestState.Scheduled || !p.IsStateUpToDate))
          {
            var childStates = parent.Children
              .Where(p => p.IsStateUpToDate)
              .Select(p => p.State)
              .ToArray();

            if(childStates.Length > 0)
            {
              parent.State = childStates.Max();
            }
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
        //Check the code item, so copies work too
        return Owner.AutoCoverTarget?.CodeItem == CodeItem;
      }
      set
      {
        if (value)
        {
          //Set the original instance even when the current one is a copy
          Owner.AutoCoverTarget = Owner.FindChild(CodeItem);
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

    private TestResultCollectionViewModel _result = new TestResultCollectionViewModel();
    public TestResultCollectionViewModel Result => _result;

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
        return this.Flatten(p => p.Children).Count(p => p.CodeItem.IsTest());
      }
    }

    public int PassedCount
    {
      get
      {
        return CodeItem.IsTest() && State == TestState.Passed ? 1 : Children.Sum(p => p.PassedCount);
      }
    }

    public int WarningCount
    {
      get
      {
        return CodeItem.IsTest() && State == TestState.Skipped ? 1 : Children.Sum(p => p.WarningCount);
      }
    }

    public int FailedCount
    {
      get
      {
        return CodeItem.IsTest() && State == TestState.Failed ? 1 : Children.Sum(p => p.FailedCount);
      }
    }

    public string DisplayName { get; }

    public TestItemViewModel(TestItemViewModel parent, TestItem testItem)
      : base(parent, testItem, CreateViewModel)
    {
      Owner = (this.Crawl(p => p.Parent).LastOrDefault() ?? this) as TestSolutionViewModel;
      WeakEventManager<TestSolutionViewModel, EventArgs<TestItemViewModel>>.AddHandler(Owner, nameof(TestSolutionViewModel.AutoCoverTargetUpdated), OnAutoCoverTargetUpdated);
      DisplayName = !CodeItem.Children.Any() && CodeItem is TestMethod ? CodeItem.Parent.DisplayName + "." + CodeItem.DisplayName : CodeItem.DisplayName;
    }

    private void OnAutoCoverTargetUpdated(object sender, EventArgs<TestItemViewModel> e)
    {
      if (e.Value.CodeItem == CodeItem)
      {
        NotifyPropertyChanged(nameof(IsCoverOnBuild));
      }
    }

    private static TestItemViewModel CreateViewModel(TestItemViewModel parent, TestItem testItem)
    {
      switch (testItem.Kind)
      {
        case CodeItemKind.Solution:
          return new TestSolutionViewModel(testItem as TestSolution);
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
      try
      {
        _isRefreshStateCountsEnabled = false;
        State = TestState.Scheduled;

        foreach (var child in Children)
        {
          child.ScheduleAll();
        }
      }
      finally
      {
        _isRefreshStateCountsEnabled = true;
        RefreshStateCounts();
      }
    }

    private bool _isRefreshStateCountsEnabled = true;
    private void RefreshStateCounts()
    {
      if (_isRefreshStateCountsEnabled)
      {
        NotifyPropertyChanged(nameof(PassedCount));
        NotifyPropertyChanged(nameof(WarningCount));
        NotifyPropertyChanged(nameof(FailedCount));
      }
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

    public TestItemViewModel CreateResultViewModel(TestResult testResult)
    {
      var newViewModel = (TestItemViewModel)MemberwiseClone();
      WeakEventManager<TestSolutionViewModel, EventArgs<TestItemViewModel>>.AddHandler(Owner, nameof(TestSolutionViewModel.AutoCoverTargetUpdated), newViewModel.OnAutoCoverTargetUpdated);
      newViewModel._result = new TestResultCollectionViewModel();
      newViewModel.Result.Results.Add(testResult);
      newViewModel.State = testResult.Outcome;
      return newViewModel;
    }
  }
}
