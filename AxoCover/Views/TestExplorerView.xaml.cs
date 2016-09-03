using AxoCover.Models;
using AxoCover.ViewModels;
using Microsoft.Practices.Unity;
using System.Windows.Controls;

namespace AxoCover.Views
{
  /// <summary>
  /// Interaction logic for TestExplorerView.xaml
  /// </summary>
  public partial class TestExplorerView : UserControl
  {
    public TestExplorerView()
    {
      InitializeComponent();
      DataContext = ContainerProvider.Container.Resolve<TestExplorerViewModel>();
    }
  }
}
