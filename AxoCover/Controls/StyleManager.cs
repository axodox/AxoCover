using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AxoCover.Controls
{
  public static class StyleManager
  {
    public static readonly DependencyProperty IsHighlightedProperty = DependencyProperty.RegisterAttached(
      "IsHighlighted", typeof(bool), typeof(StyleManager),
      new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsArrange, OnIsHighlightedChanged));

    private static void OnIsHighlightedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is ContentPresenter && VisualTreeHelper.GetChildrenCount(d) > 0)
      {
        var target = VisualTreeHelper.GetChild(d, 0);
        target.SetCurrentValue(IsHighlightedProperty, e.NewValue);
      }
    }

    public static void SetIsHighlighted(UIElement element, bool value)
    {
      element.SetValue(IsHighlightedProperty, value);
    }

    public static bool GetIsHighlighted(UIElement element)
    {
      return (bool)element.GetValue(IsHighlightedProperty);
    }
  }
}
