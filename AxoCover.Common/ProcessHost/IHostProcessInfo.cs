namespace AxoCover.Common.ProcessHost
{
  public interface IHostProcessInfo : IProcessInfo
  {
    IProcessInfo GuestProcess { get; set; }
  }
}
