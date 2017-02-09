using System.Windows;

namespace AxoCover.Views
{
  public class Extensions
  {
    public static DependencyProperty LayoutModeProperty =
      DependencyProperty.RegisterAttached("LayoutMode", typeof(string), typeof(Extensions),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

    public static string GetLayoutMode(DependencyObject obj)
    {
      return (string)obj.GetValue(LayoutModeProperty);
    }

    public static void SetLayoutMode(DependencyObject obj, string value)
    {
      obj.SetValue(LayoutModeProperty, value);
    }
  }
}
