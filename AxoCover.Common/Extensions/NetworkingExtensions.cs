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

    public static Binding GetServiceBinding()
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

    public static Uri GetServiceAddress()
    {
      return new Uri($"net.pipe://localhost/axoCover-runner/" + Guid.NewGuid().ToString());
    }
  }
}
