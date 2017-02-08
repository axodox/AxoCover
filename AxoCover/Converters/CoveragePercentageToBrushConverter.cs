using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AxoCover.Converters
{
  public class CoveragePercentageToBrushConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is double)
      {
        var position = (double)value / 100d;
        return new SolidColorBrush(Color.FromRgb(
          ToByte(Red(position)),
          ToByte(Green(position)),
          ToByte(Blue(position))));
      }
      else
      {
        return DependencyProperty.UnsetValue;
      }
    }

    private static double Red(double value)
    {
      if (value < 0.5)
      {
        return 1;
      }
      else
      {
        return 1 - 2 * (value - 0.5);
      }
    }

    private static double Green(double value)
    {
      if (value < 0.5)
      {
        return 2 * value;
      }
      else
      {
        return 1;
      }
    }

    private static double Blue(double value)
    {
      return 0;
    }

    private static byte ToByte(double value)
    {
      return (byte)(value * 255);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
