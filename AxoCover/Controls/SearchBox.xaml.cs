using System.Windows;
using System.Windows.Controls;

namespace AxoCover.Controls
{
  /// <summary>
  /// Interaction logic for SearchBox.xaml
  /// </summary>
  public partial class SearchBox : TextBox
  {
    public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(SearchBox),
      new FrameworkPropertyMetadata());

    public string PlaceholderText
    {
      get { return (string)GetValue(PlaceholderTextProperty); }
      set { SetValue(PlaceholderTextProperty, value); }
    }

    public static readonly DependencyPropertyKey IsFilteringPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsFiltering), typeof(bool), typeof(SearchBox),
      new PropertyMetadata(false));

    public static readonly DependencyProperty IsFilteringProperty = IsFilteringPropertyKey.DependencyProperty;

    public bool IsFiltering
    {
      get { return (bool)GetValue(IsFilteringProperty); }
      private set { SetValue(IsFilteringPropertyKey, value); }
    }

    public SearchBox()
    {
      SharedDictionaryManager.InitializeDictionaries(Resources.MergedDictionaries);
      InitializeComponent();
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
      base.OnTextChanged(e);
      IsFiltering = !string.IsNullOrEmpty(Text);
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
      Text = string.Empty;
    }
  }
}
