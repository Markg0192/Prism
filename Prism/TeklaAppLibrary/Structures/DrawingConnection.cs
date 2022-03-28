#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting;
using Tekla.Structures.Drawing;
using Tekla.Structures.Drawing.UI;

namespace Tekla.Structures
{
	internal class DrawingConnection : IDrawing, IConnection, ITransaction, IRunMacro, IPicker, ISelection, ISelectObject
	{
		private sealed class DrawingCollection : ICollection<Drawing>, IEnumerable<Drawing>, IEnumerable
		{
			private readonly DrawingConnection parent;

			public int Count
			{
				get
				{
					int count = 0;
					try
					{
						if (parent.IsActive)
						{
							SeparateThread.Execute(60000, delegate
							{
								//IL_002e: Unknown result type (might be due to invalid IL or missing references)
								//IL_0034: Expected O, but got Unknown
								DrawingEnumerator drawings = parent.connection.GetDrawings();
								((DrawingEnumeratorBase)drawings).SelectInstances = false;
								foreach (Drawing item in (DrawingEnumeratorBase)drawings)
								{
									Drawing val = item;
									if (val != null)
									{
										count++;
									}
								}
							});
						}
					}
					catch (Exception value)
					{
						Debug.WriteLine(value);
					}
					return count;
				}
			}

			public bool IsReadOnly => true;

			public DrawingCollection(DrawingConnection parent)
			{
				this.parent = parent;
			}

			public void Add(Drawing item)
			{
				throw new NotSupportedException();
			}

			public void Clear()
			{
				throw new NotSupportedException();
			}

			public bool Contains(Drawing item)
			{
				using (IEnumerator<Drawing> enumerator = GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Drawing current = enumerator.Current;
						if (object.Equals(current, item))
						{
							return true;
						}
					}
				}
				return false;
			}

			public void CopyTo(Drawing[] array, int arrayIndex)
			{
				using IEnumerator<Drawing> enumerator = GetEnumerator();
				while (enumerator.MoveNext())
				{
					Drawing current = enumerator.Current;
					array[arrayIndex++] = current;
				}
			}

			public IEnumerator<Drawing> GetEnumerator()
			{
				List<Drawing> snapshot = new List<Drawing>();
				try
				{
					if (parent.IsActive)
					{
						SeparateThread.Execute(60000, delegate
						{
							//IL_0025: Unknown result type (might be due to invalid IL or missing references)
							//IL_002b: Expected O, but got Unknown
							foreach (Drawing item in (DrawingEnumeratorBase)parent.connection.GetDrawings())
							{
								Drawing val = item;
								if (val != null)
								{
									snapshot.Add(val);
								}
							}
						});
					}
				}
				catch (Exception value)
				{
					Debug.WriteLine(value);
					snapshot.Clear();
				}
				return snapshot.GetEnumerator();
			}

			public bool Remove(Drawing item)
			{
				throw new NotSupportedException();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		private const int LongOperation = 60000;

		private readonly ModelConnection model;

		private DrawingHandler connection;

		private Events events;

		private bool eventsRegistered;

		public IEnumerable<object> AllObjects
		{
			get
			{
				List<object> snapshot = new List<object>();
				try
				{
					if (IsEditorOpen)
					{
						SeparateThread.Execute(60000, delegate
						{
							foreach (object item in (DrawingEnumeratorBase)((ViewBase)connection.GetActiveDrawing().GetSheet()).GetAllObjects())
							{
								if (item != null)
								{
									snapshot.Add(item);
								}
							}
						});
					}
				}
				catch (Exception value)
				{
					Debug.WriteLine(value);
					snapshot.Clear();
				}
				return snapshot;
			}
		}

		public Drawing Current
		{
			get
			{
				if (IsActive)
				{
					return SeparateThread.Execute((SeparateThread.Action<Drawing>)connection.GetActiveDrawing);
				}
				return null;
			}
		}

		public ICollection<Drawing> Drawings => new DrawingCollection(this);

		public bool IsActive => connection != null && connection.GetConnectionStatus();

		public bool IsEditorOpen => connection != null && SeparateThread.Execute((SeparateThread.Action<Drawing>)connection.GetActiveDrawing) != null;

		public IEnumerable<object> SelectedObjects
		{
			get
			{
				List<object> snapshot = new List<object>();
				try
				{
					if (IsEditorOpen)
					{
						SeparateThread.Execute(60000, delegate
						{
							foreach (object item in (DrawingEnumeratorBase)connection.GetDrawingObjectSelector().GetSelected())
							{
								if (item != null)
								{
									snapshot.Add(item);
								}
							}
						});
					}
				}
				catch (Exception value)
				{
					Debug.WriteLine(value);
					snapshot.Clear();
				}
				return snapshot;
			}
		}

		public event EventHandler ChangesCommitted;

		public event EventHandler ChangesDiscarded;

		public event EventHandler DrawingLoaded
		{
			add
			{
				UnregisterEvents();
				DrawingLoadedEvent += value;
				RegisterEvents();
			}
			remove
			{
				UnregisterEvents();
				DrawingLoadedEvent -= value;
				RegisterEvents();
			}
		}

		public event EventHandler EditorClosed
		{
			add
			{
				UnregisterEvents();
				EditorClosedEvent += value;
				RegisterEvents();
			}
			remove
			{
				UnregisterEvents();
				EditorClosedEvent -= value;
				RegisterEvents();
			}
		}

		public event EventHandler EditorOpened
		{
			add
			{
				UnregisterEvents();
				EditorOpenedEvent += value;
				RegisterEvents();
			}
			remove
			{
				UnregisterEvents();
				EditorOpenedEvent -= value;
				RegisterEvents();
			}
		}

		public event EventHandler SelectionChanged
		{
			add
			{
				UnregisterEvents();
				SelectionChangedEvent += value;
				RegisterEvents();
			}
			remove
			{
				UnregisterEvents();
				SelectionChangedEvent -= value;
				RegisterEvents();
			}
		}

		private event EventHandler DrawingLoadedEvent;

		private event EventHandler EditorClosedEvent;

		private event EventHandler EditorOpenedEvent;

		private event EventHandler SelectionChangedEvent;

		public DrawingConnection()
		{
		}

		public DrawingConnection(ModelConnection model)
		{
			this.model = model;
		}

		public bool Close()
		{
			if (IsEditorOpen)
			{
				return SeparateThread.Execute((SeparateThread.Action<bool>)connection.CloseActiveDrawing);
			}
			return false;
		}

		public bool Close(bool saveBeforeClosing)
		{
			if (IsEditorOpen)
			{
				return SeparateThread.Execute(() => connection.CloseActiveDrawing(saveBeforeClosing));
			}
			return false;
		}

		public void CommitChanges(IEnumerable<object> objects)
		{
			if (!IsEditorOpen)
			{
				return;
			}
			try
			{
				foreach (object @object in objects)
				{
					DrawingObject val = @object as DrawingObject;
					if (val != null)
					{
						((DatabaseObject)val).Modify();
					}
				}
				if (SeparateThread.Execute(() => connection.GetActiveDrawing().CommitChanges()) && this.ChangesCommitted != null)
				{
					this.ChangesCommitted(this, EventArgs.Empty);
				}
			}
			catch (RemotingException ex)
			{
				Debug.WriteLine(ex.ToString());
				DiscardChanges(objects);
			}
		}

		public bool Connect()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Expected O, but got Unknown
			try
			{
				if (connection == null)
				{
					connection = new DrawingHandler();
					SeparateThread.Execute(delegate
					{
						//IL_0002: Unknown result type (might be due to invalid IL or missing references)
						DrawingHandler.SetMessageExecutionStatus((MessageExecutionModeEnum)1);
					});
				}
				if (events == null)
				{
					events = new Events();
					RegisterEvents();
				}
			}
			catch (Exception value)
			{
				Debug.WriteLine(value);
				Disconnect();
			}
			return IsActive;
		}

		public void DiscardChanges(IEnumerable<object> objects)
		{
			if (this.ChangesDiscarded != null)
			{
				this.ChangesDiscarded(this, EventArgs.Empty);
			}
		}

		public void Disconnect()
		{
			try
			{
				UnregisterEvents();
			}
			catch (Exception value)
			{
				Debug.WriteLine(value);
			}
			finally
			{
				events = null;
				connection = null;
			}
		}

		public bool Open(Drawing drawing)
		{
			if (IsActive)
			{
				return SeparateThread.Execute(() => connection.SetActiveDrawing(drawing, true));
			}
			return false;
		}

		public object PickObject(string prompt)
		{
			if (IsEditorOpen)
			{
				try
				{
					DrawingObject result = default(DrawingObject);
					ViewBase val = default(ViewBase);
					connection.GetPicker().PickObject(prompt, ref result, ref val);
					return result;
				}
				catch (RemotingException ex)
				{
					Debug.WriteLine(ex.ToString());
				}
				catch (PickerInterruptedException)
				{
				}
			}
			return null;
		}

		public void RunMacro(string macroName)
		{
			if (IsEditorOpen && model != null)
			{
				model.RunMacro("..\\drawings\\" + macroName);
			}
		}

		public bool Save()
		{
			if (IsEditorOpen)
			{
				return SeparateThread.Execute((SeparateThread.Action<bool>)connection.SaveActiveDrawing);
			}
			return false;
		}

		public object SelectObjectByIdentifier(Identifier identifier)
		{
			return null;
		}

		private void OnDrawingLoaded()
		{
			this.DrawingLoadedEvent.BeginInvoke(this, EventArgs.Empty, null, null);
		}

		private void OnEditorClosed()
		{
			this.EditorClosedEvent.BeginInvoke(this, EventArgs.Empty, null, null);
		}

		private void OnEditorOpened()
		{
			this.EditorOpenedEvent.BeginInvoke(this, EventArgs.Empty, null, null);
		}

		private void OnSelectionChanged()
		{
			this.SelectionChangedEvent.BeginInvoke(this, EventArgs.Empty, null, null);
		}

		private void RegisterEvents()
		{
			if (events == null || eventsRegistered)
			{
				return;
			}
			SeparateThread.Execute(delegate
			{
				//IL_001e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Expected O, but got Unknown
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				//IL_0053: Expected O, but got Unknown
				//IL_0074: Unknown result type (might be due to invalid IL or missing references)
				//IL_007e: Expected O, but got Unknown
				//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ab: Expected O, but got Unknown
				int num = 0;
				if (this.EditorOpenedEvent != null)
				{
					events.add_DrawingEditorOpened(new DrawingEditorOpenedDelegate(OnEditorOpened));
					num++;
				}
				if (this.EditorClosedEvent != null)
				{
					events.add_DrawingEditorClosed(new DrawingEditorClosedDelegate(OnEditorClosed));
					num++;
				}
				if (this.DrawingLoadedEvent != null)
				{
					events.add_DrawingLoaded(new DrawingLoadedDelegate(OnDrawingLoaded));
					num++;
				}
				if (this.SelectionChangedEvent != null)
				{
					events.add_SelectionChange(new SelectionChangeDelegate(OnSelectionChanged));
					num++;
				}
				if (num > 0)
				{
					events.Register();
					eventsRegistered = true;
				}
			});
		}

		private void UnregisterEvents()
		{
			if (events == null || !eventsRegistered)
			{
				return;
			}
			SeparateThread.Execute(delegate
			{
				//IL_002f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Expected O, but got Unknown
				//IL_0056: Unknown result type (might be due to invalid IL or missing references)
				//IL_0060: Expected O, but got Unknown
				//IL_007d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0087: Expected O, but got Unknown
				//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ae: Expected O, but got Unknown
				eventsRegistered = false;
				events.UnRegister();
				if (this.EditorOpenedEvent != null)
				{
					events.remove_DrawingEditorOpened(new DrawingEditorOpenedDelegate(OnEditorOpened));
				}
				if (this.EditorClosedEvent != null)
				{
					events.remove_DrawingEditorClosed(new DrawingEditorClosedDelegate(OnEditorClosed));
				}
				if (this.DrawingLoadedEvent != null)
				{
					events.remove_DrawingLoaded(new DrawingLoadedDelegate(OnDrawingLoaded));
				}
				if (this.SelectionChangedEvent != null)
				{
					events.remove_SelectionChange(new SelectionChangeDelegate(OnSelectionChanged));
				}
			});
		}
	}
}
