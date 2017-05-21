using AxoCover.Common.Extensions;
using AxoCover.Models.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AxoCover.Models
{
  public class AdapterGuard : IAdapterGuard
  {
    private const string _excludeFileName = "exclude.txt";
    private const string _backupFolderName = ".axoCover/backup";

    private readonly IEditorContext _editorContext;
    private readonly IReferenceCounter _referenceCounter;

    public AdapterGuard(IEditorContext editorContext, IReferenceCounter referenceCounter)
    {
      _editorContext = editorContext;
      _referenceCounter = referenceCounter;
    }

    public void BackupAdapters(string[] adapters, string[] targetFolders)
    {
      try
      {
        _editorContext.WriteToLog($"Backing up conflicting files in {_backupFolderName}...");

        var excludeFilters = adapters
          .Select(p => Path.Combine(Path.GetDirectoryName(p), _excludeFileName))
          .Where(p => File.Exists(p))
          .SelectMany(p => File.ReadAllLines(p))
          .Select(p => "^" + p.Replace(".", "\\.").Replace("*", ".*") + "$")
          .Select(p => new Regex(p, RegexOptions.IgnoreCase))
          .ToArray();

        foreach (var targetFolder in targetFolders.Distinct())
        {
          try
          {
            _editorContext.WriteToLog($"Backing up files from {targetFolder}:");
            _referenceCounter.Increase(targetFolder);

            var fileNames = Directory
              .GetFiles(targetFolder)
              .Select(p => Path.GetFileName(p))
              .Where(p => excludeFilters.Any(q => q.IsMatch(p)))
              .ToArray();

            var backupFolder = Path.Combine(targetFolder, _backupFolderName);
            Directory.CreateDirectory(backupFolder);

            foreach (var fileName in fileNames)
            {
              var targetFile = Path.Combine(targetFolder, fileName);
              var backupFile = Path.Combine(backupFolder, fileName);

              try
              {
                if(File.Exists(backupFile))
                {
                  _editorContext.WriteToLog($"Discarding old backup of {fileName}...");
                  File.Delete(backupFile);
                }
                File.Move(targetFile, backupFile);
                _editorContext.WriteToLog($"Backed up {fileName}.");
              }
              catch (Exception e)
              {
                _editorContext.WriteToLog($"Could not move {fileName}.");
                _editorContext.WriteToLog(e.GetDescription());
              }
            }
          }
          catch (Exception e)
          {
            _editorContext.WriteToLog($"Could not read {targetFolder}.");
            _editorContext.WriteToLog(e.GetDescription());
          }
        }
        _editorContext.WriteToLog($"Backup finished.");
      }
      catch (Exception e)
      {
        _editorContext.WriteToLog($"Backup failed.");
        _editorContext.WriteToLog(e.GetDescription());
      }
    }

    public void RestoreAdapters(string[] targetFolders)
    {
      _editorContext.WriteToLog($"Restoring backed up files from {_backupFolderName}...");

      foreach (var targetFolder in targetFolders.Distinct())
      {
        try
        {
          _editorContext.WriteToLog($"Restoring files to {targetFolder}:");

          if (_referenceCounter.Decrease(targetFolder) > 0)
          {
            _editorContext.WriteToLog($"Skipping. {targetFolder} is still in use.");
            continue;
          }

          var backupFolder = Path.Combine(targetFolder, _backupFolderName);
          Directory.CreateDirectory(backupFolder);

          var backupFiles = Directory
            .GetFiles(backupFolder);

          foreach (var backupFile in backupFiles)
          {
            var fileName = Path.GetFileName(backupFile);
            var targetFile = Path.Combine(targetFolder, fileName);

            try
            {
              if(!File.Exists(targetFile))
              {
                File.Move(backupFile, targetFile);
                _editorContext.WriteToLog($"Restored {fileName}.");
              }              
            }
            catch (Exception e)
            {
              _editorContext.WriteToLog($"Could not move {fileName}.");
              _editorContext.WriteToLog(e.GetDescription());
            }
          }
        }
        catch (Exception e)
        {
          _editorContext.WriteToLog($"Could not read {targetFolder}.");
          _editorContext.WriteToLog(e.GetDescription());
        }
      }
      _editorContext.WriteToLog($"Restore finished.");
    }
  }
}
