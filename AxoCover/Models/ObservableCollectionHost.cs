using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace AxoCover.Models
{
  public class ObservableCollectionHost<T> : IEnumerable<T>, INotifyCollectionChanged
  {
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    private INotifyCollectionChanged _target;
    public INotifyCollectionChanged Target
    {
      get
      {
        return _target;
      }
      set
      {
        if(_target != null)
        {
          _target.CollectionChanged -= OnTargetCollectionChanged;
        }
        _target = value;
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        if (_target != null)
        {
          _target.CollectionChanged += OnTargetCollectionChanged;
        }
      }
    }

    private void OnTargetCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      CollectionChanged?.Invoke(this, e);
    }

    public IEnumerator<T> GetEnumerator()
    {
      return (_target as IEnumerable<T> ?? new T[0]).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}
