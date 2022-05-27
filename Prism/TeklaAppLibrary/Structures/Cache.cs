using System.Collections.Generic;

namespace Tekla.Structures
{
	public static class Cache<TItem>
	{
		private static class Container<TKey>
		{
			private static readonly Dictionary<TKey, TItem> Cache = new Dictionary<TKey, TItem>();

			public static void Add(TKey key, TItem item)
			{
				Cache.Add(key, item);
			}

			public static bool ContainsKey(TKey key)
			{
				return Cache.ContainsKey(key);
			}

			public static TItem GetItem(TKey key)
			{
				return Cache[key];
			}

			public static TItem GetItemOrDefault(TKey key)
			{
				if (Cache.TryGetValue(key, out var value))
				{
					return value;
				}
				return default(TItem);
			}

			public static TItem GetItemOrDefault(TKey key, TItem defaultItem)
			{
				if (Cache.TryGetValue(key, out var value))
				{
					return value;
				}
				return defaultItem;
			}

			public static bool TryGetItem(TKey key, out TItem item)
			{
				return Cache.TryGetValue(key, out item);
			}
		}

		public static void Add<TKey>(TKey key, TItem item)
		{
			Container<TKey>.Add(key, item);
		}

		public static bool ContainsKey<TKey>(TKey key)
		{
			return Container<TKey>.ContainsKey(key);
		}

		public static TItem GetItem<TKey>(TKey key)
		{
			return Container<TKey>.GetItem(key);
		}

		public static TItem GetItemOrDefault<TKey>(TKey key)
		{
			return Container<TKey>.GetItemOrDefault(key);
		}

		public static TItem GetItemOrDefault<TKey>(TKey key, TItem defaultItem)
		{
			return Container<TKey>.GetItemOrDefault(key, defaultItem);
		}

		public static bool TryGetItem<TKey>(TKey key, out TItem item)
		{
			return Container<TKey>.TryGetItem(key, out item);
		}
	}
}
