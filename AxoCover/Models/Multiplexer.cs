using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxoCover.Models
{
  public class Multiplexer<T> : IMultiplexer
  {
    protected readonly Dictionary<string, T> _implementations;
    public IEnumerable<string> Implementations
    {
      get
      {
        return _implementations.Keys;
      }
    }

    protected T _implementation;
    public string Implementation
    {
      get
      {
        return _implementation.GetType().Name;
      }
      set
      {
        if (!_implementations.ContainsKey(value))
        {
          throw new ArgumentOutOfRangeException(nameof(value), $"Implementation {value} does not exist!");
        }
        _implementation = _implementations[value];
        OnImplementationChanged();
      }
    }

    public Multiplexer(IUnityContainer unityContainer)
    {
      _implementations = unityContainer
        .ResolveAll<T>()
        .ToDictionary(p => p.GetType().Name);

      if (!_implementations.Any())
        throw new InvalidOperationException("There are no implementations registered.");

      _implementation = _implementations.Values.First();
    }

    protected virtual void OnImplementationChanged()
    {

    }
  }
}
