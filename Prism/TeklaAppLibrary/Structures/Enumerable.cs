using System.Collections;
using System.Collections.Generic;

namespace Tekla.Structures
{
	internal static class Enumerable
	{
		public static IEnumerable From(IEnumerator enumerator)
		{
			while (enumerator.MoveNext())
			{
				yield return enumerator.Current;
			}
		}

		public static IEnumerable<T> From<T>(IEnumerator<T> enumerator)
		{
			while (enumerator.MoveNext())
			{
				yield return enumerator.Current;
			}
		}
	}
}
