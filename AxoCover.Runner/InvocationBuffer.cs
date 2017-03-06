using Castle.DynamicProxy;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace AxoCover.Runner
{
  public abstract class InvocationBuffer
  {
    public static T Create<T>(Predicate<Exception> handleException)
    {
      var invocationBuffer = new InvocationBuffer<T>(handleException);

      var proxyGenerator = new ProxyGenerator();
      var proxy = proxyGenerator.CreateInterfaceProxyWithTarget(
        typeof(IInvocationBuffer<T>),
        new[] { typeof(T) },
        invocationBuffer,
        new[] { invocationBuffer });

      return (T)proxy;
    }
  }

  public interface IInvocationBuffer<T> : IDisposable
  {
    T Target { get; set; }
  }

  public class InvocationBuffer<T> : InvocationBuffer, IInterceptor, IInvocationBuffer<T>
  {
    private AutoResetEvent _invocationReady = new AutoResetEvent(false);
    private ConcurrentQueue<IInvocation> _invocations = new ConcurrentQueue<IInvocation>();
    private bool _isFinished;
    private Predicate<Exception> _handleException;
    private Thread _invocationThread;

    private T _target;
    public T Target
    {
      get { return _target; }
      set
      {
        _target = value;
        if (value != null)
        {
          _invocationReady.Set();
        }
      }
    }

    public InvocationBuffer(Predicate<Exception> handleException)
    {
      _handleException = handleException;
      _invocationThread = new Thread(InvocationWorker)
      {
        IsBackground = true,
        Name = "Invocator for " + typeof(T).Name
      };
      _invocationThread.Start();
    }

    private void InvocationWorker()
    {
      IInvocation invocation;
      object target;
      while (!_isFinished)
      {
        if (_target == null || _invocations.Count == 0)
        {
          _invocationReady.WaitOne();
        }

        target = _target;
        if (target != null && _invocations.TryPeek(out invocation))
        {
          try
          {
            invocation.Method.Invoke(target, invocation.Arguments);
            _invocations.TryDequeue(out invocation);
          }
          catch (Exception e)
          {
            if (_handleException(e))
            {
              _invocations.TryDequeue(out invocation);
            }
          }
        }
      }
    }

    public void Intercept(IInvocation invocation)
    {
      if (invocation.Method.DeclaringType == typeof(IInvocationBuffer<T>))
      {
        invocation.Proceed();
      }
      else
      {
        if (_isFinished)
        {
          throw new ObjectDisposedException(nameof(InvocationBuffer<T>));
        }

        if (invocation.Method.ReturnType != typeof(void))
        {
          throw new InvalidOperationException("Cannot buffer calls with return values.");
        }

        _invocations.Enqueue(invocation);
        _invocationReady.Set();
      }
    }

    public void Dispose()
    {
      if (_isFinished) return;

      _isFinished = true;
      _invocationReady.Set();
      _invocationThread.Join();

      if (_target != null)
      {
        IInvocation invocation;
        while (_invocations.TryDequeue(out invocation))
        {
          try
          {
            invocation.Method.Invoke(_target, invocation.Arguments);
          }
          catch (Exception e)
          {
            _handleException(e);
          }
        }
      }
      GC.SuppressFinalize(this);
    }
  }
}
