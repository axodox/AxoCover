using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AxoCover.Controls
{
  /// <summary>
  /// Interaction logic for ColorPicker.xaml
  /// </summary>
  public partial class ColorPicker : UserControl
  {
    public static DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color), typeof(ColorPicker), new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public Color Color
    {
      get { return (Color)GetValue(ColorProperty); }
      set { SetValue(ColorProperty, value); }
    }

    public static DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(ColorPicker), new PropertyMetadata(string.Empty));

    public string Text
    {
      get { return (string)GetValue(TextProperty); }
      set { SetValue(TextProperty, value); }
    }

    public ColorPicker()
    {
      InitializeComponent();
    }

    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
      using (var dialog = new System.Windows.Forms.ColorDialog())
      {
        dialog.Color = System.Drawing.Color.FromArgb(
          Color.A,
          Color.R,
          Color.G,
          Color.B);

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
          Color = Color.FromArgb(
            dialog.Color.A,
            dialog.Color.R,
            dialog.Color.G,
            dialog.Color.B);
        }
      }
    }
  }
}
