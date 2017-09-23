using AxoCover.Common.Extensions;
using System;
using System.IO;

namespace AxoCover.Models
{
  public class IoProvider : IIoProvider
  {
    private static readonly char[] _pathSeparators =
    {
      Path.DirectorySeparatorChar,
      Path.AltDirectorySeparatorChar
    };

    private readonly IOptions _options;

    public IoProvider(IOptions options)
    {
      _options = options;
    }

    public string GetAbsolutePath(string relativePath)
    {
      return GetAbsolutePath(Path.GetDirectoryName(_options.SolutionSettingsPath), relativePath);
    }

    public static string GetAbsolutePath(string sourcePath, string targetPath)
    {
      if (!Path.IsPathRooted(targetPath))
      {
        return new DirectoryInfo(Path.Combine(sourcePath, targetPath)).FullName;
      }
      else
      {
        return targetPath;
      }
    }

    public string GetRelativePath(string absolutePath)
    {
      return GetRelativePath(Path.GetDirectoryName(_options.SolutionSettingsPath), absolutePath);
    }

    public static string GetRelativePath(string sourcePath, string targetPath)
    {
      if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(targetPath))
        return targetPath;

      var sourceParts = sourcePath.Split(_pathSeparators, StringSplitOptions.RemoveEmptyEntries);
      var targetParts = targetPath.Split(_pathSeparators, StringSplitOptions.RemoveEmptyEntries);

      if (!StringComparer.OrdinalIgnoreCase.Equals(sourceParts[0], targetParts[0]))
      {
        return targetPath;
      }

      var outputPath = string.Empty;
      var index = 0;
      var isSame = true;
      while (index < sourceParts.Length || index < targetParts.Length)
      {
        isSame &= StringComparer.OrdinalIgnoreCase.Equals(sourceParts.Item(index), targetParts.Item(index));

        if (!isSame)
        {
          if (index < targetParts.Length)
          {
            if (index < sourceParts.Length)
            {
              outputPath = Path.Combine("..", outputPath, targetParts[index]);
            }
            else
            {
              outputPath = Path.Combine(outputPath, targetParts[index]);
            }
          }
          else
          {
            outputPath = Path.Combine("..", outputPath);
          }
        }
        index++;
      }
      return outputPath;
    }
  }
}
