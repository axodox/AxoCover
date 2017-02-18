using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AxoCover.Converters
{
  public class EnumToValuesConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value != null && value.GetType().IsEnum)
      {
        return Enum.GetValues(value.GetType());
      }
      else
      {
        return DependencyProperty.UnsetValue;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }
}
