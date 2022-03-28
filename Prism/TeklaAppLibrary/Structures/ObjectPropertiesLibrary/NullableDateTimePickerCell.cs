using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Tekla.Structures.ObjectPropertiesLibrary
{
	public class NullableDateTimePickerCell : DataGridViewTextBoxCell
	{
		public override object DefaultNewRowValue => DBNull.Value;

		public override Type EditType => typeof(NullableDateTimePickerEditingControl);

		public override Type ValueType => typeof(DBNull);

		public NullableDateTimePickerCell()
		{
			base.Style.Format = "d";
		}

		public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
		{
			base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
			NullableDateTimePickerEditingControl nullableDateTimePickerEditingControl = base.DataGridView.EditingControl as NullableDateTimePickerEditingControl;
			try
			{
				if (base.Value == null)
				{
					nullableDateTimePickerEditingControl.Value = null;
				}
				else
				{
					nullableDateTimePickerEditingControl.Value = ((base.Value is DateTime) ? ((DateTime)base.Value) : DateTime.Now.Date);
				}
			}
			catch (ArgumentException)
			{
				nullableDateTimePickerEditingControl.Value = DateTime.Now.Date;
			}
		}

		public override object ParseFormattedValue(object formattedValue, DataGridViewCellStyle cellStyle, TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter)
		{
			string text = formattedValue as string;
			if (formattedValue != null)
			{
				if (formattedValue is DateTime)
				{
					return formattedValue;
				}
				if (text != null && DateTime.TryParse(text, out var result))
				{
					return result;
				}
				throw new ArgumentException("FormattedValue has wrong isDayOfWeekSchedule", "formattedValue");
			}
			return null;
		}
	}
}
