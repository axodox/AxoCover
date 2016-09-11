using AxoCover.Models;
using AxoCover.ViewModels;
using Microsoft.Practices.Unity;
using System.Windows;
using System.Windows.Controls;

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
