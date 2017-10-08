using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace AxoCover.Common.Extensions
{
  public static class NetworkingExtensions
  {
    public static readonly TimeSpan NetworkTimeout = TimeSpan.MaxValue;
    public static readonly TimeSpan SessionTimeout = TimeSpan.MaxValue;

    public static Binding GetServiceBinding(CommunicationProtocol protocol)
    {
      switch(protocol)
      {
        case CommunicationProtocol.Tcp:
          return GetTcpBinding();
        case CommunicationProtocol.MemoryPipe:
          return GetMemoryTypeBinding();
        default:
          throw new NotImplementedException();
      }
    }

    private static NetTcpBinding GetTcpBinding()
    {
      var binding = new NetTcpBinding(SecurityMode.None)
      {
        MaxReceivedMessageSize = int.MaxValue,
        ReceiveTimeout = TimeSpan.MaxValue,
        SendTimeout = NetworkTimeout,
        OpenTimeout = NetworkTimeout,
        CloseTimeout = NetworkTimeout
      };
      binding.ReliableSession.Enabled = true;
      binding.ReliableSession.InactivityTimeout = SessionTimeout;
      binding.ReliableSession.Ordered = true;
      return binding;
    }

    private static Binding GetMemoryTypeBinding()
    {
      var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
      {
        MaxReceivedMessageSize = int.MaxValue,
        ReceiveTimeout = TimeSpan.MaxValue,
        SendTimeout = NetworkTimeout,
        OpenTimeout = NetworkTimeout,
        CloseTimeout = NetworkTimeout
      };
      return binding;
    }

    public static Uri GetServiceAddress(CommunicationProtocol protocol)
    {
      switch (protocol)
      {
        case CommunicationProtocol.Tcp:
          return GetTcpAddress();
        case CommunicationProtocol.MemoryPipe:
          return GetMemoryTypeAddress();
        default:
          throw new NotImplementedException();
      }
    }

    private static Uri GetTcpAddress(int minPort = 49152, int maxPort = IPEndPoint.MaxPort)
    {
      var ipProperties = IPGlobalProperties.GetIPGlobalProperties();

      var usedPorts = new List<int>();
      usedPorts.AddRange(ipProperties
        .GetActiveTcpConnections()
        .Select(p => p.LocalEndPoint.Port));
      usedPorts.AddRange(ipProperties
        .GetActiveTcpListeners()
        .Select(p => p.Port));

      var random = new Random();
      var portCount = maxPort - minPort;
      var portToTest = random.Next(minPort, maxPort);
      for (var portIndex = 0; portIndex < portCount; portIndex++, portToTest++)
      {
        if (portToTest > maxPort)
        {
          portToTest = minPort;
        }

        if (usedPorts.Contains(portToTest)) continue;

        try
        {
          var tcpListener = new TcpListener(IPAddress.Loopback, portToTest);
          tcpListener.Start();
          tcpListener.Stop();
          return new Uri($"net.tcp://{IPAddress.Loopback}:{portToTest}");
        }
        catch
        {

        }
      }

      throw new Exception("All ports in the specified segment are in use.");
    }

    private static Uri GetMemoryTypeAddress()
    {
      return new Uri($"net.pipe://localhost/axoCover-runner/" + Guid.NewGuid().ToString());
    }
  }
}
