using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace Tekla.Structures.ObjectPropertiesLibrary
{
	public class NullableDateTimePicker : DateTimePicker
	{
		private struct Nmhdr
		{
			public readonly IntPtr WindowHandleFrom;

			public readonly IntPtr IdFrom;

			public readonly int Code;
		}

		private DateTimePickerFormat format = DateTimePickerFormat.Long;

		private string formatAsString;

		private bool isNull;

		private string nullValue;

		public new string CustomFormat { get; set; }

		[Browsable(true)]
		[DefaultValue(DateTimePickerFormat.Long)]
		[TypeConverter(typeof(Enum))]
		public new DateTimePickerFormat Format
		{
			get
			{
				return format;
			}
			set
			{
				format = value;
				if (!isNull)
				{
					SetFormat();
				}
				OnFormatChanged(EventArgs.Empty);
			}
		}

		[Browsable(true)]
		[Category("Behavior")]
		[Description("The string used to display null values in the control")]
		[DefaultValue(" ")]
		public string NullValue
		{
			get
			{
				return nullValue;
			}
			set
			{
				nullValue = value;
			}
		}

		[Bindable(true)]
		[Browsable(false)]
		public new object Value
		{
			get
			{
				if (isNull)
				{
					return null;
				}
				return base.Value;
			}
			set
			{
				if (value == null || value == DBNull.Value)
				{
					SetToNullValue();
					return;
				}
				SetToDateTimeValue();
				base.Value = (DateTime)value;
			}
		}

		private string FormatAsString
		{
			get
			{
				return formatAsString;
			}
			set
			{
				formatAsString = value;
				base.CustomFormat = value;
			}
		}

		public NullableDateTimePicker()
		{
			base.Format = DateTimePickerFormat.Custom;
			NullValue = " ";
			Format = DateTimePickerFormat.Long;
			base.DataBindings.CollectionChanged += DataBindingsCollectionChanged;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (!e.Handled)
			{
				if (e.KeyCode == Keys.Delete)
				{
					Value = null;
					OnValueChanged(EventArgs.Empty);
				}
				base.OnKeyDown(e);
			}
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if (!e.Handled)
			{
				if (e.KeyChar == '\b' || e.KeyChar == '.')
				{
					Value = null;
					OnValueChanged(EventArgs.Empty);
				}
				base.OnKeyPress(e);
			}
		}

		protected override void WndProc(ref Message m)
		{
			if (isNull && m.Msg == 78)
			{
				Nmhdr nmhdr = (Nmhdr)m.GetLParam(typeof(Nmhdr));
				if (nmhdr.Code == -746 || nmhdr.Code == -722)
				{
					SetToDateTimeValue();
				}
			}
			base.WndProc(ref m);
		}

		private void DataBindingsCollectionChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Add)
			{
				base.DataBindings[base.DataBindings.Count - 1].Parse += NullableDateTimePickerParse;
			}
		}

		private void NullableDateTimePickerParse(object sender, ConvertEventArgs e)
		{
			if (isNull)
			{
				e.Value = null;
			}
		}

		private void SetFormat()
		{
			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
			DateTimeFormatInfo dateTimeFormat = currentCulture.DateTimeFormat;
			switch (format)
			{
			case DateTimePickerFormat.Long:
				FormatAsString = dateTimeFormat.LongDatePattern;
				break;
			case DateTimePickerFormat.Short:
				FormatAsString = dateTimeFormat.ShortDatePattern;
				break;
			case DateTimePickerFormat.Time:
				FormatAsString = dateTimeFormat.ShortTimePattern;
				break;
			case DateTimePickerFormat.Custom:
				FormatAsString = CustomFormat;
				break;
			}
		}

		private void SetToDateTimeValue()
		{
			if (isNull)
			{
				SetFormat();
				isNull = false;
				OnValueChanged(new EventArgs());
			}
		}

		private void SetToNullValue()
		{
			isNull = true;
			base.CustomFormat = ((nullValue == null || nullValue.Length == 0) ? " " : ("'" + nullValue + "'"));
		}
	}
}
