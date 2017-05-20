using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxoCover.Common.Models
{
  public class SerializableException
  {
    public string Type { get; set; }

    public string Message { get; set; }

    public string StackTrace { get; set; }

    public SerializableException InnerException { get; set; }

    public SerializableException() { }

    public SerializableException(Exception exception)
    {
      if(exception == null)
      {
        throw new ArgumentNullException(nameof(exception));
      }

      Type = exception.GetType().FullName;
      Message = exception.Message;
      StackTrace = exception.StackTrace;

      if(exception.InnerException != null)
      {
        InnerException = new SerializableException(exception.InnerException);
      }
    }

    public static implicit operator SerializableException(Exception exception)
    {
      return new SerializableException(exception);
    }
  }
}
