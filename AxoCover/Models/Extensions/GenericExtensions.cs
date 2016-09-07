using System;
using System.Collections.Generic;

namespace AxoCover.Models.Extensions
{
  public static class GenericExtensions
  {
    public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
      TValue value;
      if (dictionary.TryGetValue(key, out value))
      {
        return value;
      }
      else
      {
        return default(TValue);
      }
    }

    public static bool CheckAs<T>(this object value, Func<T, bool> func)
    {
      return value is T ? func((T)value) : false;
    }
  }
}
