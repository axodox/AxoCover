using AxoCover.ViewModels;
using System.Windows;

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
  }
}
