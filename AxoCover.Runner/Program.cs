using AxoCover.Common.Extensions;
using AxoCover.Common.Models;
using AxoCover.Common.ProcessHost;
using AxoCover.Common.Runner;
using AxoCover.Native;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading;

namespace AxoCover.Runner
{
  class Program
  {
    private static ManualResetEvent _isFinished = new ManualResetEvent(false);
    private static TimeSpan _closeTimeout = TimeSpan.FromSeconds(2);

    public static void Exit()
    {
      _isFinished.Set();
    }

    private static void Main(string[] args)
    {
      try
      {
#if DEBUG
        AppDomain.CurrentDomain.FirstChanceException += (o, e) => Console.WriteLine(e.Exception.GetDescription());
#endif

        RunnerMode runnerMode;
        int parentPid;

        if (args.Length < 2 || !Enum.TryParse(args[0], true, out runnerMode) || !int.TryParse(args[1], out parentPid) || !args.Skip(2).All(p => File.Exists(p)))
        {
          throw new Exception("Arguments are invalid.");
        }

        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

        var root = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        Assembly.LoadFrom(Path.Combine(root, Environment.Is64BitProcess ? "x64": "x86", "AxoCover.Native.dll"));

        RedirectFiles();

        foreach (var assemblyPath in args.Skip(2))
        {
          Console.Write($"Loading {assemblyPath}... ");
          Assembly.LoadFrom(assemblyPath);
          Console.WriteLine("Done.");
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

        Type serviceInterface;
        Type serviceImplementation;
        GetService(runnerMode, out serviceInterface, out serviceImplementation);

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

        _isFinished.WaitOne();
        Console.WriteLine("Exiting...");
        try
        {
          serviceHost.Close(_closeTimeout);
        }
        catch { }

        //Make sure to kill leftover non-background threads started by tests
        Environment.Exit(0);
      }
      catch (Exception e)
      {
#if DEBUG
        Debugger.Launch();
#endif
        var crashFilePath = Path.GetTempFileName();
        var crashDetails = JsonConvert.SerializeObject(new SerializableException(e));
        File.WriteAllText(crashFilePath, crashDetails);
        ServiceProcess.PrintServiceFailed(crashFilePath);
      }
    }

    private static void RedirectFiles()
    {
      var root = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
      var redirectedFiles = Directory
        .GetFiles(root, "map.txt", SearchOption.AllDirectories)
        .SelectMany(p => File.ReadAllLines(p).SelectMany(q => Directory.GetFiles(Path.GetDirectoryName(p), q)))
        .Distinct()
        .ToArray();
      FileRemapper.RedirectFiles(redirectedFiles);
    }

    private static void GetService(RunnerMode runnerMode, out Type serviceInterface, out Type serviceImplementation)
    {
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
