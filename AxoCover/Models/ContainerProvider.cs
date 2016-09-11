using AxoCover.Models.Commands;
using Microsoft.Practices.Unity;

namespace AxoCover.Models
{
  public static class ContainerProvider
  {
    public static UnityContainer Container;

    public static void Initialize()
    {
      if (Container != null)
      {
        return;
      }

      Container = new UnityContainer();
      Container.RegisterType<ITestAssemblyScanner, IsolatedTestAssemblyScanner>(new ContainerControlledLifetimeManager());
      Container.RegisterType<ITestProvider, TestProvider>(new ContainerControlledLifetimeManager());
      Container.RegisterType<IEditorContext, EditorContext>(new ContainerControlledLifetimeManager());
      Container.RegisterType<ITestRunner, TestRunner>(new ContainerControlledLifetimeManager());
      Container.RegisterType<ICoverageProvider, CoverageProvider>(new ContainerControlledLifetimeManager());
      Container.RegisterType<IResultProvider, ResultProvider>(new ContainerControlledLifetimeManager());
      Container.RegisterInstance(new NavigateToTestCommand());
    }
  }
}
