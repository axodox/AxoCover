using AxoCover.Models.Testing.Data;
using System;
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
        IsSelectedChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    public event EventHandler IsSelectedChanged;

    public TestStateGroupViewModel(TestState state)
    {
      State = state;
      Items = new ObservableCollection<TestItemViewModel>();
      Items.CollectionChanged += OnCollectionChanged;
    }

    private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      NotifyPropertyChanged(nameof(Count));
    }
  }
}
