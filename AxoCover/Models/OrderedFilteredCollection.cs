using AxoCover.Models.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace AxoCover.Models
{
  public class OrderedFilteredCollection<T> : ObservableCollection<T>
  {
    private readonly ObservableCollection<T> _baseCollection;

    private Predicate<T> _onFilter;

    private readonly Comparison<T> _onCompare;

    public int? ResultLimit { get; set; }

    public OrderedFilteredCollection(ObservableCollection<T> baseCollection, Comparison<T> onCompare)
    {
      _onFilter = p => true;
      _onCompare = onCompare;
      _baseCollection = baseCollection;
      _baseCollection.CollectionChanged += OnCollectionChanged;
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
      {
        foreach (T oldItem in e.OldItems)
        {
          if (_onFilter(oldItem))
          {
            this.Remove(oldItem);
          }
        }
      }

      if (e.NewItems != null)
      {
        foreach (T newItem in e.NewItems)
        {
          if (ResultLimit.HasValue && Count >= ResultLimit) break;

          if (_onFilter(newItem))
          {
            this.OrderedAdd(newItem, _onCompare, ReplacementBehavior.KeepBoth);
          }
        }
      }
    }

    public void ApplyFilter(Predicate<T> onFilter)
    {
      _onFilter = onFilter;
      foreach (var item in this.ToArray())
      {
        if (!onFilter(item))
        {
          Remove(item);
        }
      }

      foreach (var item in _baseCollection)
      {
        if (ResultLimit.HasValue && Count >= ResultLimit) break;

        if (onFilter(item) && !Contains(item))
        {
          this.OrderedAdd(item, _onCompare, ReplacementBehavior.KeepBoth);
        }
      }
    }
  }
}
