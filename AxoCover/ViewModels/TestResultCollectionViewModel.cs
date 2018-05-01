using AxoCover.Models.Testing.Data;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

namespace AxoCover.ViewModels
{
  public class TestResultCollectionViewModel : ViewModel, ITestResult
  {
    public ObservableCollection<TestResult> Results { get; } = new ObservableCollection<TestResult>();

    public TestResultCollectionViewModel()
    {
      Results.CollectionChanged += OnCollectionChanged;
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      //Update selected index if needed
      if (Results.Count == 0)
      {
        SelectedPage = 0;
      }
      else
      {
        if (Results.Count < SelectedPage)
        {
          SelectedPage = Results.Count;
        }

        if (SelectedPage < 1)
        {
          SelectedPage = 1;
        }
      }

      //Notify UI about property changes
      NotifyPropertyChanged(nameof(SelectedResult));
      NotifyPropertyChanged(nameof(PageCount));
      NotifyPropertyChanged(nameof(HasMultiplePages));
      NotifySelectedResultChanged();
    }

    private void NotifySelectedResultChanged()
    {
      NotifyPropertyChanged(nameof(SelectedResult));

      NotifyPropertyChanged(nameof(Duration));
      NotifyPropertyChanged(nameof(ErrorMessage));
      NotifyPropertyChanged(nameof(Method));
      NotifyPropertyChanged(nameof(Outcome));
      NotifyPropertyChanged(nameof(StackTrace));
      NotifyPropertyChanged(nameof(Output));
      NotifyPropertyChanged(nameof(AreHeadersVisible));
      NotifyPropertyChanged(nameof(IconPath));
    }

    private int _selectedPage = 0;
    public int SelectedPage
    {
      get
      {
        return _selectedPage;
      }
      set
      {
        _selectedPage = value;
        NotifyPropertyChanged(nameof(SelectedPage));
        NotifySelectedResultChanged();
      }
    }

    public ICommand NextCommand => new DelegateCommand(
      p => SelectedPage++,
      p => SelectedPage < PageCount,
      p => ExecuteOnPropertyChange(p, nameof(SelectedPage), nameof(PageCount)));

    public ICommand PreviousCommand => new DelegateCommand(
      p => SelectedPage--,
      p => SelectedPage > 1,
      p => ExecuteOnPropertyChange(p, nameof(SelectedPage), nameof(PageCount)));

    public int PageCount => Results.Count;

    public bool HasMultiplePages => PageCount > 1;

    public TestResult SelectedResult
    {
      get
      {
        if (SelectedPage >= 1 && SelectedPage <= Results.Count)
        {
          return Results[SelectedPage - 1];
        }
        else
        {
          return null;
        }
      }
    }

    public TimeSpan Duration => SelectedResult?.Duration ?? TimeSpan.Zero;

    public string ErrorMessage => SelectedResult?.ErrorMessage;

    public TestMethod Method => SelectedResult?.Method;

    public TestState Outcome => SelectedResult?.Outcome ?? TestState.Scheduled;

    public StackItem[] StackTrace => SelectedResult?.StackTrace;

    public string Output => SelectedResult?.Output;

    public bool AreHeadersVisible => 
      !string.IsNullOrWhiteSpace(SelectedResult?.ErrorMessage) &&
      !string.IsNullOrWhiteSpace(SelectedResult?.Output);

    public string IconPath
    {
      get
      {
        if (Outcome != TestState.Unknown)
        {
          return AxoCoverPackage.ResourcesPath + Outcome + ".png";
        }
        else
        {
          return AxoCoverPackage.ResourcesPath + "test.png";
        }
      }
    }
  }
}
