using AxoCover.Common.Events;
using AxoCover.Models.Data;
using System;

namespace AxoCover.ViewModels
{
  public class TestSolutionViewModel : TestItemViewModel
  {
    public bool IsEmpty
    {
      get
      {
        return Children.Count == 0;
      }
    }

    public TestSolutionViewModel(TestSolution testItem)
      : base(null, testItem)
    {

    }

    protected override void OnUpdated()
    {
      base.OnUpdated();
      NotifyPropertyChanged(nameof(IsEmpty));
    }

    public event EventHandler<EventArgs<TestItemViewModel>> AutoCoverTargetUpdated;

    private TestItemViewModel _autoCoverTarget;
    public TestItemViewModel AutoCoverTarget
    {
      get { return _autoCoverTarget; }
      set
      {
        var oldCoverTarget = _autoCoverTarget;
        _autoCoverTarget = value;
        if(oldCoverTarget != null) AutoCoverTargetUpdated?.Invoke(this, new EventArgs<TestItemViewModel>(oldCoverTarget));
        if(_autoCoverTarget != null) AutoCoverTargetUpdated?.Invoke(this, new EventArgs<TestItemViewModel>(_autoCoverTarget));

        NotifyPropertyChanged(nameof(AutoCoverTarget));
      }
    }

  }
}
