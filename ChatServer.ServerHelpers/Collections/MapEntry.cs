using System;
using System.Collections.Generic;
using System.Text;

namespace ChatServer.ServerHelpers.Collections
{
	public struct MapEntry<T1, T2>
	{
		private T1 val1;
		private T2 val2;

		public T1 Value1 => val1;
		public T2 Value2 => val2;

		public MapEntry(T1 value1, T2 value2)
		{
			val1 = value1;
			val2 = value2;
		}

		public static implicit operator KeyValuePair<T1, T2>(MapEntry<T1, T2> entry)
		{
			return new KeyValuePair<T1, T2>(entry.val1, entry.val2);
		}
		public static implicit operator MapEntry<T1, T2>(KeyValuePair<T1, T2> keyValuePair)
		{
			return new MapEntry<T1, T2>(keyValuePair.Key, keyValuePair.Value);
		}
	}
}
