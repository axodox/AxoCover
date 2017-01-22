using AxoCover.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AxoCover.Views
{
  /// <summary>
  /// Interaction logic for CoverageExplorerView.xaml
  /// </summary>
  public partial class CoverageExplorerView : View<CoverageExplorerViewModel>
  {
    public CoverageExplorerView()
    {
      InitializeComponent();
    }

    private void OnSelectedCoverageItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      if (e.NewValue == null) return;
      ViewModel.SelectedCoverageItem = e.NewValue as CoverageItemViewModel;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
      if (string.IsNullOrEmpty(_searchBox.Text))
      {
        if (ViewModel.SelectedCoverageItem != null)
        {
          ViewModel.SelectedCoverageItem.ExpandParents();
        }
      }
    }

    private void OnResultItemSelected(object sender, RoutedEventArgs e)
    {
      var item = sender as TreeViewItem;
      if (item != null)
      {
        item.BringIntoView();
        e.Handled = true;
      }
    }

    private void OnTreeResultItemMouseDown(object sender, MouseButtonEventArgs e)
    {
      var item = sender as TreeViewItem;
      if (item != null && e.RightButton == MouseButtonState.Pressed)
      {
        item.IsSelected = true;
        e.Handled = true;
      }
    }

    private void OnListResultItemMouseDown(object sender, MouseButtonEventArgs e)
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
