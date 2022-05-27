using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tekla.Structures.ObjectPropertiesLibrary
{
	public class SearchableSortableBindingList<T> : BindingList<T>, IBindingList, IList, ICollection, IEnumerable
	{
		private bool isSorted;

		private ListSortDirection sortDirection;

		private PropertyDescriptor sortProperty;

		protected override bool IsSortedCore => isSorted;

		protected override ListSortDirection SortDirectionCore => sortDirection;

		protected override PropertyDescriptor SortPropertyCore => sortProperty;

		protected override bool SupportsSearchingCore => true;

		protected override bool SupportsSortingCore => true;

		public void Load(string filename)
		{
			ClearItems();
			if (File.Exists(filename))
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				using FileStream serializationStream = new FileStream(filename, FileMode.Open);
				((List<T>)base.Items).AddRange((IEnumerable<T>)binaryFormatter.Deserialize(serializationStream));
			}
			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		public void Save(string filename)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using FileStream serializationStream = new FileStream(filename, FileMode.Create);
			binaryFormatter.Serialize(serializationStream, (List<T>)base.Items);
		}

		public void Sort(PropertyDescriptor property, ListSortDirection direction)
		{
			ApplySortCore(property, direction);
		}

		protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
		{
			List<T> list = base.Items as List<T>;
			if (list != null)
			{
				PropertyComparer<T> comparer = new PropertyComparer<T>(property, direction);
				list.Sort(comparer);
				isSorted = true;
			}
			else
			{
				isSorted = false;
			}
			sortProperty = property;
			sortDirection = direction;
			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		protected override int FindCore(PropertyDescriptor property, object key)
		{
			if (property == null)
			{
				return -1;
			}
			List<T> list = base.Items as List<T>;
			foreach (T item in list)
			{
				string text = (string)property.GetValue(item);
				if ((string)key == text)
				{
					return IndexOf(item);
				}
			}
			return -1;
		}

		protected override void RemoveSortCore()
		{
			isSorted = false;
			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}
	}
}
