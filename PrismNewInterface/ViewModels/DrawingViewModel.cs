using PrismNewInterface.Infrastructure;
using PrismNewInterface.Models;
using PrismNewInterface.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PrismNewInterface.ViewModels
{
	/// <summary>
	/// Handles the drawing workflow shown in the Drawing tab.
	/// Runs drawing checks, drawing creation and orientation detailing.
	/// </summary>
	public sealed class DrawingViewModel : WorkflowViewModelBase
	{
		private readonly PrismOperations _prismOperations;
		private readonly Action<OperationProgress> _reportProgress;

		private string _selectedOrientationHoleType;
		private string _pltOnFlange;

		public DrawingViewModel(PrismOperations prismOperations, Action<OperationProgress> reportProgress)
		{
			if (prismOperations == null)
			{
				throw new ArgumentNullException(nameof(prismOperations));
			}

			_prismOperations = prismOperations;
			_reportProgress = reportProgress;

			OrientationHoleTypes = new ObservableCollection<string>
			{
				"Holes",
				"Holes and Plate",
				"Plate"
			};

			CheckSelectionCommand = new AsyncRelayCommand(CheckSelectionAsync, CanRunOperation);
			CreateDrawingsCommand = new AsyncRelayCommand(CreateDrawingsAsync, CanRunOperation);
			DetailOrientationHolesCommand = new AsyncRelayCommand(DetailOrientationHolesAsync, CanDetailOrientationHoles);
			ClearCommand = new RelayCommand(Clear);

			SelectedOrientationHoleType = OrientationHoleTypes[0];
		}

		public ObservableCollection<string> OrientationHoleTypes { get; private set; }

		public string SelectedOrientationHoleType
		{
			get { return _selectedOrientationHoleType; }
			set
			{
				if (SetProperty(ref _selectedOrientationHoleType, value))
				{
					OnPropertyChanged(nameof(ShowPltOnFlange));
					DetailOrientationHolesCommand.RaiseCanExecuteChanged();
				}
			}
		}

		public bool ShowPltOnFlange
		{
			get { return SelectedOrientationHoleType == "Holes and Plate"; }
		}

		public string PltOnFlange
		{
			get { return _pltOnFlange; }
			set
			{
				if (SetProperty(ref _pltOnFlange, value))
				{
					DetailOrientationHolesCommand.RaiseCanExecuteChanged();
				}
			}
		}

		public AsyncRelayCommand CheckSelectionCommand { get; private set; }

		public AsyncRelayCommand CreateDrawingsCommand { get; private set; }

		public AsyncRelayCommand DetailOrientationHolesCommand { get; private set; }

		public RelayCommand ClearCommand { get; private set; }

		public void AutoCompleteApplied()
		{
			ValidationStatus = "Recheck required";
			ValidationSummary = "Auto-Complete applied. Re-run the check to verify the changes.";
		}

		private bool CanRunOperation()
		{
			return !IsBusy;
		}

		private bool CanDetailOrientationHoles()
		{
			if (IsBusy)
			{
				return false;
			}

			if (SelectedOrientationHoleType == "Holes and Plate")
			{
				return !string.IsNullOrWhiteSpace(PltOnFlange);
			}

			return true;
		}

		private async Task CheckSelectionAsync()
		{
			BeginValidation();
			RefreshCommands();

			try
			{
				OperationResult result = await _prismOperations.CheckDrawingSelectionAsync(CreateProgress());

				ApplyValidationResult(result);
				OnPropertyChanged();
			}
			finally
			{
				IsBusy = false;
				RefreshCommands();
			}
		}

		private async Task CreateDrawingsAsync()
		{
			IsBusy = true;
			RefreshCommands();

			try
			{
				OperationResult result = await _prismOperations.CreateDrawingsAsync(CreateProgress());

				if (!result.Success && result.ValidationResults != null && result.ValidationResults.Count > 0)
				{
					ApplyValidationResult(result);
					return;
				}

				if (!result.Success)
				{
					ValidationSummary = result.Message;
					return;
				}

				ValidationResults.Clear();

				ValidationState = ValidationState.Passed;
				ValidationStatus = "Passed";
				ValidationSummary = result.Message;
				ResultCount = "No failed parts";
			}
			finally
			{
				IsBusy = false;
				RefreshCommands();
			}
		}

		private async Task DetailOrientationHolesAsync()
		{
			IsBusy = true;
			RefreshCommands();

			try
			{
				OperationResult result = await _prismOperations.DetailOrientationHolesAsync(
					SelectedOrientationHoleType, PltOnFlange, CreateProgress());

				ValidationSummary = result.Message;
			}
			finally
			{
				IsBusy = false;
				RefreshCommands();
			}
		}

		private void Clear()
		{
			ResetValidation();

			OnPropertyChanged();
			RefreshCommands();
		}

		private Progress<OperationProgress> CreateProgress()
		{
			return new Progress<OperationProgress>(progress => _reportProgress?.Invoke(progress));
		}

		private void RefreshCommands()
		{
			CheckSelectionCommand.RaiseCanExecuteChanged();
			CreateDrawingsCommand.RaiseCanExecuteChanged();
			DetailOrientationHolesCommand.RaiseCanExecuteChanged();
		}
	}
}