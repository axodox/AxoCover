using AxoCover.Common.Extensions;
using AxoCover.Common.Models;
using AxoCover.Common.ProcessHost;
using AxoCover.Common.Runner;
using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;

namespace AxoCover.Models
{
  public abstract class TestProcess<TServiceInterface> : ServiceProcess
    where TServiceInterface : class, ITestService
  {
    protected TServiceInterface TestService { get; private set; }

    private readonly ManualResetEvent _serviceStartedEvent = new ManualResetEvent(false);    

    private int _serviceProcessId;

    private SerializableException _failReason;

    public TestProcess(IProcessInfo processInfo) : base(processInfo)
    {
      Exited += OnExited;

      _serviceStartedEvent.WaitOne();
      if (TestService == null)
      {
        throw new RemoteException("Could not create service.", _failReason);
      }
    }

    protected override void OnServiceStarted()
    {
      var channelFactory = new DuplexChannelFactory<TServiceInterface>(this, NetworkingExtensions.GetServiceBinding());
      TestService = channelFactory.CreateChannel(new EndpointAddress(ServiceUri));
      try
      {
        _serviceProcessId = TestService.Initialize();
      }
      catch
      {
        TestService = null;
      }
      _serviceStartedEvent.Set();
    }

    protected override void OnServiceFailed(SerializableException exception)
    {
      _failReason = exception;
      _serviceStartedEvent.Set();
    }

    private void OnExited(object sender, EventArgs e)
    {
      if (TestService != null)
      {
        (TestService as ICommunicationObject).Abort();
      }
    }

    public bool TryShutdown()
    {
      try
      {
        TestService.Shutdown();
        return true;
      }
      catch
      {
        try
        {
          Process.GetProcessById(_serviceProcessId).KillWithChildren();
          return true;
        }
        catch
        {
          return false;
        }
      }
    }
  }
}
