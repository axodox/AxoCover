using AxoCover.Models.Data;
using AxoCover.ViewModels;
using System.Windows;

namespace AxoCover.Views
{
  /// <summary>
  /// Interaction logic for SettingsView.xaml
  /// </summary>
  public partial class SettingsView : View<SettingsViewModel>
  {
    public static DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(SettingsView), new PropertyMetadata(OnIsSelectedChanged));

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if ((bool)e.NewValue)
      {
        (d as SettingsView).ViewModel.Refresh();
      }
    }

    public bool IsSelected
    {
      get { return (bool)GetValue(IsSelectedProperty); }
      set { SetValue(IsSelectedProperty, value); }
    }

    public static DependencyProperty SelectedTestSettingsProperty = DependencyProperty.Register(nameof(SelectedTestSettings), typeof(string), typeof(SettingsView));

    public string SelectedTestSettings
    {
      get { return (string)GetValue(SelectedTestSettingsProperty); }
      set { SetValue(SelectedTestSettingsProperty, value); }
    }

    public SettingsView()
    {
      InitializeComponent();
      ViewModel.ExecuteOnPropertyChange(() => SelectedTestSettings = ViewModel.SelectedTestSettings, nameof(SettingsViewModel.SelectedTestSettings));
    }
  }
}
