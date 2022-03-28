using System;

namespace Tekla.Structures
{
	public interface IModel : IConnection, ITransaction, IRunMacro, IPicker, ISelection, ISelectObject
	{
		ModelFolder Folder { get; }

		string Name { get; }

		event EventHandler Changed;

		event EventHandler Loaded;

		event EventHandler Numbering;

		event EventHandler Saved;
	}
}
