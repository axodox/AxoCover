using AxoCover.Common.Extensions;
using AxoCover.Common.ProcessHost;
using AxoCover.Common.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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
        if (args.Length == 0)
        {
          Console.WriteLine("This process is used by AxoCover to discover and run tests.");
        }

        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

        Type serviceInterface;
        Type serviceImplementation;
        switch (args[0])
        {
          case "discovery":
            serviceInterface = typeof(ITestDiscoveryService);
            serviceImplementation = typeof(TestDiscoveryService);
            break;
          case "execution":
            serviceInterface = typeof(ITestExecutionService);
            serviceImplementation = typeof(TestExecutionService);
            break;
          default:
            Console.WriteLine("Please specify the mode of usage.");
            return;
        }

        Console.WriteLine("AxoCover.Runner");
        Console.WriteLine("Copyright (c) 2016-2017 Péter Major");
        Console.WriteLine();

        Console.WriteLine($"Starting {args[0]} service...");
        var serviceAddress = GetServiceAddress();
        var serviceBinding = GetServiceBinding();

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
        Console.WriteLine("AxoCover.Runner failed.");
        Console.WriteLine(e.GetDescription());
      }
    }

    private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
      var assemblyName = new AssemblyName(args.Name).Name;
      var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
      return loadedAssemblies.FirstOrDefault(p => p.GetName().Name == assemblyName);
    }

    public static Uri GetServiceAddress(int minPort = 49152, int maxPort = IPEndPoint.MaxPort)
    {
      var ipProperties = IPGlobalProperties.GetIPGlobalProperties();

      var usedPorts = new List<int>();
      usedPorts.AddRange(ipProperties
        .GetActiveTcpConnections()
        .Select(p => p.LocalEndPoint.Port));
      usedPorts.AddRange(ipProperties
        .GetActiveTcpListeners()
        .Select(p => p.Port));

      int port;
      for (port = minPort; port < maxPort; port++)
      {
        if (usedPorts.Contains(port)) continue;

        try
        {
          var tcpListener = new TcpListener(IPAddress.Any, port);
          tcpListener.Start();
          tcpListener.Stop();
          break;
        }
        catch
        {

        }
      }

      if (port == maxPort)
      {
        throw new Exception("All ports in the specified segment are in use.");
      }

      return new Uri($"net.tcp://{ipProperties.HostName}:{port}");
    }

    public static NetTcpBinding GetServiceBinding()
    {
      var binding = new NetTcpBinding(SecurityMode.None)
      {
        MaxReceivedMessageSize = int.MaxValue,
        ReceiveTimeout = TimeSpan.MaxValue,
      };
      binding.ReliableSession.Enabled = true;
      binding.ReliableSession.InactivityTimeout = TimeSpan.FromSeconds(5);
      binding.ReliableSession.Ordered = true;
      return binding;
    }
  }
}
