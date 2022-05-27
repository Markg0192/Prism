using System;
using System.Windows.Forms;

namespace Tekla.Structures.ObjectPropertiesLibrary
{
	public class DataGridViewNullableDateTimePickerColumn : DataGridViewColumn
	{
		public override DataGridViewCell CellTemplate
		{
			get
			{
				return base.CellTemplate;
			}
			set
			{
				if (value != null && !value.GetType().IsAssignableFrom(typeof(NullableDateTimePickerCell)))
				{
					throw new InvalidCastException("Must be a NullableDateTimePickerCell");
				}
				base.CellTemplate = value;
			}
		}

		public DataGridViewNullableDateTimePickerColumn()
			: base(new NullableDateTimePickerCell())
		{
		}
	}
}
