using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace AxoCover.Models.Toolkit
{
  public class ObservableCollectionHost<T> : IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
  {
    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    private IList<T> _target;
    public IList<T> Target
    {
      get
      {
        return _target;
      }
      set
      {
        if(_target != null)
        {
          (_target as INotifyCollectionChanged).CollectionChanged -= OnTargetCollectionChanged;
          (_target as INotifyPropertyChanged).PropertyChanged -= OnTargetPropertyChanged;
        }
        _target = value;        
        if (_target != null)
        {
          (_target as INotifyCollectionChanged).CollectionChanged += OnTargetCollectionChanged;
          (_target as INotifyPropertyChanged).PropertyChanged += OnTargetPropertyChanged;
        }
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      }
    }

    public int Count => Target?.Count ?? 0;

    public bool IsReadOnly => Target?.IsReadOnly ?? true;

    public T this[int index]
    {
      get => Target[index];
      set => Target[index] = value;
    }

    private void OnTargetCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      CollectionChanged?.Invoke(this, e);
    }

    private void OnTargetPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      PropertyChanged?.Invoke(this, e);
    }

    public IEnumerator<T> GetEnumerator()
    {
      return (Target as IEnumerable<T> ?? new T[0]).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public int IndexOf(T item) => Target?.IndexOf(item) ?? -1;

    public void Insert(int index, T item) => Target.Insert(index, item);

    public void RemoveAt(int index) => Target.RemoveAt(index);

    public void Add(T item) => Target.Add(item);

    public void Clear() => Target.Clear();

    public bool Contains(T item) => Target?.Contains(item) ?? false;

    public void CopyTo(T[] array, int arrayIndex) => Target?.CopyTo(array, arrayIndex);

    public bool Remove(T item) => Target.Remove(item);
  }
}
