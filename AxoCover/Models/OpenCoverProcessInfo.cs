using AxoCover.Common.ProcessHost;
using AxoCover.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AxoCover.Models
{
  public class OpenCoverProcessInfo : IHostProcessInfo
  {
    private const string _runnerName = @"OpenCover\OpenCover.Console.exe";
    protected readonly static string _runnerPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _runnerName);

    private readonly string _baseArguments;
    public string Arguments
    {
      get
      {
        if (GuestProcess == null)
        {
          throw new Exception("Child process must be specified.");
        }

        return _baseArguments + $" -target:\"{GuestProcess.FilePath}\" -targetargs:\"{GuestProcess.Arguments.Replace("\"", "\\\"")}\"";
      }
    }

    public string FilePath
    {
      get
      {
        return _runnerPath;
      }
    }

    public IProcessInfo GuestProcess { get; set; }

    public OpenCoverProcessInfo(IEnumerable<string> codeAssemblies, IEnumerable<string> testAssemblies, string coverageReportPath)
    {
      _baseArguments = GetSettingsBasedArguments(codeAssemblies, testAssemblies) + $"-mergebyhash -output:\"{coverageReportPath}\" -register:user";
    }

    public static string GetSettingsBasedArguments(IEnumerable<string> codeAssemblies, IEnumerable<string> testAssemblies)
    {
      var arguments = string.Empty;

      if (Settings.Default.IsCoveringByTest)
      {
        arguments += " -coverbytest:" + string.Join(";", testAssemblies.Select(p => "*" + p + "*"));
      }

      if (!string.IsNullOrWhiteSpace(Settings.Default.ExcludeAttributes))
      {
        arguments += $" \"-excludebyattribute:{Settings.Default.ExcludeAttributes}\"";
      }

      if (!string.IsNullOrWhiteSpace(Settings.Default.ExcludeFiles))
      {
        arguments += $" \"-excludebyfile:{Settings.Default.ExcludeFiles}\"";
      }

      if (!string.IsNullOrWhiteSpace(Settings.Default.ExcludeDirectories))
      {
        arguments += $" \"-excludedirs:{Settings.Default.ExcludeDirectories}\"";
      }

      var filters = string.Empty;
      if (Settings.Default.IsIncludingSolutionAssemblies)
      {
        filters += GetAssemblyList(codeAssemblies);

        if (!Settings.Default.IsExcludingTestAssemblies)
        {
          filters += GetAssemblyList(testAssemblies);
        }
      }
      else if (!string.IsNullOrWhiteSpace(Settings.Default.Filters))
      {
        filters += Settings.Default.Filters;

        if (Settings.Default.IsExcludingTestAssemblies)
        {
          filters += GetAssemblyList(testAssemblies, false);
        }
      }

      if (!string.IsNullOrWhiteSpace(filters))
      {
        arguments += $" \"-filter:{filters}\"";
      }

      return arguments + " -hideskipped:All ";
    }

    private static string GetAssemblyList(IEnumerable<string> assemblies, bool isInclusive = true)
    {
      return string.Join(" ", assemblies.Select(p => (isInclusive ? "+" : "-") + "[" + p + "]*"));
    }
  }
}
