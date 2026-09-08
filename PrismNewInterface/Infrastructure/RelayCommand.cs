using System;
using System.Windows.Input;

namespace PrismNewInterface.Infrastructure
{
	public sealed class RelayCommand : ICommand
	{
		private readonly Action<object> _execute;
		private readonly Predicate<object> _canExecute;

		public RelayCommand(
			Action execute)
		{
			if (execute == null)
			{
				throw new ArgumentNullException(
					nameof(execute));
			}

			_execute =
				delegate
				{
					execute();
				};

			_canExecute = null;
		}

		public RelayCommand(
			Action execute,
			Func<bool> canExecute)
		{
			if (execute == null)
			{
				throw new ArgumentNullException(
					nameof(execute));
			}

			_execute =
				delegate
				{
					execute();
				};

			if (canExecute != null)
			{
				_canExecute =
					delegate
					{
						return canExecute();
					};
			}
		}

		public RelayCommand(
			Action<object> execute)
		{
			if (execute == null)
			{
				throw new ArgumentNullException(
					nameof(execute));
			}

			_execute = execute;
			_canExecute = null;
		}

		public RelayCommand(
			Action<object> execute,
			Predicate<object> canExecute)
		{
			if (execute == null)
			{
				throw new ArgumentNullException(
					nameof(execute));
			}

			_execute = execute;
			_canExecute = canExecute;
		}

		public bool CanExecute(
			object parameter)
		{
			return _canExecute == null
				|| _canExecute(parameter);
		}

		public void Execute(
			object parameter)
		{
			_execute(parameter);
		}

		public event EventHandler CanExecuteChanged;

		public void RaiseCanExecuteChanged()
		{
			EventHandler handler =
				CanExecuteChanged;

			if (handler != null)
			{
				handler(
					this,
					EventArgs.Empty);
			}
		}
	}
}