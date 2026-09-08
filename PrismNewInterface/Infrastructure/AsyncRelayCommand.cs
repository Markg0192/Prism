using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PrismNewInterface.Infrastructure
{
	public sealed class AsyncRelayCommand : ICommand
	{
		private readonly Func<Task> _execute;
		private readonly Func<bool> _canExecute;

		private bool _isExecuting;

		public AsyncRelayCommand(
			Func<Task> execute)
			: this(
				execute,
				null)
		{
		}

		public AsyncRelayCommand(
			Func<Task> execute,
			Func<bool> canExecute)
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
			if (_isExecuting)
			{
				return false;
			}

			return _canExecute == null
				|| _canExecute();
		}

		public async void Execute(
			object parameter)
		{
			if (!CanExecute(parameter))
			{
				return;
			}

			try
			{
				_isExecuting = true;
				RaiseCanExecuteChanged();

				await _execute();
			}
			finally
			{
				_isExecuting = false;
				RaiseCanExecuteChanged();
			}
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