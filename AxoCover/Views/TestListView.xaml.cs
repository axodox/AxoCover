using AxoCover.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace AxoCover.Views
{
  /// <summary>
  /// Interaction logic for TestListView.xaml
  /// </summary>
  public partial class TestListView : View<TestListViewModel>
  {
    public static DependencyProperty TestSolutionProperty = DependencyProperty.Register(nameof(TestSolution), typeof(TestItemViewModel), typeof(TestListView),
      new PropertyMetadata(OnTestSolutionChanged));

    private static void OnTestSolutionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as TestListView)._viewModel.TestSolution = e.NewValue as TestItemViewModel;
    }

    public TestItemViewModel TestSolution
    {
      get { return (TestItemViewModel)GetValue(TestSolutionProperty); }
      set { SetValue(TestSolutionProperty, value); }
    }

    public static DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(TestItemViewModel), typeof(TestListView),
      new FrameworkPropertyMetadata(OnSelectedItemChanged) { BindsTwoWayByDefault = true });

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as TestListView)._listBox.SelectedItem = e.NewValue as TestItemViewModel;
    }

    public TestItemViewModel SelectedItem
    {
      get { return (TestItemViewModel)GetValue(SelectedItemProperty); }
      set { SetValue(SelectedItemProperty, value); }
    }

    public TestListView()
    {
      InitializeComponent();
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      SelectedItem = _listBox.SelectedItem as TestItemViewModel;
    }
  }
}
