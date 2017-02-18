using System;
using System.Threading;

namespace AxoCover.Common.Settings
{
  public enum TestApartmentState
  {
    STA,
    MTA
  }

  public static class TestApartmentStateExtensions
  {
    public static ApartmentState ToApartmentState(this TestApartmentState state)
    {
      switch (state)
      {
        case TestApartmentState.STA:
          return ApartmentState.STA;
        case TestApartmentState.MTA:
          return ApartmentState.MTA;
        default:
          throw new ArgumentOutOfRangeException($"State {state} is not supported.");
      }
    }
  }
}
