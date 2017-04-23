using System;
using System.Linq;

namespace AxoCover.Host.x86
{
  public class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("AxoCover.Host-x86 is initializing...");
      AppDomain.CurrentDomain.ExecuteAssembly(args[0], args.Skip(1).ToArray());
    }
  }
}
