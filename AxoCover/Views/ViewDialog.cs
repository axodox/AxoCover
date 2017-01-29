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
      MaxHeight = 640;
      SizeToContent = SizeToContent.Height;
      ShowInTaskbar = false;
      Owner = Application.Current.MainWindow;
      Background = FindResource(EnvironmentColors.CommandBarGradientBrushKey) as Brush;
      View = new TView();
      base.AddChild(View);

      var dialog = View as IDialog;
      if (dialog != null)
      {
        Title = dialog.Title;
        dialog.ClosingDialog += (o, e) =>
        {
          if (IsVisible) DialogResult = e.Result;
        };
        Closing += (o, e) => dialog.OnClosing();
      }
    }
  }
}
