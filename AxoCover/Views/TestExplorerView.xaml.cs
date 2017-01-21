using AxoCover.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
      ViewModel.SelectedItem = e.NewValue as TestItemViewModel;
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
        if (ViewModel.SelectedItem != null)
        {
          ViewModel.SelectedItem.ExpandParents();
        }
      }
    }

    private void OnTreeTestItemMouseDown(object sender, MouseButtonEventArgs e)
    {
      var item = sender as TreeViewItem;
      if (item != null && e.RightButton == MouseButtonState.Pressed)
      {
        item.IsSelected = true;
        e.Handled = true;
      }
    }

    private void OnListTestItemMouseDown(object sender, MouseButtonEventArgs e)
    {
      var item = sender as ListBoxItem;
      if (item != null && e.RightButton == MouseButtonState.Pressed)
      {
        item.IsSelected = true;
        e.Handled = true;
      }
    }
  }
}
