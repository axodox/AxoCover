using AxoCover.Common.Extensions;

namespace AxoCover.Common.ProcessHost
{
  public class ProcessInfo : IProcessInfo
  {
    public string Arguments { get; private set; }

    public string FilePath { get; private set; }

    public ProcessInfo(string filePath, string arguments)
    {
      FilePath = filePath.ToAbsolutePath();
      Arguments = arguments;
    }
  }
}
