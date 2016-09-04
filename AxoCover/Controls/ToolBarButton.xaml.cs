using System.Windows;
using System.Windows.Controls;

namespace AxoCover.Controls
{
  /// <summary>
  /// Interaction logic for ToolBarButton.xaml
  /// </summary>
  public partial class ToolBarButton : Button
  {
    public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ToolBarButton));

    public string Text
    {
      get { return (string)GetValue(TextProperty); }
      set { SetValue(TextProperty, value); }
    }

    public static DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(string), typeof(ToolBarButton));

    public string Icon
    {
      get { return (string)GetValue(IconProperty); }
      set { SetValue(IconProperty, value); }
    }

    public ToolBarButton()
    {
      InitializeComponent();
    }
  }
}
