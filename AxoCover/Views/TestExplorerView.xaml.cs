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

    private void OnSelectedTestItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      if (e.NewValue == null) return;
      ViewModel.SelectedTestItem = e.NewValue as TestItemViewModel;
    }

    private void OnTestItemSelected(object sender, RoutedEventArgs e)
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
        if (ViewModel.SelectedTestItem != null)
        {
          ViewModel.SelectedTestItem.ExpandParents();
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

    private void OnOpenCoverageReportClick(object sender, RoutedEventArgs e)
    {
      var ofd = new Microsoft.Win32.OpenFileDialog();
      ofd.Filter = AxoCover.Resources.OpenCoverageReportFilter;
      ofd.CheckFileExists = true;

      ((FrameworkElement)sender).Tag = ofd.ShowDialog() == true ? ofd.FileName : null;
    }
  }
}
