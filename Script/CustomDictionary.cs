using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class CustomDictionary<TKey, TValue> : IEnumerable<CustomKeyValuePair<TKey, TValue>>
{
    [SerializeField] List<CustomKeyValuePair<TKey, TValue>> pair = new();
    public TValue this[TKey _key] => Get(_key);

    public int Count => pair.Count;
    public void Add(TKey _key, TValue _value)
    {
        if (ContainsKey(_key))
            return;
        pair.Add(new CustomKeyValuePair<TKey, TValue>() { Key = _key, _Value = _value });
    }

    public void Remove (CustomKeyValuePair<TKey, TValue> _item)
    {
        if (!ContainsKey(_item.Key))
            return;
        pair.Remove(_item);
    }
    public bool ContainsKey(TKey _key)
    {
        for (int i = 0; i < pair.Count; i++)
        {
            if (pair[i].Key.Equals(_key))
                return true;
        }
        return false;
    }

    public TValue Get(TKey _key)
    {
        for (int i = 0; i < pair.Count; i++)
        {
            if (pair[i].Key.Equals(_key))
                return pair[i]._Value;
        }
        return default;
    }
    public CustomKeyValuePair<TKey, TValue> GetPair(TKey _key)
    {
        for (int i = 0; i < pair.Count; i++)
        {
            if (pair[i].Key.Equals(_key))
                return pair[i];
        }
        return default;
    }

    public IEnumerator<CustomKeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (CustomKeyValuePair<TKey, TValue> _pair in pair)
        {
            yield return _pair;
        }
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}


[Serializable]
public struct CustomKeyValuePair<TKey, TValue>
{
    [SerializeField] TKey key;
    [SerializeField] TValue _value;

    public TKey Key { get => key; set => key = value; }
    public TValue _Value { get => _value; set => _value = value; }
}