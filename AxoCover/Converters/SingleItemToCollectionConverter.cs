using System;
using System.Globalization;
using System.Windows.Data;

namespace AxoCover.Converters
{
  public class SingleItemToCollectionConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
      {
        return new object[0];
      }
      else
      {
        return new object[] { value };
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
