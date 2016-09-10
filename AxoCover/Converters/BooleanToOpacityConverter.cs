using System;
using System.Globalization;
using System.Windows.Data;

namespace AxoCover.Converters
{
  public class BooleanToOpacityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return true.Equals(value) ? 1d : 0.5d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
