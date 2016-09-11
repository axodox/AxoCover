using System.Windows;

namespace AxoCover.Controls
{
  /// <summary>
  /// Interaction logic for ToolBarButton.xaml
  /// </summary>
  public partial class Button : System.Windows.Controls.Button
  {
    public static DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(Button));

    public string Text
    {
      get { return (string)GetValue(TextProperty); }
      set { SetValue(TextProperty, value); }
    }

    public static DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(string), typeof(Button));

    public string Icon
    {
      get { return (string)GetValue(IconProperty); }
      set { SetValue(IconProperty, value); }
    }

    public static DependencyProperty IsToggleProperty = DependencyProperty.Register(nameof(IsToggle), typeof(bool), typeof(Button));

    public bool IsToggle
    {
      get { return (bool)GetValue(IsToggleProperty); }
      set { SetValue(IsToggleProperty, value); }
    }

    public static DependencyProperty IsCheckedProperty = DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(Button),
      new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true });

    public bool IsChecked
    {
      get { return (bool)GetValue(IsCheckedProperty); }
      set { SetValue(IsCheckedProperty, value); }
    }

    public static DependencyProperty ContentAlignmentProperty = DependencyProperty.Register(nameof(ContentAlignment), typeof(HorizontalAlignment), typeof(Button),
      new FrameworkPropertyMetadata(HorizontalAlignment.Center));

    public HorizontalAlignment ContentAlignment
    {
      get { return (HorizontalAlignment)GetValue(ContentAlignmentProperty); }
      set { SetValue(ContentAlignmentProperty, value); }
    }

    public Button()
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
