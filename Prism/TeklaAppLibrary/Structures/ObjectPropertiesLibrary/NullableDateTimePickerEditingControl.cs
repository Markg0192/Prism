using System;
using System.Globalization;
using System.Windows.Forms;

namespace Tekla.Structures.ObjectPropertiesLibrary
{
	internal class NullableDateTimePickerEditingControl : NullableDateTimePicker, IDataGridViewEditingControl
	{
		private bool valueChanged;

		public DataGridView EditingControlDataGridView { get; set; }

		public object EditingControlFormattedValue
		{
			get
			{
				if (base.Value == null)
				{
					return null;
				}
				return base.Value;
			}
			set
			{
				string text = value as string;
				if (text != null)
				{
					if (text.Length > 0)
					{
						base.Value = DateTime.Parse(text);
					}
					else
					{
						base.Value = null;
					}
				}
			}
		}

		public int EditingControlRowIndex { get; set; }

		public bool EditingControlValueChanged
		{
			get
			{
				return valueChanged;
			}
			set
			{
				valueChanged = value;
			}
		}

		public Cursor EditingPanelCursor => base.Cursor;

		public bool RepositionEditingControlOnValueChange => false;

		public NullableDateTimePickerEditingControl()
		{
			base.Format = DateTimePickerFormat.Custom;
			base.CustomFormat = $"{CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern}";
		}

		public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
		{
			Font = dataGridViewCellStyle.Font;
			base.CalendarForeColor = dataGridViewCellStyle.ForeColor;
			base.CalendarMonthBackground = dataGridViewCellStyle.BackColor;
		}

		public bool EditingControlWantsInputKey(Keys key, bool dataGridViewWantsInputKey)
		{
			Keys keys = key & Keys.KeyCode;
			Keys keys2 = keys;
			if ((uint)(keys2 - 33) <= 7u)
			{
				return true;
			}
			return false;
		}

		public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
		{
			return EditingControlFormattedValue;
		}

		public void PrepareEditingControlForEdit(bool selectAll)
		{
		}

		protected override void OnValueChanged(EventArgs e)
		{
			valueChanged = true;
			EditingControlDataGridView.NotifyCurrentCellDirty(dirty: true);
			base.OnValueChanged(e);
		}
	}
}
