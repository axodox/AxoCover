using System.Collections.Generic;

namespace AxoCover.Models
{
  public interface IMultiplexer
  {
    string Implementation { get; set; }
    IEnumerable<string> Implementations { get; }
  }
}