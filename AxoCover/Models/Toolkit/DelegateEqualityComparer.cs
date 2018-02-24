using System;
using System.Collections.Generic;

namespace AxoCover.Models.Toolkit
{
  public class DelegateEqualityComparer<T> : IEqualityComparer<T>
  {
    private Func<T, T, bool> _equals;
    private Func<T, int> _getHashCode;

    public DelegateEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
    {
      _equals = equals;
      _getHashCode = getHashCode;
    }

    public bool Equals(T x, T y)
    {
      return _equals(x, y);
    }

    public int GetHashCode(T obj)
    {
      return _getHashCode(obj);
    }
  }
}
