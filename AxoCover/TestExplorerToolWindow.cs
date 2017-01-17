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
      base.Content = new TestExplorerView();
      Caption = "AxoCover";
    }
  }
}
