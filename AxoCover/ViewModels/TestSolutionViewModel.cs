using AxoCover.Models.Data;

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

    private TestItemViewModel _autoCoverTarget;
    public TestItemViewModel AutoCoverTarget
    {
      get { return _autoCoverTarget; }
      set
      {
        var oldAutoCoverTarget = _autoCoverTarget;
        _autoCoverTarget = value;

        if (oldAutoCoverTarget != null)
        {
          oldAutoCoverTarget.IsCoverOnBuild = false;
        }
        NotifyPropertyChanged(nameof(AutoCoverTarget));
      }
    }

  }
}
