using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AxoCover.Converters
{
  public class StringEqualityToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value as string == parameter as string ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }
}
