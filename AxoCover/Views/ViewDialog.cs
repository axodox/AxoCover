using System.Windows;

namespace AxoCover.Views
{
  public class ViewDialog<TView> : Window
    where TView : UIElement, new()
  {
    public TView View { get; private set; }

    public ViewDialog()
    {
      Width = 640;
      Height = 480;
      SizeToContent = SizeToContent.Height;
      ShowInTaskbar = false;
      Owner = Application.Current.MainWindow;
      View = new TView();
      AddChild(View);
    }
  }
}
