using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceModel;

namespace AxoCover.Common.Extensions
{
  public static class NetworkingExtensions
  {
    public static readonly TimeSpan NetworkTimeout = TimeSpan.MaxValue;
    public static readonly TimeSpan SessionTimeout = TimeSpan.MaxValue;

    public static NetTcpBinding GetServiceBinding()
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

      var random = new Random();
      var portCount = maxPort - minPort;
      var portToTest = random.Next(minPort, maxPort);
      for (var portIndex = 0; portIndex < portCount; portIndex++, portToTest++)
      {
        if(portToTest > maxPort)
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
  }
}
