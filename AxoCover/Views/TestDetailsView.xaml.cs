using AxoCover.Models;
using AxoCover.ViewModels;
using Microsoft.Practices.Unity;
using System.Windows;
using System.Windows.Controls;

namespace AxoCover.Views
{
  /// <summary>
  /// Interaction logic for TestDetailsView.xaml
  /// </summary>
  public partial class TestDetailsView : UserControl
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

    private readonly TestDetailsViewModel _viewModel;

    public TestDetailsView()
    {
      InitializeComponent();
      _root.DataContext = _viewModel = ContainerProvider.Container.Resolve<TestDetailsViewModel>();
    }
  }
}
