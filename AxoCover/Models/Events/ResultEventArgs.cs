using System;

namespace AxoCover.Models.Events
{
  public class ResultEventArgs<T> : EventArgs
  {
    public T Result { get; private set; }

    public ResultEventArgs(T result)
    {
      Result = result;
    }
  }
}
