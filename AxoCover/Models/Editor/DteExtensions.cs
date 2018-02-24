using EnvDTE;
using System;
using System.Linq;

namespace AxoCover.Models.Editor
{
  public static class DteExtensions
  {
    public static Command GetCommand(this DTE context, string name)
    {
      return context.Commands
        .OfType<Command>()
        .FirstOrDefault(p => StringComparer.OrdinalIgnoreCase.Equals(p.Name, name));
    }

    public static bool TryExecute(this DTE context, Command command, string commandArgs = "")
    {
      if (command.IsAvailable)
      {
        context.ExecuteCommand(command.Name, commandArgs);
        return true;
      }
      else
      {
        return false;
      }
    }
  }
}
