using AxoCover.Common.Extensions;
using AxoCover.Common.ProcessHost;
using AxoCover.Common.Runner;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading;

namespace AxoCover.Runner
{
  class Program
  {
    static void Main(string[] args)
    {
      try
      {
        RunnerMode runnerMode;
        int parentPid;

        if (args.Length != 2 || !Enum.TryParse(args[0], true, out runnerMode) || !int.TryParse(args[1], out parentPid))
        {
          throw new Exception("Arguments are invalid.");
        }

        Process parentProcess = null;
        try
        {
          parentProcess = Process.GetProcessById(parentPid);
          parentProcess.EnableRaisingEvents = true;
          parentProcess.Exited += OnParentProcessExited;
        }
        catch (Exception e)
        {
          throw new Exception("Cannot open parent process.", e);
        }

        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

        Type serviceInterface;
        Type serviceImplementation;
        switch (runnerMode)
        {
          case RunnerMode.Discovery:
            serviceInterface = typeof(ITestDiscoveryService);
            serviceImplementation = typeof(TestDiscoveryService);
            break;
          case RunnerMode.Execution:
            serviceInterface = typeof(ITestExecutionService);
            serviceImplementation = typeof(TestExecutionService);
            break;
          default:
            throw new Exception("Invalid mode of usage specified!");
        }

        Console.WriteLine("AxoCover.Runner");
        Console.WriteLine("Copyright (c) 2016-2017 Péter Major");
        Console.WriteLine();

        Console.WriteLine($"Starting {args[0]} service...");
        var serviceAddress = NetworkingExtensions.GetServiceAddress();
        var serviceBinding = NetworkingExtensions.GetServiceBinding();

        var serviceHost = new ServiceHost(serviceImplementation, new[] { serviceAddress });
        serviceHost.AddServiceEndpoint(serviceInterface, serviceBinding, serviceAddress);
        serviceHost.Open();
        ServiceProcess.PrintServiceStarted(serviceAddress);

        try
        {
          Thread.Sleep(Timeout.Infinite);
        }
        catch
        {
          Console.WriteLine("Exiting...");
        }
      }
      catch (Exception e)
      {
        ServiceProcess.PrintServiceFailed();
        Console.WriteLine(e.GetDescription());
      }
    }

    private static void OnParentProcessExited(object sender, EventArgs e)
    {
      Console.WriteLine("Parent exited, runner will quit too.");
      Environment.Exit(0);
    }

    private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
      var assemblyName = new AssemblyName(args.Name).Name;
      var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
      return loadedAssemblies.FirstOrDefault(p => p.GetName().Name == assemblyName);
    }
  }
}
