using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AxoCover.Models
{
  public class TypeTemplateSelector : DataTemplateSelector
  {
    public List<DataTemplate> Templates { get; set; }

    public TypeTemplateSelector()
    {
      Templates = new List<DataTemplate>();
    }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (item == null || Templates == null)
      {
        return null;
      }

      var typeName = item.GetType().Name;
      return Templates.FirstOrDefault(p => (p.DataType as string ?? ":").EndsWith(typeName));
    }
  }
}
