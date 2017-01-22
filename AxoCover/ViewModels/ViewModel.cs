using System;
using System.ComponentModel;
using System.Linq;

namespace AxoCover.ViewModels
{
  public abstract class ViewModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public void NotifyPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void ExecuteOnPropertyChange(Action action, params string[] propertyNames)
    {
      PropertyChanged += (o, e) =>
       {
         if (propertyNames.Contains(e.PropertyName))
         {
           action();
         }
       };
    }
  }
}
