using AxoCover.Models;
using AxoCover.ViewModels;
using Microsoft.Practices.Unity;
using System.Windows.Controls;

namespace AxoCover.Views
{
  /// <summary>
  /// Interaction logic for TestLogView.xaml
  /// </summary>
  public partial class TestLogView : UserControl
  {
    public TestLogView()
    {
      InitializeComponent();
      DataContext = ContainerProvider.Container.Resolve<TestLogViewModel>();
    }
  }
}
