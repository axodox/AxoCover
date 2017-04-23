using System;
using System.Linq;

namespace AxoCover.Host.x64
{
  public class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("AxoCover.Host-x64 is initializing...");
      AppDomain.CurrentDomain.ExecuteAssembly(args[0], args.Skip(1).ToArray());
    }
  }
}
