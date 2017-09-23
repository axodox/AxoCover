namespace AxoCover.Models
{
  public interface IIoProvider
  {
    string GetAbsolutePath(string relativePath);
    string GetRelativePath(string absolutePath);
  }
}