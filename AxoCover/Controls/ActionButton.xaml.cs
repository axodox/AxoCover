using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AxoCover.Controls
{
  /// <summary>
  /// Interaction logic for ToolBarButton.xaml
  /// </summary>
  public partial class ActionButton : Button
  {
    public static DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(ActionButton));

    public string Text
    {
      get { return (string)GetValue(TextProperty); }
      set { SetValue(TextProperty, value); }
    }

    public static DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(ImageSource), typeof(ActionButton));

    public ImageSource Icon
    {
      get { return (ImageSource)GetValue(IconProperty); }
      set { SetValue(IconProperty, value); }
    }

    public static DependencyProperty IsToggleProperty = DependencyProperty.Register(nameof(IsToggle), typeof(bool), typeof(ActionButton));

    public bool IsToggle
    {
      get { return (bool)GetValue(IsToggleProperty); }
      set { SetValue(IsToggleProperty, value); }
    }

    public static DependencyProperty IsCheckedProperty = DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(ActionButton),
      new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true });

    public bool IsChecked
    {
      get { return (bool)GetValue(IsCheckedProperty); }
      set { SetValue(IsCheckedProperty, value); }
    }

    public static DependencyProperty ContentAlignmentProperty = DependencyProperty.Register(nameof(ContentAlignment), typeof(HorizontalAlignment), typeof(ActionButton),
      new FrameworkPropertyMetadata(HorizontalAlignment.Center));

    public HorizontalAlignment ContentAlignment
    {
      get { return (HorizontalAlignment)GetValue(ContentAlignmentProperty); }
      set { SetValue(ContentAlignmentProperty, value); }
    }

    public ActionButton()
    {
      Resources.MergedDictionaries.Add(SharedDictionaryManager.SharedDictionary);
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
