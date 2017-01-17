namespace AxoCover.ViewModels
{
  public class TextViewModel : ViewModel
  {
    private string _text;
    public string Text
    {
      get { return _text; }
      set
      {
        _text = value;
        NotifyPropertyChanged(nameof(Text));
      }
    }
  }
}
