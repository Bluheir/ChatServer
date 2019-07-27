using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ChatServer.ServerHelpers.Collections
{
	public class Map<T1, T2> : 
		IEnumerable<KeyValuePair<T1, T2>>, 
		IEnumerable<MapEntry<T1, T2>>
	{
		private Dictionary<T1, T2> coll1;
		private Dictionary<T2, T1> coll2;

		public T1 this[T2 value]
		{
			get
			{
				return coll2[value];
			}
		}
		public T2 this[T1 value]
		{
			get
			{
				return coll1[value];
			}
		}
		public int Count => coll1.Count;

		public Map() 
		{
			coll1 = new Dictionary<T1, T2>();
			coll2 = new Dictionary<T2, T1>();
		}
		public Map(IEnumerable<KeyValuePair<T1, T2>> keys) : this()
		{
			AddRange(keys);
		}
		public Map(IEnumerable<MapEntry<T1, T2>> keys) : this()
		{
			AddRange(keys);
		}

		public void AddRange(IEnumerable<KeyValuePair<T1, T2>> keys)
		{
			foreach (var item in keys)
			{
				Add(item.Key, item.Value);
			}
		}
		public void AddRange(IEnumerable<MapEntry<T1, T2>> keys)
		{
			foreach(var item in keys)
			{
				Add(item.Value1, item.Value2);
			}
		}
		public bool Add(T1 val1, T2 val2)
		{
			if(Contains(val1))
			{
				return false;
			}
			coll1.Add(val1, val2);
			coll2.Add(val2, val1);
			return true;
		}
		public bool Contains(T1 Key)
		{
			return coll1.ContainsKey(Key);
		}
		public bool Contains(T2 Key)
		{
			return coll2.ContainsKey(Key);
		}
		public MapEntry<T1, T2> GetOrAdd(T1 val1, T2 val2)
		{
			if(Contains(val1) || Contains(val2))
			{
				return new MapEntry<T1, T2>(val1, val2);
			}
			coll1.Add(val1, val2);
			coll2.Add(val2, val1);
			return new MapEntry<T1, T2>(val1, val2);
		}
		public bool TryAdd(T1 key, T2 value)
		{
			if(Contains(key))
			{
				return false;
			}
			coll1.Add(key, value);
			coll2.Add(value, key);
			return true;
		}
		public bool Remove(T1 val1)
		{
			if(!Contains(val1))
			{
				return false;
			}
			coll2.Remove(coll1[val1]);
			coll1.Remove(val1);
			return true;
		}
		public bool Remove(T2 val2)
		{
			if(!Contains(val2))
			{
				return false;
			}
			coll1.Remove(coll2[val2]);
			coll2.Remove(val2);
			return true;
		}

		public T2 Forward(T1 value)
		{
			return coll1[value];
		}
		public T1 Reverse(T2 value)
		{
			return coll2[value];
		}

		public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
		{
			return coll1.GetEnumerator();
		}
		IEnumerator<MapEntry<T1, T2>> IEnumerable<MapEntry<T1, T2>>.GetEnumerator()
		{
			foreach(var item in coll1)
			{
				yield return new MapEntry<T1, T2>(item.Key, item.Value);
			}
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
