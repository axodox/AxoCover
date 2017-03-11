using AxoCover.Models.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public class StorageController : IStorageController
  {
    private const string _axoCoverDirectory = ".axoCover";
    private const string _testRunDirectory = "runs";
    private const string _testReportDirectory = "reports";
    private readonly IEditorContext _editorContext;

    public string AxoCoverRoot
    {
      get
      {
        if (_editorContext.Solution.IsOpen)
        {
          return Path.Combine(Path.GetDirectoryName(_editorContext.Solution.FileName), _axoCoverDirectory);
        }
        else
        {
          return null;
        }
      }
    }

    public string CreateTestRunDirectory()
    {
      if (AxoCoverRoot == null)
      {
        new InvalidOperationException("No solution loaded.");
      }

      var path = Path.Combine(AxoCoverRoot, _testRunDirectory, "run_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
      Directory.CreateDirectory(path);
      return path;
    }

    public string CreateReportDirectory()
    {
      if (AxoCoverRoot == null)
      {
        new InvalidOperationException("No solution loaded.");
      }

      var path = Path.Combine(AxoCoverRoot, _testReportDirectory, "report_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
      Directory.CreateDirectory(path);
      return path;
    }

    public StorageController(IEditorContext editorContext)
    {
      _editorContext = editorContext;
    }

    public string[] GetOutputDirectories()
    {
      if (AxoCoverRoot == null) return new string[0];

      var directories = new[]
      {
        Path.Combine(AxoCoverRoot, _testRunDirectory),
        Path.Combine(AxoCoverRoot, _testReportDirectory)
      };

      return directories
        .Where(p => Directory.Exists(p))
        .ToArray();
    }

    public async Task<OutputDescription> GetOutputFilesAsync(string directory)
    {
      return await Task.Run(() => GetOutputFiles(directory));
    }

    private OutputDescription GetOutputFiles(string directory)
    {
      var directories = Directory.GetDirectories(directory, "*", SearchOption.AllDirectories);
      var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
      var size = files
        .AsParallel()
        .Select(GetFileSize)
        .Sum() / 1024d / 1024d;

      return new OutputDescription(directories, files, size);
    }

    private static long GetFileSize(string filePath)
    {
      try
      {
        return new FileInfo(filePath).Length;
      }
      catch
      {
        return 0;
      }
    }

    public async Task CleanOutputAsync(OutputDescription testOutput)
    {
      await Task.Run(() => CleanOutput(testOutput));
    }

    public void CleanOutput(OutputDescription testOutput)
    {
      foreach (var file in testOutput.Files)
      {
        try
        {
          if (File.Exists(file))
          {
            File.Delete(file);
          }
        }
        catch
        {
          //Delete failed, skip
        }
      }

      foreach (var directory in testOutput.Directories)
      {
        try
        {
          if (!Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Any())
          {
            Directory.Delete(directory, true);
          }
        }
        catch
        {
          //Delete failed, skip
        }
      }
    }
  }
}
