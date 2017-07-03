namespace AxoCover.Models
{
  public interface IAdapterGuard
  {
    void BackupAdapters(string[] adapters, string[] targetFolders);
    void RestoreAdapters(string[] targetFolders);
  }
}