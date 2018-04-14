using AxoCover.Common.Extensions;
using AxoCover.Common.ProcessHost;
using AxoCover.Models.Testing.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxoCover.Models.Testing.Execution
{
  public class OpenCoverProcessInfo : IHostProcessInfo
  {
    private const string _runnerName = @"OpenCover\OpenCover.Console.exe";

    private readonly string _baseArguments;
    public string Arguments
    {
      get
      {
        if (GuestProcess != null)
        {
          return _baseArguments + $" -target:\"{GuestProcess.FilePath}\" -targetargs:\"{GuestProcess.Arguments.Replace("\"", "\\\"")}\"";
        }
        else
        {
          throw new InvalidOperationException("The guest process is not specified.");
        }
      }
    }

    public string FilePath
    {
      get
      {
        return _runnerName.ToAbsolutePath();
      }
    }

    public IProcessInfo GuestProcess { get; set; }

    public string CoverageReportPath { get; private set; }

    public OpenCoverProcessInfo(OpenCoverOptions options)
    {
      CoverageReportPath = options.CoverageReportPath;
      _baseArguments = GetSettingsBasedArguments(options) + $" -output:\"{options.CoverageReportPath}\" -register:user";
    }

    private static string GetSettingsBasedArguments(OpenCoverOptions options)
    {
      var arguments = string.Empty;

      if (options.IsMergingByHash)
      {
        arguments += " -mergebyhash";
      }

      if (options.IsSkippingAutoProps)
      {
        arguments += " -skipautoprops";
      }

      if (options.IsCoveringByTest)
      {
        arguments += " -coverbytest:" + string.Join(";", options.TestAssemblies.Select(p => "*" + p + "*"));
      }

      if(options.IsVisitorCountLimited)
      {
        arguments += " -threshold:" + options.VisitorCountLimit;
      }

      if (!string.IsNullOrWhiteSpace(options.ExcludeAttributes))
      {
        arguments += $" \"-excludebyattribute:{options.ExcludeAttributes}\"";
      }

      if (!string.IsNullOrWhiteSpace(options.ExcludeFiles))
      {
        arguments += $" \"-excludebyfile:{options.ExcludeFiles}\"";
      }

      if (!string.IsNullOrWhiteSpace(options.ExcludeDirectories))
      {
        arguments += $" \"-excludedirs:{options.ExcludeDirectories}\"";
      }

      var filters = string.Empty;
      if (options.IsIncludingSolutionAssemblies)
      {
        filters += GetAssemblyList(options.CodeAssemblies);

        if (!options.IsExcludingTestAssemblies)
        {
          filters += GetAssemblyList(options.TestAssemblies);
        }
      }
      else
      {
        filters += "-[AxoCover.*]* ";

        if (!string.IsNullOrWhiteSpace(options.Filters))
        {
          filters += options.Filters;

          if (options.IsExcludingTestAssemblies)
          {
            filters += GetAssemblyList(options.TestAssemblies, false);
          }
        }
      }

      if (!string.IsNullOrWhiteSpace(filters))
      {
        arguments += $" \"-filter:{filters.Trim()}\"";
      }

      arguments += $" \"-searchdirs:{string.Join(";", options.PdbDirectories)}\"";

      return arguments + " -hideskipped:All ";
    }

    private static string GetAssemblyList(IEnumerable<string> assemblies, bool isInclusive = true)
    {
      return string.Join(" ", assemblies.Select(p => (isInclusive ? "+" : "-") + "[" + p + "]*")) + " ";
    }
  }
}
