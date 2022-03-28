using System;
using System.Collections.Generic;

namespace Tekla.Structures
{
	internal class Connection : IConnection, ITransaction, IRunMacro, IPicker, ISelection, ISelectObject
	{
		private readonly DrawingConnection drawing;

		private readonly ModelConnection model;

		public IEnumerable<object> AllObjects
		{
			get
			{
				if (drawing.IsEditorOpen)
				{
					return drawing.AllObjects;
				}
				return model.AllObjects;
			}
		}

		public DrawingConnection Drawing => drawing;

		public bool IsActive => model.IsActive && drawing.IsActive;

		public ModelConnection Model => model;

		public IEnumerable<object> SelectedObjects
		{
			get
			{
				if (drawing.IsEditorOpen)
				{
					return drawing.SelectedObjects;
				}
				return model.SelectedObjects;
			}
		}

		public event EventHandler ChangesCommitted
		{
			add
			{
				model.ChangesCommitted += value;
				drawing.ChangesCommitted += value;
			}
			remove
			{
				model.ChangesCommitted -= value;
				drawing.ChangesCommitted -= value;
			}
		}

		public event EventHandler ChangesDiscarded
		{
			add
			{
				model.ChangesDiscarded += value;
				drawing.ChangesDiscarded += value;
			}
			remove
			{
				model.ChangesDiscarded -= value;
				drawing.ChangesDiscarded -= value;
			}
		}

		public event EventHandler SelectionChanged
		{
			add
			{
				model.SelectionChanged += value;
				drawing.SelectionChanged += value;
			}
			remove
			{
				model.SelectionChanged -= value;
				drawing.SelectionChanged -= value;
			}
		}

		public Connection()
		{
			model = new ModelConnection();
			drawing = new DrawingConnection(model);
		}

		public void CommitChanges(IEnumerable<object> objects)
		{
			if (drawing.IsEditorOpen)
			{
				drawing.CommitChanges(objects);
			}
			else
			{
				model.CommitChanges(objects);
			}
		}

		public bool Connect()
		{
			return model.Connect() && drawing.Connect();
		}

		public void DiscardChanges(IEnumerable<object> objects)
		{
			if (drawing.IsEditorOpen)
			{
				drawing.DiscardChanges(objects);
			}
			else
			{
				model.DiscardChanges(objects);
			}
		}

		public void Disconnect()
		{
			model.Disconnect();
			drawing.Disconnect();
		}

		public object PickObject(string prompt)
		{
			if (drawing.IsEditorOpen)
			{
				return drawing.PickObject(prompt);
			}
			return model.PickObject(prompt);
		}

		public void RunMacro(string macroName)
		{
			if (drawing.IsEditorOpen)
			{
				drawing.RunMacro(macroName);
			}
			else
			{
				model.RunMacro(macroName);
			}
		}

		public object SelectObjectByIdentifier(Identifier identifier)
		{
			return model.SelectObjectByIdentifier(identifier);
		}
	}
}
