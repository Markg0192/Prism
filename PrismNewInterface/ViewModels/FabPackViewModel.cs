using PrismNewInterface.Infrastructure;
using PrismNewInterface.Models;
using PrismNewInterface.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PrismNewInterface.ViewModels
{
	public sealed class FabPackViewModel : WorkflowViewModelBase
	{
		private readonly PrismOperations _prismOperations;
		private readonly Action<OperationProgress> _reportProgress;

		private string _phaseNumber;
		private string _issueNumber;
		private string _siteDate;
		private string _selectedVariationType;
		private string _variationNumber;

		public FabPackViewModel(
			PrismOperations prismOperations,
			Action<OperationProgress> reportProgress)
		{
			if (prismOperations == null)
			{
				throw new ArgumentNullException(nameof(prismOperations));
			}

			_prismOperations = prismOperations;
			_reportProgress = reportProgress;
			VariationTypes = new ObservableCollection<string>
			{
				"Select variation",
				"V.O.",
				"S.O.",
				"D.O.V."
			};

			CheckSelectionCommand =
				new AsyncRelayCommand(CheckSelectionAsync, CanCheckSelection);

			CreatePackageCommand =
				new AsyncRelayCommand(CreatePackageAsync, CanCreatePackage);

			ClearCommand =
				new RelayCommand(Clear);

			SelectedVariationType = VariationTypes[0];
		}

		private FabPackRequest CreateRequest()
		{
			return new FabPackRequest
			{
				PhaseNumber = PhaseNumber,
				IssueNumber = IssueNumber,
				SiteDate = SiteDate,
				VariationType = SelectedVariationType,
				VariationNumber = VariationNumber
			};
		}

		public ObservableCollection<string> VariationTypes { get; }

		public AsyncRelayCommand CheckSelectionCommand { get; }

		public AsyncRelayCommand CreatePackageCommand { get; }

		public RelayCommand ClearCommand { get; }

		public string PhaseNumber
		{
			get
			{
				return _phaseNumber;
			}
			set
			{
				if (SetProperty(ref _phaseNumber, value))
				{
					RefreshCommands();
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
				if (SetProperty(ref _issueNumber, value))
				{
					RefreshCommands();
				}
			}
		}

		public string SiteDate
		{
			get
			{
				return _siteDate;
			}
			set
			{
				if (SetProperty(ref _siteDate, value))
				{
					RefreshCommands();
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
				if (SetProperty(ref _selectedVariationType, value))
				{
					RefreshCommands();
				}
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
				if (SetProperty(ref _variationNumber, value))
				{
					RefreshCommands();
				}
			}
		}

		private bool CanCheckSelection()
		{
			return !IsBusy;
		}

		private bool CanCreatePackage()
		{
			return !IsBusy
				&& !string.IsNullOrWhiteSpace(PhaseNumber)
				&& !string.IsNullOrWhiteSpace(IssueNumber)
				&& IssueNumber.Length >= 2;
		}

		private async Task CheckSelectionAsync()
		{
			IsBusy = true;
			BeginValidation();
			RefreshCommands();

			try
			{
				OperationResult result =
					await _prismOperations.CheckFabPackSelectionAsync(
						CreateRequest(),
						CreateProgress());

				ApplyValidationResult(result);
			}
			finally
			{
				IsBusy = false;
				RefreshCommands();
			}
		}

		private async Task CreatePackageAsync()
		{
			IsBusy = true;
			RefreshCommands();

			try
			{
				OperationResult result =
					await _prismOperations.CreateFabPackAsync(
						CreateRequest(),
						CreateProgress());

				if (!result.Success
					&& result.ValidationResults != null
					&& result.ValidationResults.Count > 0)
				{
					ApplyValidationResult(result);
					return;
				}

				if (!result.Success)
				{
					ValidationState = ValidationState.Failed;
					ValidationStatus = "Fab pack failed";
					ValidationSummary = result.Message;

					return;
				}

				ValidationResults.Clear();

				ValidationState = ValidationState.Passed;
				ValidationStatus = "Fab pack issued";
				ValidationSummary = result.Message;
				ResultCount = "No failed parts";
			}
			finally
			{
				IsBusy = false;
				RefreshCommands();
			}
		}

		private void Clear()
		{
			PhaseNumber = string.Empty;
			IssueNumber = string.Empty;
			SiteDate = string.Empty;
			VariationNumber = string.Empty;
			SelectedVariationType = VariationTypes[0];

			ResetValidation();
			RefreshCommands();
		}

		private Progress<OperationProgress> CreateProgress()
		{
			return new Progress<OperationProgress>(
				progress => _reportProgress?.Invoke(progress));
		}

		private void RefreshCommands()
		{
			CheckSelectionCommand.RaiseCanExecuteChanged();
			CreatePackageCommand.RaiseCanExecuteChanged();
		}
	}
}