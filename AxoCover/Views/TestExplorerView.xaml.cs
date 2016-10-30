using AxoCover.ViewModels;
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
      if (e.NewValue == null) return;
      _viewModel.SelectedItem = e.NewValue as TestItemViewModel;
    }

    private void OnItemSelected(object sender, RoutedEventArgs e)
    {
      var item = sender as TreeViewItem;
      if (item != null)
      {
        item.BringIntoView();
        e.Handled = true;
      }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
      if (string.IsNullOrEmpty(_searchBox.Text))
      {
        if (_viewModel.SelectedItem != null)
        {
          _viewModel.SelectedItem.ExpandParents();
        }
      }
    }

    private void OnSettingsIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if ((bool)e.NewValue == true)
      {
        _viewModel.TestSettingsFiles.Refresh();
        _viewModel.RefreshProjectSizes();
      }
    }
  }
}
