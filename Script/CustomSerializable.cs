
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Serialize a dictionary in Unity Inspector (valid key : string, int, float, enum)
/// </summary>
[Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TKey, TValue>>
{
	[SerializeField] 
	List<TKey> keys = new();
	[SerializeField] 
	List<TValue> values = new();

	Dictionary<TKey, TValue> dictionary = new();

	public Dictionary<TKey, TValue> ToDictionary() => dictionary;
	public int Count => dictionary.Count;
	public ICollection<TKey> Keys => dictionary.Keys;
	public ICollection<TValue> Values => dictionary.Values;
	public TValue this[TKey _key]
	{
		get => dictionary[_key];
		set
		{
			if (dictionary.ContainsKey(_key))
				dictionary[_key] = value;
			else
			{
				dictionary[_key] = value;
				keys.Add(_key);
				values.Add(value);
			}
		}
	}

	public bool ContainsKey(TKey _key) => dictionary.ContainsKey(_key);
	public bool ContainsValue(TValue _value) => dictionary.ContainsValue(_value);
	public bool TryGetValue(TKey _key, out TValue _value) => dictionary.TryGetValue(_key, out _value);
	public bool TryAdd(TKey _key, TValue _value) => dictionary.TryAdd(_key, _value);
	public int EnsureCapacity(int _capacity) => dictionary.EnsureCapacity(_capacity);
	public void Add(TKey _key, TValue _value) => dictionary.Add(_key, _value);
	public bool Remove(TKey _key) => dictionary.Remove(_key);
	public void Clear() => dictionary.Clear();

	public void OnBeforeSerialize()
	{
		keys.Clear();
		values.Clear();

		foreach (KeyValuePair<TKey, TValue> _pair in dictionary)
		{
			keys.Add(_pair.Key);
			values.Add(_pair.Value);
		}
	}
	public void OnAfterDeserialize()
	{
		dictionary = new Dictionary<TKey, TValue>();

		for (int i = 0; i < keys.Count; i++)
		{
			if (!dictionary.ContainsKey(keys[i]))
				dictionary.Add(keys[i], values[i]);
		}
	}

	public bool CheckValidity()
	{
		for (int i = 0; i < values.Count; i++)
		{
			if (values[i] == null)
				return false;
		}
		return true;
	}
	public override string ToString()
	{
		string _result = "[";
		for (int i = 0; i < keys.Count; i++)
		{
			_result += $"(_key = {keys[i]}, value = {values[i]})";
			if (i < keys.Count - 1)
				_result += ", ";
		}
		_result += "]";
		return _result;
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return dictionary.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}

/// <summary>
/// Serialize a clock time in the Inspector
/// </summary>
[Serializable]
public struct DayTime
{
	[ClampMinMax(0, 23)]
	public int hour;
	[ClampMinMax(0, 59)]
	public int minute;

	public override string ToString()
	{
		string _result = $"{hour:00}h{minute:00}";
		return _result;
	}
}

/// <summary>
/// Serialize a min/max range in the Inspector
/// </summary>
[Serializable]
public struct RangeBetween
{
	public int min;
	public int max;

	public override string ToString()
	{
		string _result = $"Clamp between [{min} - {max}]";
		return _result;
	}
}