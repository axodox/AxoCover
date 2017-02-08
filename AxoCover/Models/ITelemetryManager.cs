using System;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public interface ITelemetryManager
  {
    bool IsTelemetryEnabled { get; set; }

    Task<bool> UploadExceptionAsync(Exception exception);
  }
}