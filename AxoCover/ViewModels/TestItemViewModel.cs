using AxoCover.Models;
using AxoCover.Models.Data;
using Microsoft.Practices.Unity;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace AxoCover.ViewModels
{
  public class TestItemViewModel : ViewModel
  {
    public TestItem TestItem { get; private set; }

    public TestItemViewModel Parent { get; private set; }

    public ObservableCollection<TestItemViewModel> Children { get; private set; }

    private TestState _state;
    public TestState State
    {
      get
      {
        return _state;
      }
      set
      {
        _state = value;
        NotifyPropertyChanged(nameof(State));
      }
    }

    public ICommand TestCommand
    {
      get { return new DelagateCommand(p => _testRunner.RunTests(TestItem)); }
    }

    public ITestRunner _testRunner;

    public TestItemViewModel(TestItemViewModel parent, TestItem testItem)
    {
      _testRunner = ContainerProvider.Container.Resolve<ITestRunner>();

      if (testItem == null)
        throw new ArgumentNullException(nameof(testItem));

      TestItem = testItem;
      Parent = parent;
      Children = new ObservableCollection<TestItemViewModel>();
      foreach (var childItem in testItem.Children)
      {
        AddChild(childItem);
      }
    }

    public void UpdateItem(TestItem testItem)
    {
      TestItem = testItem;
      NotifyPropertyChanged(nameof(TestItem));

      var childrenToUpdate = Children.ToList();
      foreach (var childItem in testItem.Children)
      {
        var childToUpdate = childrenToUpdate.FirstOrDefault(p => p.TestItem == childItem);
        if (childToUpdate != null)
        {
          childToUpdate.UpdateItem(childItem);
          childrenToUpdate.Remove(childToUpdate);
        }
        else
        {
          AddChild(childItem);
        }
      }

      foreach (var childToDelete in childrenToUpdate)
      {
        Children.Remove(childToDelete);
      }
    }

    public void ResetState()
    {
      State = TestState.Unknown;

      foreach (var child in Children)
      {
        child.ResetState();
      }
    }

    private void AddChild(TestItem testItem)
    {
      var child = new TestItemViewModel(this, testItem);

      int i;
      for (i = 0; i < Children.Count; i++)
      {
        if (StringComparer.OrdinalIgnoreCase.Compare(Children[i].TestItem.Name, TestItem.Name) > 0)
        {
          break;
        }
      }

      Children.Insert(i, child);
    }
  }
}
