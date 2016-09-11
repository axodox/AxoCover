using System.Windows;
using System.Windows.Controls;

namespace AxoCover.Controls
{
  /// <summary>
  /// Interaction logic for ToolBarButton.xaml
  /// </summary>
  public partial class ToolBarButton : Button
  {
    public static DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(ToolBarButton));

    public string Text
    {
      get { return (string)GetValue(TextProperty); }
      set { SetValue(TextProperty, value); }
    }

    public static DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(string), typeof(ToolBarButton));

    public string Icon
    {
      get { return (string)GetValue(IconProperty); }
      set { SetValue(IconProperty, value); }
    }

    public static DependencyProperty IsToggleProperty = DependencyProperty.Register(nameof(IsToggle), typeof(bool), typeof(ToolBarButton));

    public bool IsToggle
    {
      get { return (bool)GetValue(IsToggleProperty); }
      set { SetValue(IsToggleProperty, value); }
    }

    public static DependencyProperty IsCheckedProperty = DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(ToolBarButton),
      new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true });

    public bool IsChecked
    {
      get { return (bool)GetValue(IsCheckedProperty); }
      set { SetValue(IsCheckedProperty, value); }
    }

    public static DependencyProperty ContentAlignmentProperty = DependencyProperty.Register(nameof(ContentAlignment), typeof(HorizontalAlignment), typeof(ToolBarButton),
      new FrameworkPropertyMetadata(HorizontalAlignment.Center));

    public HorizontalAlignment ContentAlignment
    {
      get { return (HorizontalAlignment)GetValue(ContentAlignmentProperty); }
      set { SetValue(ContentAlignmentProperty, value); }
    }

    public ToolBarButton()
    {
      InitializeComponent();
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
      if (IsToggle)
      {
        IsChecked = !IsChecked;
      }
    }
  }
}
