namespace AxoCover.Common.ProcessHost
{
  public interface IProcessInfo
  {
    string FilePath { get; }
    string Arguments { get; }
  }
}
