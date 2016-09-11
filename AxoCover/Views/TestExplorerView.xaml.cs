using AxoCover.ViewModels;
using System.Windows;

namespace AxoCover.Views
{
  /// <summary>
  /// Interaction logic for TestExplorerView.xaml
  /// </summary>
  public partial class TestExplorerView : View<TestExplorerViewModel>
  {
    public TestExplorerView()
    {
      InitializeComponent();
    }

    private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      _viewModel.SelectedItem = e.NewValue as TestItemViewModel;
    }
  }
}
