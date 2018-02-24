namespace AxoCover.Models.Testing.Data
{
  public class OutputDescription
  {
    public string[] Directories { get; private set; }
    public string[] Files { get; private set; }
    public double Size { get; private set; }
    public OutputDescription(string[] directories, string[] files, double size)
    {
      Directories = directories;
      Files = files;
      Size = size;
    }
  }
}
