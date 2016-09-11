using AxoCover.Models;
using AxoCover.Models.Data;
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
    public static DependencyProperty SelectedTestProperty = DependencyProperty.Register(nameof(SelectedTest), typeof(TestItemViewModel), typeof(TestDetailsView), new PropertyMetadata(OnSelectedTestChanged));

    public TestItemViewModel SelectedTest
    {
      get { return (TestItemViewModel)GetValue(SelectedTestProperty); }
      set { SetValue(SelectedTestProperty, value); }
    }

    private static void OnSelectedTestChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var view = d as TestDetailsView;
      view._root.Visibility = (e.NewValue as TestItemViewModel)?.TestItem.Kind == Models.Data.TestItemKind.Method ? Visibility.Visible : Visibility.Collapsed;
    }

    private IEditorContext _editorContext;

    public TestDetailsView()
    {
      _editorContext = ContainerProvider.Container.Resolve<IEditorContext>();

      InitializeComponent();
      _root.Visibility = Visibility.Collapsed;
      _root.DataContext = this;
    }

    private void OnStackItemClick(object sender, RoutedEventArgs e)
    {
      var stackItem = (sender as Control)?.Tag as StackItem;
      if (stackItem != null && stackItem.HasFileReference)
      {
        _editorContext.NavigateToFile(stackItem.SourceFile, stackItem.Line);
      }
    }
  }
}
