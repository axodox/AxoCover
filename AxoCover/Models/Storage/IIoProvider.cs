namespace AxoCover.Models.Storage
{
  public interface IIoProvider
  {
    string GetAbsolutePath(string relativePath);
    string GetRelativePath(string absolutePath);
  }
}
