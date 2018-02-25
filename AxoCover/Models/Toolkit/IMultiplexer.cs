using System.Collections.Generic;

namespace AxoCover.Models.Toolkit
{
  public interface IMultiplexer
  {
    string Implementation { get; set; }
    IEnumerable<string> Implementations { get; }
  }
}
