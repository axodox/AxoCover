namespace AxoCover.Models.Data
{
  public interface IReferenceCounter
  {
    int this[string key] { get; }

    int Decrease(string key);
    int Increase(string key);
  }
}