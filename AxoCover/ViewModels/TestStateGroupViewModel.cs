using AxoCover.Models.Testing.Data;
using System.Collections.ObjectModel;

namespace AxoCover.ViewModels
{
  public class TestStateGroupViewModel : ViewModel
  {
    public ObservableCollection<TestItemViewModel> Items { get; set; }

    public int Count
    {
      get
      {
        return Items.Count;
      }
    }

    public bool IsVisible
    {
      get
      {
        return Count > 0;
      }
    }

    public TestState State { get; set; }

    public string IconPath
    {
      get
      {
        if (State != TestState.Unknown)
        {
          return AxoCoverPackage.ResourcesPath + State + ".png";
        }
        else
        {
          return AxoCoverPackage.ResourcesPath + "test.png";
        }
      }
    }

    private bool _isSelected;
    public bool IsSelected
    {
      get
      {
        return _isSelected;
      }
      set
      {
        _isSelected = value;
        NotifyPropertyChanged(nameof(IsSelected));

        if (value)
        {
          LineCoverageAdornment.SelectTestNode(SelectedTestItem?.CodeItem);
        }
      }
    }

    private TestItemViewModel _selectedTestItem;
    public TestItemViewModel SelectedTestItem
    {
      get
      {
        return _selectedTestItem;
      }
      set
      {
        _selectedTestItem = value;
        NotifyPropertyChanged(nameof(SelectedTestItem));
        LineCoverageAdornment.SelectTestNode(value?.CodeItem);
      }
    }

    public TestStateGroupViewModel(TestState state)
    {
      State = state;
      Items = new ObservableCollection<TestItemViewModel>();
      Items.CollectionChanged += OnCollectionChanged;
    }

    private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      NotifyPropertyChanged(nameof(Count));
      NotifyPropertyChanged(nameof(IsVisible));
    }
  }
}
