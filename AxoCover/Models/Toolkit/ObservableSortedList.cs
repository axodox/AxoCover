using AxoCover.Common.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxoCover.Models.Toolkit
{
  public class ObservableSortedList<T> : IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
  {
    private const string _indexerName = "Item[]";
    private const string _countName = nameof(Count);
    private const string _comparisonName = nameof(Comparison);
    private List<T> _items = new List<T>();

    public T this[int index] { get => _items[index]; set => throw new NotSupportedException(); }

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public event PropertyChangedEventHandler PropertyChanged;
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    private Comparison<T> _comparison;
    public Comparison<T> Comparison
    {
      get { return _comparison; }
      set
      {
        _comparison = value;
        Sort();
      }
    }

    public ObservableSortedList(Comparison<T> comparison)
    {
      _comparison = comparison;
    }

    public void Add(T item)
    {
      var index = _items.OrderedAdd(item, Comparison);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_countName));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_indexerName));
      CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    public void Clear()
    {
      _items.Clear();
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_countName));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_indexerName));
      CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public bool Contains(T item)
    {
      return _items.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      _items.CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
      return _items.GetEnumerator();
    }

    public int IndexOf(T item)
    {
      return _items.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
      throw new NotSupportedException();
    }

    public bool Remove(T item)
    {
      var index = IndexOf(item);
      if (index != -1)
      {
        RemoveAt(index);
        return true;
      }
      else
      {
        return false;
      }
    }

    public void RemoveAt(int index)
    {
      var item = _items[index];
      _items.RemoveAt(index);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_countName));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_indexerName));
      CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public void Sort()
    {
      _items.Sort(Comparison);

      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_comparisonName));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_indexerName));
      CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
  }
}
