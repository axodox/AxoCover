using AxoCover.Models.Data;
using System.IO;

namespace AxoCover.ViewModels
{
  public class OutputDirectoryViewModel : ViewModel
  {
    public string Name { get; private set; }

    public string Location { get; private set; }

    private OutputDescription _output;
    public OutputDescription Output
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

    public OutputDirectoryViewModel(string location)
    {
      Name = Path.GetFileName(location);
      Location = location;
    }
  }
}
