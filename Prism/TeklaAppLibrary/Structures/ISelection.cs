using System;
using System.Collections.Generic;

namespace Tekla.Structures
{
	public interface ISelection
	{
		IEnumerable<object> AllObjects { get; }

		IEnumerable<object> SelectedObjects { get; }

		event EventHandler SelectionChanged;
	}
}
