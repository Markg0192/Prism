using System;
using System.Collections.Generic;
using Tekla.Structures.Drawing;

namespace Tekla.Structures
{
	public interface IDrawing : IConnection, ITransaction, IRunMacro, IPicker, ISelection, ISelectObject
	{
		Drawing Current { get; }

		ICollection<Drawing> Drawings { get; }

		bool IsEditorOpen { get; }

		event EventHandler DrawingLoaded;

		event EventHandler EditorClosed;

		event EventHandler EditorOpened;

		bool Close();

		bool Close(bool saveBeforeClosing);

		bool Open(Drawing drawing);

		bool Save();
	}
}
