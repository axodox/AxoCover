using AxoCover.Views;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

namespace AxoCover
{
  [Guid("da00c8fe-3e71-41c1-8d88-0fab813d8ee8")]
  public class TestExplorerToolWindow : ToolWindowPane
  {
    public TestExplorerToolWindow()
    {
      Content = new TestExplorerView();
      Caption = "AxoCover Explorer";
    }
  }

  [Guid("a38a03c9-0c4f-4548-ade7-b18837709756")]
  public class TestLogToolWindow : ToolWindowPane
  {
    public TestLogToolWindow()
    {
      Content = new TestLogView();
      Caption = "AxoCover Log";
    }
  }
}
