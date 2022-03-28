using System;
using System.Collections.Generic;

namespace Tekla.Structures
{
	internal class Connection : IConnection, ITransaction, IRunMacro, IPicker, ISelection, ISelectObject
	{
		private readonly ModelConnection model;

		public IEnumerable<object> AllObjects
		{
			get
			{				
				return model.AllObjects;
			}
		}

		public bool IsActive => model.IsActive;

		public ModelConnection Model => model;

		public IEnumerable<object> SelectedObjects
		{
			get
			{
				return model.SelectedObjects;
			}
		}

		public event EventHandler ChangesCommitted
		{
			add
			{
				model.ChangesCommitted += value;
			}
			remove
			{
				model.ChangesCommitted -= value;
			}
		}

		public event EventHandler ChangesDiscarded
		{
			add
			{
				model.ChangesDiscarded += value;
			}
			remove
			{
				model.ChangesDiscarded -= value;
			}
		}

		public event EventHandler SelectionChanged
		{
			add
			{
				model.SelectionChanged += value;
			}
			remove
			{
				model.SelectionChanged -= value;
			}
		}

		public Connection()
		{
			model = new ModelConnection();
		}

		public void CommitChanges(IEnumerable<object> objects)
		{
				model.CommitChanges(objects);
			
		}

		public bool Connect()
		{
			return model.Connect();
		}

		public void DiscardChanges(IEnumerable<object> objects)
		{
				model.DiscardChanges(objects);
			
		}

		public void Disconnect()
		{
			model.Disconnect();
		}

		public object PickObject(string prompt)
		{			
			return model.PickObject(prompt);
		}

		public void RunMacro(string macroName)
		{
				model.RunMacro(macroName);
			
		}

		public object SelectObjectByIdentifier(Identifier identifier)
		{
			return model.SelectObjectByIdentifier(identifier);
		}
	}
}