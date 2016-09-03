using System;
using System.Linq;
using System.Reflection;
using System.Security.Policy;

namespace AxoCover.Model
{
  sealed class Isolated<T> : IDisposable
    where T : MarshalByRefObject
  {
    private AppDomain _domain;
    private T _value;

    static Isolated()
    {
      AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
    }

    private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
      var assemblyName = new AssemblyName(args.Name);
      return AppDomain.CurrentDomain
        .GetAssemblies()
        .FirstOrDefault(p => p.GetName().Name == assemblyName.Name);
    }

    public Isolated()
    {
      _domain = AppDomain.CreateDomain("Isolated: " + Guid.NewGuid(), new Evidence(AppDomain.CurrentDomain.Evidence), AppDomain.CurrentDomain.SetupInformation);
      _value = (T)_domain.CreateInstanceFromAndUnwrap(typeof(T).Assembly.Location, typeof(T).FullName);
    }

    public T Value
    {
      get
      {
        return _value;
      }
    }

    public void Dispose()
    {
      if (_domain != null)
      {
        AppDomain.Unload(_domain);
        _domain = null;
      }
    }
  }
}
