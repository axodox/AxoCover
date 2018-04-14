using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AxoCover.Converters
{
  public class TimeSpanFormatter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if(value is TimeSpan time)
      {
        if(time.TotalMilliseconds < 1)
        {
          return $"{time.TotalMilliseconds*1000:G3} ns";
        }
        else if(time.TotalSeconds < 1)
        {
          return $"{time.TotalMilliseconds:G3} ms";
        }
        else if (time.TotalMinutes < 1)
        {
          return $"{time.TotalSeconds:G3} s";
        }
        else
        {
          return $"{time.TotalMinutes:G3} min";
        }
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
