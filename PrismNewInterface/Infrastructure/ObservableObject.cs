using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PrismNewInterface.Infrastructure
{
	public abstract class ObservableObject : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(
			[CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler =
				PropertyChanged;

			if (handler != null)
			{
				handler(
					this,
					new PropertyChangedEventArgs(propertyName));
			}
		}

		protected bool SetProperty<T>(
			ref T storage,
			T value,
			[CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(
				storage,
				value))
			{
				return false;
			}

			storage = value;

			OnPropertyChanged(propertyName);

			return true;
		}
	}
}