using AxoCover.ViewModels;
using System.Windows;

namespace AxoCover.Views
{
  /// <summary>
  /// Interaction logic for TestDetailsView.xaml
  /// </summary>
  public partial class TestDetailsView : View<TestDetailsViewModel>
  {
    public static DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(TestItemViewModel), typeof(TestDetailsView),
      new PropertyMetadata(OnSelectedItemChanged));

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as TestDetailsView)._viewModel.SelectedItem = e.NewValue as TestItemViewModel;
    }

    public TestItemViewModel SelectedItem
    {
      get { return (TestItemViewModel)GetValue(SelectedItemProperty); }
      set { SetValue(SelectedItemProperty, value); }
    }

    public TestDetailsView()
    {
      InitializeComponent();
    }
  }
}
