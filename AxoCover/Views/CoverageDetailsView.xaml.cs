using AxoCover.ViewModels;
using System.Windows;

namespace AxoCover.Views
{
  /// <summary>
  /// Interaction logic for CoverageDetailsView.xaml
  /// </summary>
  public partial class CoverageDetailsView : View<CoverageDetailsViewModel>
  {
    public static DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(CoverageItemViewModel), typeof(CoverageDetailsView),
      new PropertyMetadata(OnSelectedItemChanged));

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as CoverageDetailsView).ViewModel.SelectedItem = e.NewValue as CoverageItemViewModel;
    }

    public CoverageItemViewModel SelectedItem
    {
      get { return (CoverageItemViewModel)GetValue(SelectedItemProperty); }
      set { SetValue(SelectedItemProperty, value); }
    }

    public CoverageDetailsView()
    {
      InitializeComponent();
    }
  }
}
