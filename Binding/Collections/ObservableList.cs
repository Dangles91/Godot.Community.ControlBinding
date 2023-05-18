using ControlBinding.EventArgs;
using ControlBinding.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ControlBinding.Collections;

public partial class ObservableList<T> : ObservableListBase, IObservableObject, IList<T>, IObservableList
{
    private IList<T> _backingList = new List<T>();

    public int Count => _backingList.Count;

    public bool IsReadOnly => _backingList.IsReadOnly;

    public IList<object> GetBackingList()
    {
        List<object> listCopy = new();
        listCopy.AddRange(_backingList.Cast<object>());

        return listCopy.AsReadOnly();
    }

    T IList<T>.this[int index] { get => _backingList[index]; set => _backingList[index] = value; }
    public T this[int index]
    {
        get => _backingList[index];
        set => _backingList[index] = value;
    }

    public void Add(T item)
    {
        _backingList.Add(item);
        OnPropertyChanged("name"); // TODO: send useful events
        OnObservableListChanged(new ObservableListChangedEventArgs
        {
            ChangedEntries = new List<object> { item },
            ChangeType = ObservableListChangeType.Add,
            Index = _backingList.IndexOf(item)
        });
    }

    public void Clear()
    {
        List<T> copy = new List<T>();
        copy.AddRange(_backingList);

        _backingList.Clear();
        OnPropertyChanged("name");
        OnObservableListChanged(new ObservableListChangedEventArgs
        {
            ChangedEntries = copy.Cast<object>().ToList(),
            ChangeType = ObservableListChangeType.Clear,
            Index = 0
        });
    }

    public bool Contains(T item) => _backingList.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _backingList.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator() => _backingList.GetEnumerator();

    public int IndexOf(T item) => _backingList.IndexOf(item);

    public void Insert(int index, T item)
    {
        _backingList.Insert(index, item);
        OnPropertyChanged("name");
        OnObservableListChanged(new ObservableListChangedEventArgs
        {
            ChangedEntries = new List<object> { item },
            ChangeType = ObservableListChangeType.Insert,
            Index = index
        });
    }

    public void OnPropertyChanged(string name)
    {

        EmitSignal(nameof(PropertyChanged), this);
    }

    public bool Remove(T item)
    {
        var removedIndex = _backingList.IndexOf(item);
        var removed = _backingList.Remove(item);
        if (removed)
        {
            OnPropertyChanged("name");
            OnObservableListChanged(new ObservableListChangedEventArgs
            {
                ChangedEntries = new List<object> { item },
                ChangeType = ObservableListChangeType.Remove,
                Index = removedIndex
            });
        }
        return removed;
    }

    public void RemoveAt(int index)
    {
        if (index >= 0 && index < _backingList.Count - 1)
        {
            var removedItem = _backingList[index];
            _backingList.RemoveAt(index);
            OnPropertyChanged("name");
            OnObservableListChanged(new ObservableListChangedEventArgs
            {
                ChangedEntries = new List<object> { removedItem },
                ChangeType = ObservableListChangeType.Remove,
                Index = index
            });
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => _backingList.GetEnumerator();

    public void OnObservableListChanged(ObservableListChangedEventArgs eventArgs)
    {
        EmitSignal(nameof(ObservableListChanged), eventArgs);
    }

    public void SetViewModelData(object viewModelData)
    {
        throw new System.NotImplementedException();
    }

    public ObservableList()
    {

    }

    public ObservableList(IList<T> list)
    {
        this._backingList = list;
        EmitSignal(nameof(ObservableListChanged), new ObservableListChangedEventArgs
        {
            ChangeType = ObservableListChangeType.Add,
            ChangedEntries = _backingList.Cast<object>().ToList()
        });
    }

    public static implicit operator ObservableList<T>(List<T> list)
    {
        return new ObservableList<T>(list);
    }
}