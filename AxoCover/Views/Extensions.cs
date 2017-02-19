using System.Windows;
using System.Windows.Input;

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

    public static readonly DependencyProperty InputBindingsProperty =
      DependencyProperty.RegisterAttached("InputBindings", typeof(InputBindingCollection), typeof(Extensions),
        new FrameworkPropertyMetadata(new InputBindingCollection(), OnInputBindingsChanged));

    private static void OnInputBindingsChanged
      (DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      var element = sender as UIElement;
      if (element == null) return;
      element.InputBindings.Clear();
      element.InputBindings.AddRange((InputBindingCollection)e.NewValue);
    }

    public static InputBindingCollection GetInputBindings(UIElement element)
    {
      return (InputBindingCollection)element.GetValue(InputBindingsProperty);
    }

    public static void SetInputBindings(UIElement element, InputBindingCollection inputBindings)
    {
      element.SetValue(InputBindingsProperty, inputBindings);
    }
  }
}
