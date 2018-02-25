using AxoCover.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AxoCover.Models.Toolkit
{
  public static class ObservableEnumeration
  {
    public static ObservableEnumeration<T> Create<T>(Func<IEnumerable<T>> enumerationSource, Comparison<T> onCompare = null)
    {
      return new ObservableEnumeration<T>(enumerationSource, onCompare);
    }
  }

  public class ObservableEnumeration<T> : ObservableCollection<T>
  {
    Func<IEnumerable<T>> _enumerationSource;

    Comparison<T> _onCompare;

    public ObservableEnumeration(Func<IEnumerable<T>> enumerationSource, Comparison<T> onCompare = null)
    {
      _enumerationSource = enumerationSource;
      _onCompare = onCompare;
      Refresh();
    }

    public void Refresh()
    {
      var itemsToDelete = this.ToList();

      var enumeration = _enumerationSource();
      foreach (var item in enumeration)
      {
        if (!itemsToDelete.Remove(item))
        {
          if (_onCompare != null)
          {
            this.OrderedAdd(item, _onCompare);
          }
          else
          {
            Add(item);
          }
        }
      }

      foreach (var item in itemsToDelete)
      {
        Remove(item);
      }
    }
  }
}
