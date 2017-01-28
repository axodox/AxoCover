using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AxoCover.Converters
{
  public class BooleanInverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return Invert(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return Invert(value);
    }

    private static object Invert(object value)
    {
      if (value is bool)
      {
        return !(bool)value;
      }
      else
      {
        return DependencyProperty.UnsetValue;
      }
    }
  }
}
