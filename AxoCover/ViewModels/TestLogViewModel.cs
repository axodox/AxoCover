using AxoCover.Models;
using AxoCover.Models.Events;
using System;
using System.Collections.ObjectModel;

namespace AxoCover.ViewModels
{
  public class TestLogViewModel : ViewModel
  {
    private ITestRunner _testRunner;

    public ObservableCollection<string> Lines { get; private set; }

    public TestLogViewModel(ITestRunner testRunner)
    {
      _testRunner = testRunner;
      Lines = new ObservableCollection<string>();

      _testRunner.TestsStarted += OnTestsStarted;
      _testRunner.TestLogAdded += OnTestLogAdded;
    }

    private void OnTestLogAdded(object sender, TestLogAddedEventArgs e)
    {
      Lines.Add(e.Text);
    }

    private void OnTestsStarted(object sender, EventArgs e)
    {
      Lines.Clear();
    }
  }
}
