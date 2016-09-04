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
    private readonly TestExplorerViewModel _viewModel;

    public TestExplorerView()
    {
      InitializeComponent();
      DataContext = _viewModel = ContainerProvider.Container.Resolve<TestExplorerViewModel>();
    }

    private void OnSelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
    {
      _viewModel.SelectedItem = e.NewValue as TestItemViewModel;
    }
  }
}
