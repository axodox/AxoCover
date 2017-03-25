using Microsoft.VisualStudio.PlatformUI;
using System.Windows;
using System.Windows.Media;

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
      ShowInTaskbar = false;
      Owner = Application.Current.MainWindow;
      Background = FindResource(EnvironmentColors.CommandBarGradientBrushKey) as Brush;
      WindowStartupLocation = WindowStartupLocation.CenterOwner;
      View = new TView();
      base.AddChild(View);

      var dialog = View as IDialog;
      if (dialog != null)
      {
        dialog.InitializeWindow(this);
      }
    }
  }
}
