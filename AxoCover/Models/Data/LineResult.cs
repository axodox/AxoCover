namespace AxoCover.Models.Data
{
  public class LineResult
  {
    public string TestName { get; set; }

    public string ErrorMessage { get; set; }

    public bool IsPrimary { get; set; }

    public StackItem[] StackTrace { get; set; }
  }
}
