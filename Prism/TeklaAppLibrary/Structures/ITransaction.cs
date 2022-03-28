using System;
using System.Collections.Generic;

namespace Tekla.Structures
{
	public interface ITransaction
	{
		event EventHandler ChangesCommitted;

		event EventHandler ChangesDiscarded;

		void CommitChanges(IEnumerable<object> objects);

		void DiscardChanges(IEnumerable<object> objects);
	}
}
