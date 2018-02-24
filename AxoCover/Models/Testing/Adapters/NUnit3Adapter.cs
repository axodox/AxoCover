namespace AxoCover.Models.Testing.Adapters
{
  public class NUnit3Adapter : NUnitAdapter
  {
    public override string ExecutorUri => "executor://nunit3testexecutor";

    public NUnit3Adapter() 
      : base(3, "NUnit3.TestAdapter.dll")
    {

    }
  }
}
