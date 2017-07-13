using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

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
