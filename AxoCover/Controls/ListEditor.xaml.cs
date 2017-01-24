using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AxoCover.Controls
{
  /// <summary>
  /// Interaction logic for ListEditor.xaml
  /// </summary>
  public partial class ListEditor : UserControl, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public static DependencyProperty IsEditingProperty = DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(ListEditor), new FrameworkPropertyMetadata(false) { BindsTwoWayByDefault = true });

    public bool IsEditing
    {
      get { return (bool)GetValue(IsEditingProperty); }
      set { SetValue(IsEditingProperty, value); }
    }

    public static DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(string), typeof(ListEditor), new FrameworkPropertyMetadata(string.Empty, OnValidationRelatedPropertyChanged) { BindsTwoWayByDefault = true });

    public string Value
    {
      get { return (string)GetValue(ValueProperty); }
      set { SetValue(ValueProperty, value); }
    }

    public static DependencyProperty SplitPatternProperty = DependencyProperty.Register(nameof(SplitPattern), typeof(string), typeof(ListEditor), new PropertyMetadata("[^;]+", OnValidationRelatedPropertyChanged));

    public string SplitPattern
    {
      get { return (string)GetValue(SplitPatternProperty); }
      set { SetValue(SplitPatternProperty, value); }
    }

    public static DependencyProperty ValidationPatternProperty = DependencyProperty.Register(nameof(ValidationPattern), typeof(string), typeof(ListEditor), new PropertyMetadata("^.*$", OnValidationRelatedPropertyChanged));

    public string ValidationPattern
    {
      get { return (string)GetValue(ValidationPatternProperty); }
      set { SetValue(ValidationPatternProperty, value); }
    }

    public ObservableCollection<Item> Items { get; private set; }

    public bool IsValid { get; private set; }

    public class Item
    {
      public string Value { get; set; }
      public bool IsValid { get; set; }
    }

    public ListEditor()
    {
      IsValid = true;
      Items = new ObservableCollection<Item>();
      InitializeComponent();
    }

    private static void OnValidationRelatedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as ListEditor).Validate();
    }

    private void Validate()
    {
      Items.Clear();
      foreach (Match match in Regex.Matches(Value ?? string.Empty, SplitPattern))
      {
        var item = new Item()
        {
          Value = match.Value,
          IsValid = Regex.IsMatch(match.Value, ValidationPattern)
        };
        Items.Add(item);
      }

      IsValid = Items.All(p => p.IsValid);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsValid)));
    }

    private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
    {
      IsEditing = false;
    }

    private void OnTextBoxIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (_textBox.IsVisible)
      {
        Keyboard.Focus(_textBox);
      }
    }
  }
}
