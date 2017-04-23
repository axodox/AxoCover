namespace AxoCover.Models.Data
{
  public class LineResult
  {
    public TestMethod TestMethod { get; set; }

    public string ErrorMessage { get; set; }

    public bool IsPrimary { get; set; }

    public StackItem[] StackTrace { get; set; }
  }
}
