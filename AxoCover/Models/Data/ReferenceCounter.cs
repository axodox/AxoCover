using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxoCover.Models.Data
{
  public class ReferenceCounter : IReferenceCounter
  {
    private readonly Dictionary<string, int> _referenceDictinary = new Dictionary<string, int>();
    private readonly object _syncRoot = new object();

    public int this[string key]
    {
      get
      {
        lock (_syncRoot)
        {
          if (_referenceDictinary.TryGetValue(key, out var count))
          {
            return count;
          }
          else
          {
            return 0;
          }
        }
      }
    }

    public int Increase(string key)
    {
      lock (_syncRoot)
      {
        if (_referenceDictinary.TryGetValue(key, out var count))
        {
          _referenceDictinary[key] = ++count;
          return count;
        }
        else
        {
          _referenceDictinary[key] = 1;
          return 1;
        }
      }
    }

    public int Decrease(string key)
    {
      lock (_syncRoot)
      {
        if (_referenceDictinary.TryGetValue(key, out var count))
        {
          _referenceDictinary[key] = --count;

          if (count == 0)
          {
            _referenceDictinary.Remove(key);
          }
          return count;
        }
        else
        {
          return 0;
        }
      }
    }
  }
}
