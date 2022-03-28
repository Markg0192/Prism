using System;
using System.Windows.Forms;

namespace Tekla.Structures.UI
{
	public class ExpandableStringComboBox : ComboBox
	{
		public delegate void AddNewStringHandler(object sender, ExpandableStringComboBoxEvent Event);

		public delegate void GetNewStringHandler(object sender, ExpandableStringComboBoxEvent Event);

		private enum Status
		{
			Unknown,
			Old,
			New,
			Invalid
		}

		public class ExpandableStringComboBoxEvent : EventArgs
		{
			public string Input { get; set; }

			public string Result { get; set; }

			public ExpandableStringComboBoxEvent(string strInput)
			{
				Input = strInput;
				Result = null;
			}
		}

		private int previousIndex;

		private Status state;

		public string ListTerminator { get; set; }

		public event AddNewStringHandler AddNewStringEvent;

		public event GetNewStringHandler GetNewStringEvent;

		public ExpandableStringComboBox()
		{
			base.DropDownStyle = ComboBoxStyle.DropDown;
			base.SelectionChangeCommitted += SelectionChangeCommittedHandler;
			base.TextUpdate += TextUpdateHandler;
			base.Leave += LeaveHandler;
			state = Status.Unknown;
			AddNewStringEvent += AddToListEvent;
			ListTerminator = "...";
			previousIndex = -1;
		}

		public void Initialize(string[] data, string currentValue = null)
		{
			base.Items.Clear();
			base.Items.AddRange(data);
			if (ListTerminator != null)
			{
				base.Items.Add(ListTerminator);
			}
			if (!string.IsNullOrEmpty(currentValue))
			{
				if (!base.Items.Contains(currentValue))
				{
					AddNewString(currentValue);
				}
				previousIndex = base.Items.IndexOf(currentValue);
				Text = currentValue;
				state = Status.Old;
			}
			else
			{
				previousIndex = -1;
				state = Status.Unknown;
			}
		}

		public void Select(string currentValue)
		{
			if (ListTerminator != null && !base.Items.Contains(ListTerminator))
			{
				base.Items.Add(ListTerminator);
			}
			if (!base.Items.Contains(currentValue))
			{
				AddNewString(currentValue);
			}
			previousIndex = base.Items.IndexOf(currentValue);
			Text = currentValue;
			state = Status.Old;
		}

		private static void AddToListEvent(object sender, ExpandableStringComboBoxEvent expandableStringComboBoxEvent)
		{
			ExpandableStringComboBox expandableStringComboBox = (ExpandableStringComboBox)sender;
			if (expandableStringComboBox.ListTerminator != null)
			{
				expandableStringComboBox.Items.Insert(expandableStringComboBox.Items.Count - 1, expandableStringComboBoxEvent.Input);
			}
			else
			{
				expandableStringComboBox.Items.Add(expandableStringComboBoxEvent.Input);
			}
		}

		private void AddNewString(string newString)
		{
			this.AddNewStringEvent(this, new ExpandableStringComboBoxEvent(newString));
		}

		private void LeaveHandler(object sender, EventArgs e)
		{
			if (state == Status.New)
			{
				AddNewString(Text);
				SelectedIndex = (previousIndex = base.Items.IndexOf(Text));
			}
			else if (state == Status.Invalid)
			{
				base.SelectedItem = ((-1 != previousIndex) ? base.Items[previousIndex] : null);
			}
			state = Status.Old;
		}

		private void SelectionChangeCommittedHandler(object sender, EventArgs eventArgs)
		{
			if (ListTerminator != null && ListTerminator.Equals(base.SelectedItem))
			{
				string strInput = ((-1 != previousIndex) ? (base.Items[previousIndex] as string) : string.Empty);
				string text = null;
				if (this.GetNewStringEvent != null)
				{
					ExpandableStringComboBoxEvent expandableStringComboBoxEvent = new ExpandableStringComboBoxEvent(strInput);
					this.GetNewStringEvent(this, expandableStringComboBoxEvent);
					text = expandableStringComboBoxEvent.Result;
				}
				if (text != null)
				{
					if (!base.Items.Contains(text))
					{
						AddNewString(text);
					}
					SelectedIndex = (previousIndex = base.Items.IndexOf(text));
					Text = text;
				}
				else
				{
					SelectedIndex = previousIndex;
				}
			}
			else
			{
				previousIndex = SelectedIndex;
			}
		}

		private void TextUpdateHandler(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(Text))
			{
				state = (base.Items.Contains(Text) ? Status.Old : Status.New);
			}
			else
			{
				state = Status.Invalid;
			}
		}
	}
}
