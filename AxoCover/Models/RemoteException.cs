using AxoCover.Common.Models;
using System;

namespace AxoCover.Models
{
  public class RemoteException : Exception
  {
    public SerializableException RemoteReason { get; private set; }

    public RemoteException(string message, SerializableException remoteReason)
      : base(message)
    {
      RemoteReason = remoteReason;
    }
  }
}
