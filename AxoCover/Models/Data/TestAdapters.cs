using System;

namespace AxoCover.Models.Data
{
  [Flags]
  public enum TestAdapterKinds
  {
    None = 0,
    MSTestV1 = 1,
    MSTestV2 = 2,
    NUnit = 4,
    XUnit = 8
  }
}
