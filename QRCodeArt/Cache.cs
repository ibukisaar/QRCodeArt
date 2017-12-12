using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public static class Cache<TKey, TValue> {
		static readonly Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

		public static TValue Get(TKey key, Func<TKey, TValue> creator) {
			if (!dict.TryGetValue(key , out var value)) {
				value = creator(key);
				dict.Add(key, value);
			}
			return value;
		}
	}
}
