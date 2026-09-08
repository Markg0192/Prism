using PrismNewInterface.Infrastructure;
using PrismNewInterface.Models;
using PrismNewInterface.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PrismNewInterface.ViewModels
{
	public sealed class MaterialOrderViewModel : WorkflowViewModelBase
	{
		private readonly PrismOperations _prismOperations;
		private readonly Action<OperationProgress> _reportProgress;

		private string _startNumber;
		private string _selectedOrderAction;
		private string _selectedOrderType;
		private string _phaseNumber;
		private string _issueNumber;
		private string _selectedVariationType;
		private string _variationNumber;
		private DateTime? _requiredBy;
		private string _checkOneHeader = "Check 1";
		private string _checkTwoHeader = "Check 2";
		private string _checkThreeHeader = "Check 3";
		public string CheckOneHeader
		{
			get { return _checkOneHeader; }
			private set { SetProperty(ref _checkOneHeader, value); }
		}

		public void AutoCompleteApplied()
		{
			ValidationStatus = "Recheck required";
			ValidationSummary = "Auto-Complete applied. Re-run the check to verify the changes.";
		}

		public string CheckTwoHeader
		{
			get { return _checkTwoHeader; }
			private set { SetProperty(ref _checkTwoHeader, value); }
		}

		public string CheckThreeHeader
		{
			get { return _checkThreeHeader; }
			private set { SetProperty(ref _checkThreeHeader, value); }
		}

		public MaterialOrderViewModel(PrismOperations prismOperations, Action<OperationProgress> reportProgress)
		{
			if (prismOperations == null)
			{
				throw new ArgumentNullException(nameof(prismOperations));
			}

			_prismOperations = prismOperations;
			_reportProgress = reportProgress;

			OrderActions = new ObservableCollection<string> { "Order", "Add", "Omit" };

			OrderTypes = new ObservableCollection<string> { "Material", "Special Fittings", "Bolts", "Seversafe", "Fabsec Carcasses" };

			VariationTypes = new ObservableCollection<string> { "Select variation", "V.O.", "S.O.", "D.O.V." };

			CheckSelectionCommand = new AsyncRelayCommand(CheckSelectionAsync, CanCheckSelection);

			CreateOrderCommand = new AsyncRelayCommand(CreateOrderAsync, CanCreateOrder);

			ClearCommand = new RelayCommand(Clear);

			PrepareFabsecsCommand = new RelayCommand(ExecutePrepareFabsecs);

			SelectedOrderAction = OrderActions[0];
			SelectedOrderType = OrderTypes[0];
			SelectedVariationType = VariationTypes[0];
		}

		public ObservableCollection<string> OrderActions
		{
			get;
			private set;
		}

		public ObservableCollection<string> OrderTypes
		{
			get;
			private set;
		}

		public ObservableCollection<string> VariationTypes
		{
			get;
			private set;
		}

		public string StartNumber
		{
			get { return _startNumber; }
			set
			{
				if (SetProperty(ref _startNumber, value))
				{
					CheckSelectionCommand.RaiseCanExecuteChanged();
					CreateOrderCommand.RaiseCanExecuteChanged();

					OnPropertyChanged(nameof(StartNumberWarningVisible));
				}
			}
		}

		public bool StartNumberWarningVisible
		{
			get
			{
				return string.IsNullOrWhiteSpace(StartNumber);
			}
		}

		public string SelectedOrderAction
		{
			get
			{
				return _selectedOrderAction;
			}
			set
			{
				if (SetProperty(
					ref _selectedOrderAction,
					value))
				{
					if (CreateOrderCommand != null)
					{
						CreateOrderCommand
							.RaiseCanExecuteChanged();
					}
				}
			}
		}

		public string SelectedOrderType
		{
			get
			{
				return _selectedOrderType;
			}
			set
			{
				if (SetProperty(
					ref _selectedOrderType,
					value))
				{
					if (CreateOrderCommand != null)
					{
						CreateOrderCommand
							.RaiseCanExecuteChanged();
					}
				}
			}
		}

		public string PhaseNumber
		{
			get
			{
				return _phaseNumber;
			}
			set
			{
				if (SetProperty(
					ref _phaseNumber,
					value))
				{
					if (CreateOrderCommand != null)
					{
						CreateOrderCommand
							.RaiseCanExecuteChanged();
					}
				}
			}
		}

		public string IssueNumber
		{
			get
			{
				return _issueNumber;
			}
			set
			{
				if (SetProperty(
					ref _issueNumber,
					value))
				{
					if (CreateOrderCommand != null)
					{
						CreateOrderCommand
							.RaiseCanExecuteChanged();
					}
				}
			}
		}

		public string SelectedVariationType
		{
			get
			{
				return _selectedVariationType;
			}
			set
			{
				SetProperty(
					ref _selectedVariationType,
					value);
			}
		}

		public string VariationNumber
		{
			get
			{
				return _variationNumber;
			}
			set
			{
				SetProperty(
					ref _variationNumber,
					value);
			}
		}

		public DateTime? RequiredBy
		{
			get
			{
				return _requiredBy;
			}
			set
			{
				SetProperty(
					ref _requiredBy,
					value);
			}
		}

		public AsyncRelayCommand CheckSelectionCommand
		{
			get;
			private set;
		}

		public AsyncRelayCommand CreateOrderCommand
		{
			get;
			private set;
		}

		public RelayCommand ClearCommand
		{
			get;
			private set;
		}

		public RelayCommand PrepareFabsecsCommand
		{
			get;
			private set;
		}

		private bool CanCheckSelection()
		{
			return !IsBusy &&
				   !string.IsNullOrWhiteSpace(StartNumber);
		}

		private bool CanCreateOrder()
		{
			return !IsBusy
				&& !string.IsNullOrWhiteSpace(
					PhaseNumber)
				&& !string.IsNullOrWhiteSpace(
					IssueNumber)
				&& IssueNumber.Length >= 2
				&& !string.IsNullOrWhiteSpace(
					SelectedOrderType)
				&& SelectedOrderType != "Select type";
		}

		private async Task CheckSelectionAsync()
		{
			BeginValidation();

			RefreshCommands();

			Progress<OperationProgress> progress = CreateProgress();

			OperationResult result = await _prismOperations.CheckMaterialSelectionAsync(StartNumber, progress);

			UpdateValidationHeaders(result);

			ApplyValidationResult(result);

			OnPropertyChanged();

			RefreshCommands();
		}

		private async Task CreateOrderAsync()
		{
			IsBusy = true;
			RefreshCommands();

			try
			{
				MaterialOrderRequest request = new MaterialOrderRequest
				{
					OrderAction = SelectedOrderAction,
					OrderType = SelectedOrderType,
					PhaseNumber = PhaseNumber,
					IssueNumber = IssueNumber,
					VariationType = SelectedVariationType,
					VariationNumber = VariationNumber,
					RequiredBy = RequiredBy
				};

				OperationResult result = await _prismOperations.CreateMaterialOrderAsync(request, CreateProgress());

				if (!result.Success && result.ValidationResults != null && result.ValidationResults.Count > 0)
				{
					UpdateValidationHeaders(result);
					ApplyValidationResult(result);

					OnPropertyChanged();
					RefreshCommands();

					return;
				}

				if (!result.Success)
				{
					ValidationSummary = result.Message;
					return;
				}

				ValidationResults.Clear();

				ValidationState = ValidationState.Passed;
				ValidationStatus = "Order created";
				ValidationSummary = result.Message;
				ResultCount = "No failed parts";
			}
			finally
			{
				IsBusy = false;
				RefreshCommands();
			}
		}

		private void ExecutePrepareFabsecs()
		{
			/*
             * Opening the Prepare Fabsecs window remains a view concern.
             * We will connect this when the Material Order tab is moved
             * fully onto MVVM commands.
             */
		}

		private void Clear()
		{
			StartNumber = string.Empty;
			PhaseNumber = string.Empty;
			IssueNumber = string.Empty;
			VariationNumber = string.Empty;
			RequiredBy = null;

			SelectedOrderAction = OrderActions[0];

			SelectedOrderType = OrderTypes[0];

			SelectedVariationType = VariationTypes[0];

			ResetValidation();

			OnPropertyChanged();

			RefreshCommands();
		}

		private Progress<OperationProgress> CreateProgress()
		{
			return new Progress<OperationProgress>(
				delegate (OperationProgress progress)
				{
					if (_reportProgress != null)
					{
						_reportProgress(progress);
					}
				});
		}

		private void RefreshCommands()
		{
			if (CheckSelectionCommand != null)
			{
				CheckSelectionCommand.RaiseCanExecuteChanged();
			}

			if (CreateOrderCommand != null)
			{
				CreateOrderCommand.RaiseCanExecuteChanged();
			}
		}

		private void UpdateValidationHeaders(OperationResult result)
		{
			if (result.ValidationResults == null || result.ValidationResults.Count == 0)
			{
				return;
			}

			ValidationResult firstResult = result.ValidationResults[0];

			if (firstResult.Checks == null)
			{
				return;
			}

			if (firstResult.Checks.Count > 0)
			{
				CheckOneHeader = firstResult.Checks[0].CheckName;
			}

			if (firstResult.Checks.Count > 1)
			{
				CheckTwoHeader = firstResult.Checks[1].CheckName;
			}

			if (firstResult.Checks.Count > 2)
			{
				CheckThreeHeader = firstResult.Checks[2].CheckName;
			}
		}
	}
}