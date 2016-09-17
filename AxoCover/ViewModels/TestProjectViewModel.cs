using AxoCover.Models.Data;

namespace AxoCover.ViewModels
{
  public class TestProjectViewModel : TestItemViewModel
  {
    public TestProjectViewModel(TestItemViewModel parent, TestProject testItem)
      : base(parent, testItem)
    {

    }

    private TestOutputDescription _output;
    public TestOutputDescription Output
    {
      get
      {
        return _output;
      }
      set
      {
        _output = value;
        NotifyPropertyChanged(nameof(Output));
      }
    }
  }
}
