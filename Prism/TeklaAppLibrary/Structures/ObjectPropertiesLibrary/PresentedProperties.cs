#define DEBUG
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Tekla.Structures.ObjectPropertiesLibrary
{
	public class PresentedProperties : IComparable, INotifyPropertyChanged
	{
		public const string DefaultPropertyName = "New Property";

		public const int DefaultWidth = 100;

		private int decimals = 2;

		private string displayType;

		private bool hidden;

		private string name;

		private string propertyType;

		private string reportPropertyName;

		private string udaPropertyName;

		private int width = 100;

		[XmlAttribute("Decimals")]
		public int Decimals
		{
			get
			{
				return decimals;
			}
			set
			{
				if (value != decimals)
				{
					decimals = value;
					OnPropertyChanged("Decimals");
				}
			}
		}

		[XmlAttribute("Display_type")]
		public string DisplayType
		{
			get
			{
				return displayType;
			}
			set
			{
				if (!(value == displayType))
				{
					displayType = value;
					OnPropertyChanged("DisplayType");
				}
			}
		}

		[XmlAttribute("Hidden")]
		public bool Hidden
		{
			get
			{
				return hidden;
			}
			set
			{
				if (value != hidden)
				{
					hidden = value;
					OnPropertyChanged("Hidden");
				}
			}
		}

		[XmlAttribute("Display_name")]
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				if (!(value == name))
				{
					name = value;
					OnPropertyChanged("Name");
				}
			}
		}

		[XmlAttribute("Property_type")]
		public string PropertyType
		{
			get
			{
				return propertyType;
			}
			set
			{
				if (!(value == propertyType))
				{
					propertyType = value;
					OnPropertyChanged("PropertyType");
				}
			}
		}

		[XmlAttribute("REPORT_property_name")]
		public string ReportPropertyName
		{
			get
			{
				return reportPropertyName;
			}
			set
			{
				if (!(value == reportPropertyName))
				{
					reportPropertyName = value;
					OnPropertyChanged("ReportPropertyName");
				}
			}
		}

		[XmlAttribute("UDA_property_name")]
		public string UdaPropertyName
		{
			get
			{
				return udaPropertyName;
			}
			set
			{
				if (!(value == udaPropertyName))
				{
					udaPropertyName = value;
					OnPropertyChanged("UdaPropertyName");
				}
			}
		}

		[XmlIgnore]
		public bool Visible
		{
			get
			{
				return !hidden;
			}
			set
			{
				if (value != !hidden)
				{
					hidden = !value;
					OnPropertyChanged("Visible");
				}
			}
		}

		[XmlAttribute("Width")]
		public int Width
		{
			get
			{
				return width;
			}
			set
			{
				if (value != width)
				{
					width = value;
					OnPropertyChanged("Width");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public PresentedProperties()
		{
			name = "New Property";
			reportPropertyName = string.Empty;
			udaPropertyName = string.Empty;
			propertyType = "double";
			decimals = 2;
			width = 100;
			hidden = true;
		}

		public PresentedProperties(string displayName, string reportName, string udaName, string type, int decimals, int width, bool hidden)
		{
			name = displayName;
			reportPropertyName = reportName;
			udaPropertyName = udaName;
			propertyType = type;
			this.decimals = decimals;
			this.width = width;
			this.hidden = hidden;
		}

		public PresentedProperties(PresentedProperties propertyToCopy)
		{
			name = propertyToCopy.Name;
			reportPropertyName = propertyToCopy.ReportPropertyName;
			udaPropertyName = propertyToCopy.UdaPropertyName;
			propertyType = propertyToCopy.PropertyType;
			displayType = propertyToCopy.DisplayType;
			decimals = propertyToCopy.Decimals;
			width = propertyToCopy.Width;
			hidden = propertyToCopy.Hidden;
		}

		public void Copy(PresentedProperties propertyToCopy)
		{
			name = propertyToCopy.Name;
			reportPropertyName = propertyToCopy.ReportPropertyName;
			udaPropertyName = propertyToCopy.UdaPropertyName;
			propertyType = propertyToCopy.PropertyType;
			displayType = propertyToCopy.DisplayType;
			decimals = propertyToCopy.Decimals;
			width = propertyToCopy.Width;
			hidden = propertyToCopy.Hidden;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			return Name == ((PresentedProperties)obj).Name;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return $"{name}_{reportPropertyName}_{udaPropertyName}_{propertyType}_{displayType}_{decimals}";
		}

		int IComparable.CompareTo(object first)
		{
			PresentedProperties presentedProperties = (PresentedProperties)first;
			return string.CompareOrdinal(presentedProperties.Name, Name);
		}

		protected virtual void OnPropertyChanged(string name)
		{
			lock (this)
			{
				try
				{
					if (this.PropertyChanged != null)
					{
						this.PropertyChanged(this, new PropertyChangedEventArgs(name));
					}
				}
				catch (InvalidOperationException ex)
				{
					Debug.WriteLine(ex.Message);
				}
			}
		}
	}
}
