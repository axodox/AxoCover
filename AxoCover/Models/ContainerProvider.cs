using AxoCover.Models.Commands;
using AxoCover.Models.TestCaseProcessors;
using Microsoft.Practices.Unity;

namespace AxoCover.Models
{
  public static class ContainerProvider
  {
    private static IUnityContainer _container;
    public static IUnityContainer Container
    {
      get
      {
        if (_container == null)
        {
          _container = new UnityContainer();
          RegisterTypes();
        }
        return _container;
      }
    }

    private static void RegisterTypes()
    {
      Container.RegisterType<ITestCaseProcessor, XUnitTestCaseProcessor>("xUnit");
      Container.RegisterType<ITestCaseProcessor, NUnitTestCaseProcessor>("NUnit");
      Container.RegisterType<ITestProvider, TestProvider>(new ContainerControlledLifetimeManager());
      Container.RegisterType<IEditorContext, EditorContext>(new ContainerControlledLifetimeManager());
      Container.RegisterType<ITestRunner, AxoTestRunner>("axo");
      Container.RegisterType<ITestRunner, MultiplexedTestRunner>(new ContainerControlledLifetimeManager());
      Container.RegisterType<ICoverageProvider, CoverageProvider>(new ContainerControlledLifetimeManager());
      Container.RegisterType<IResultProvider, ResultProvider>(new ContainerControlledLifetimeManager());
      Container.RegisterType<IStorageController, StorageController>(new ContainerControlledLifetimeManager());
      Container.RegisterType<IReportProvider, ReportProvider>(new ContainerControlledLifetimeManager());
      Container.RegisterType<ITelemetryManager, HockeyClient>(new ContainerControlledLifetimeManager());
      Container.RegisterType<IOptions, Options>(new ContainerControlledLifetimeManager());
      Container.RegisterType<IReleaseManager, ReleaseManager>(new ContainerControlledLifetimeManager());
      Container.RegisterInstance(new SelectTestCommand());
      Container.RegisterInstance(new JumpToTestCommand());
      Container.RegisterInstance(new DebugTestCommand());
    }
  }
}
