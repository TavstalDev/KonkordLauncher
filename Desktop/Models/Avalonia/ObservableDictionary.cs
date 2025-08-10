using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

public class ObservableDictionary<TKey, TValue> :
    IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _dictionary = new();
    public ICollection<TKey> Keys => _dictionary.Keys;
    public ICollection<TValue> Values => _dictionary.Values;
    public int Count => _dictionary.Count;
    public bool IsReadOnly { get; }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableDictionary()
    {
        IsReadOnly = false;
    }

    public ObservableDictionary(bool isReadOnly)
    {
        IsReadOnly = isReadOnly;
    }
    
    public ObservableDictionary(Dictionary<TKey, TValue> dictionary)
    {
        _dictionary = dictionary;
        IsReadOnly = false;
    }

    // Implement IDictionary<TKey, TValue> members
    // and raise CollectionChanged/PropertyChanged events accordingly.
    // For example, in the Add method:
    public void Add(TKey key, TValue value)
    {
        _dictionary.Add(key, value);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value)));
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]"); // Notifies that the indexer property changed
    }

    public bool ContainsKey(TKey key)
    {
        return _dictionary.ContainsKey(key);
    }

    public bool Remove(TKey key)
    {
        if (!_dictionary.Remove(key, out var value))
            return false;

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,  new KeyValuePair<TKey, TValue>(key, value)));
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]"); // Notifies that the indexer property changed
        return true;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    public TValue this[TKey key]
    {
        get => _dictionary[key];
        set
        {
            bool existed = _dictionary.TryGetValue(key, out var oldValue);

            _dictionary[key] = value;

            if (existed)
            {
                // If the key already existed, it's a Replace action
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    new KeyValuePair<TKey, TValue>(key, value),    // New item
#pragma warning disable CS8604 // Possible null reference argument.
                    new KeyValuePair<TKey, TValue>(key, oldValue)  // Old item
#pragma warning restore CS8604 // Possible null reference argument.
                ));
            }
            else
            {
                // If the key didn't exist, it's an Add action
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    new KeyValuePair<TKey, TValue>(key, value)
                ));
                OnPropertyChanged(nameof(Count)); // Count changed if it was an add
            }
            OnPropertyChanged("Item[]"); // Notifies that the indexer property changed
        }
    }

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _dictionary.ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        _dictionary.Add(item.Key, item.Value);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]"); // Notifies that the indexer property changed
    }

    public void Clear()
    {
        _dictionary.Clear();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]"); // Notifies that the indexer property changed
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if (!_dictionary.TryGetValue(item.Key, out var value))
            return false;
        
        if (value == null && item.Value == null)
            return true;
        
        return Equals(value, item.Value);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (!_dictionary.TryGetValue(item.Key, out var value) || !Equals(value, item.Value))
            return false;

        _dictionary.Remove(item.Key);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]"); // Notifies that the indexer property changed
        return true;
    }
}