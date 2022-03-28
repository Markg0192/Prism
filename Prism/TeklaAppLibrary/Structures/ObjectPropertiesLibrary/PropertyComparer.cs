using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Tekla.Structures.ObjectPropertiesLibrary
{
	public class PropertyComparer<T> : IComparer<T>
	{
		private readonly ListSortDirection direction;

		private readonly PropertyDescriptor property;

		public PropertyComparer(PropertyDescriptor property, ListSortDirection direction)
		{
			this.property = property;
			this.direction = direction;
		}

		public int Compare(T wordX, T wordY)
		{
			object propertvalueY = GetPropertvalueY(wordX, property.Name);
			object propertvalueY2 = GetPropertvalueY(wordY, property.Name);
			if (direction == ListSortDirection.Ascending)
			{
				return CompareAscending(propertvalueY, propertvalueY2);
			}
			return CompareDescending(propertvalueY, propertvalueY2);
		}

		public bool Equals(T wordX, T wordY)
		{
			return wordX.Equals(wordY);
		}

		public int GetHashCode(T obj)
		{
			return obj.GetHashCode();
		}

		private int CompareAscending(object valueX, object valueY)
		{
			if (valueX == null && valueY == null)
			{
				return 0;
			}
			if (valueX == null && valueY != null)
			{
				return -1;
			}
			if (valueX != null && valueY == null)
			{
				return 1;
			}
			return (valueX is IComparable) ? ((IComparable)valueX).CompareTo(valueY) : ((!valueX.Equals(valueY)) ? valueX.ToString().CompareTo(valueY.ToString()) : 0);
		}

		private int CompareDescending(object valueX, object valueY)
		{
			return CompareAscending(valueX, valueY) * -1;
		}

		private object GetPropertvalueY(T value, string property)
		{
			PropertyInfo propertyInfo = value.GetType().GetProperty(property);
			return propertyInfo.GetValue(value, null);
		}
	}
}
