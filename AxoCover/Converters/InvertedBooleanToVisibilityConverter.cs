using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AxoCover.Converters
{
  public class InvertedBooleanToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return (value is bool && (bool)value == false) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
