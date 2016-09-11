using AxoCover.Models;
using AxoCover.ViewModels;
using Microsoft.Practices.Unity;
using System.Windows;
using System.Windows.Controls;

namespace AxoCover.Views
{
  public abstract class View<T> : UserControl
    where T : ViewModel
  {
    protected readonly T _viewModel;

    public View()
    {
      _viewModel = ContainerProvider.Container?.Resolve<T>();
      Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      (Content as FrameworkElement).DataContext = _viewModel;
    }
  }
}
