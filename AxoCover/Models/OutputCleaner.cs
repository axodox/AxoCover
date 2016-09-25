using AxoCover.Models.Data;
using AxoCover.Models.Data.TestReport;
using AxoCover.Models.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public class OutputCleaner : IOutputCleaner
  {
    public async Task<TestOutputDescription> GetOutputFilesAsync(TestProject testProject)
    {
      return await Task.Run(() => GetOutputFiles(testProject));
    }

    private TestOutputDescription GetOutputFiles(TestProject testProject)
    {
      if (testProject == null)
        throw new ArgumentNullException(nameof(testProject));

      var directories = new List<string>();
      var files = new List<string>();
      try
      {
        var trxFiles = Directory.EnumerateFiles(testProject.OutputDirectory, "*.trx", SearchOption.AllDirectories);
        foreach (var trxFile in trxFiles)
        {
          files.Add(trxFile);
          var coverageFile = Path.ChangeExtension(trxFile, ".xml");
          if (File.Exists(coverageFile))
          {
            files.Add(coverageFile);
          }

          try
          {
            var testRun = GenericExtensions.ParseXml<TestRun>(trxFile);
            var deployment = testRun?.TestSettings?.Deployment;
            if (deployment == null) continue;

            var deploymentDirectory = Path.Combine(deployment.UserDeploymentRoot, deployment.RunDeploymentRoot);
            directories.Add(deploymentDirectory);
            if (Directory.Exists(deploymentDirectory))
            {
              files.AddRange(Directory.GetFiles(deploymentDirectory, "*", SearchOption.AllDirectories));
            }
          }
          catch
          {
            //File enumeration failed, skip
          }
        }
      }
      catch
      {
        //File enumeration failed, skip
      }

      var size = files
        .AsParallel()
        .Select(GetFileSize)
        .Sum() / 1024d / 1024d;

      return new TestOutputDescription(directories.ToArray(), files.ToArray(), size);
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

    public async Task CleanOutputAsync(TestOutputDescription testOutput)
    {
      await Task.Run(() => CleanOutput(testOutput));
    }

    public void CleanOutput(TestOutputDescription testOutput)
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
