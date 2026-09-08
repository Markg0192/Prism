using PrismNewInterface.Infrastructure;
using PrismNewInterface.Models;
using PrismNewInterface.Services;
using System;
using System.Threading.Tasks;

namespace PrismNewInterface.ViewModels
{
	public sealed class PrepareFabsecsViewModel : ObservableObject
	{
		private readonly PrismOperations _prismOperations;

		private OperationState _prepareMaterialState;
		private OperationState _createCarcassState;
		private bool _isBusy;

		public PrepareFabsecsViewModel(PrismOperations prismOperations)
		{
			if (prismOperations == null)
			{
				throw new ArgumentNullException(nameof(prismOperations));
			}

			_prismOperations = prismOperations;

			PrepareMaterialState = OperationState.Ready;
			CreateCarcassState = OperationState.Ready;

			PrepareMaterialCommand = new AsyncRelayCommand(PrepareMaterialMemberAsync, CanRun);

			CreateCarcassCommand = new AsyncRelayCommand(CreateCarcassAsync, CanRun);
		}

		public bool IsBusy
		{
			get { return _isBusy; }
			private set
			{
				if (SetProperty(ref _isBusy, value))
				{
					RefreshCommands();
				}
			}
		}

		public OperationState PrepareMaterialState
		{
			get { return _prepareMaterialState; }
			private set
			{
				if (SetProperty(ref _prepareMaterialState, value))
				{
					OnPropertyChanged(nameof(PrepareMaterialStatus));
				}
			}
		}

		public OperationState CreateCarcassState
		{
			get { return _createCarcassState; }
			private set
			{
				if (SetProperty(ref _createCarcassState, value))
				{
					OnPropertyChanged(nameof(CreateCarcassStatus));
				}
			}
		}

		public string PrepareMaterialStatus
		{
			get { return PrepareMaterialState.ToString(); }
		}

		public string CreateCarcassStatus
		{
			get { return CreateCarcassState.ToString(); }
		}

		public AsyncRelayCommand PrepareMaterialCommand { get; private set; }

		public AsyncRelayCommand CreateCarcassCommand { get; private set; }

		private bool CanRun()
		{
			return !IsBusy;
		}

		private async Task PrepareMaterialMemberAsync()
		{
			IsBusy = true;
			PrepareMaterialState = OperationState.Running;

			try
			{
				OperationResult result = await _prismOperations.PrepareFabsecMaterialAsync();

				PrepareMaterialState = result.Success ? OperationState.Complete : OperationState.Failed;
			}
			catch (Exception ex)
			{
				PrepareMaterialState = OperationState.Failed;
				HandleOperationException("Prepare Fabsec Material", ex);
			}
			finally
			{
				IsBusy = false;
			}
		}

		private async Task CreateCarcassAsync()
		{
			IsBusy = true;
			CreateCarcassState = OperationState.Running;

			try
			{
				OperationResult result = await _prismOperations.CreateFabsecCarcassAsync();

				CreateCarcassState = result.Success ? OperationState.Complete : OperationState.Failed;
			}
			catch (Exception ex)
			{
				CreateCarcassState = OperationState.Failed;
				HandleOperationException("Create Fabsec Carcass", ex);
			}
			finally
			{
				IsBusy = false;
			}
		}

		private void HandleOperationException(string operationName, Exception ex)
		{
			// Add Prism logging here later.
		}

		private void RefreshCommands()
		{
			if (PrepareMaterialCommand != null)
			{
				PrepareMaterialCommand.RaiseCanExecuteChanged();
			}

			if (CreateCarcassCommand != null)
			{
				CreateCarcassCommand.RaiseCanExecuteChanged();
			}
		}
	}

	public enum OperationState
	{
		Ready,
		Running,
		Complete,
		Failed
	}
}