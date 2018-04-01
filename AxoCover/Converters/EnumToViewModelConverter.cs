using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace AxoCover.Converters
{
  public class EnumToViewModelConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value != null && value.GetType().IsEnum)
      {
        return Enum
          .GetValues(value.GetType())
          .OfType<object>()
          .Select(p => new
          {
            Name = value.GetType().GetMember(p.ToString()).FirstOrDefault().GetCustomAttribute<DescriptionAttribute>().Description,
            Value = p
          })
          .ToArray();
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
