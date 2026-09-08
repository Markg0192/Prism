using PrismNewInterface.Infrastructure;
using PrismNewInterface.Models;
using System.Collections.ObjectModel;

namespace PrismNewInterface.ViewModels
{
	public abstract class WorkflowViewModelBase : ObservableObject
	{
		private ValidationState _validationState;
		private string _validationStatus;
		private string _validationSummary;
		private string _resultCount;
		private bool _isBusy;

		protected WorkflowViewModelBase()
		{
			ValidationResults = new ObservableCollection<Models.ValidationResult>();

			ValidationState = ValidationState.NotChecked;

			ValidationStatus = "Not checked";

			ValidationSummary = "Run the check to continue.";

			ResultCount = "No check completed";
		}

		public ObservableCollection<ValidationResult>
			ValidationResults
		{
			get;
			private set;
		}

		public ValidationState ValidationState
		{
			get
			{
				return _validationState;
			}
			protected set
			{
				SetProperty(ref _validationState, value);
			}
		}

		public string ValidationStatus
		{
			get
			{
				return _validationStatus;
			}
			protected set
			{
				SetProperty(ref _validationStatus, value);
			}
		}

		public string ValidationSummary
		{
			get
			{
				return _validationSummary;
			}
			protected set
			{
				SetProperty(ref _validationSummary, value);
			}
		}

		public string ResultCount
		{
			get
			{
				return _resultCount;
			}
			protected set
			{
				SetProperty(ref _resultCount, value);
			}
		}

		public bool IsBusy
		{
			get
			{
				return _isBusy;
			}
			protected set
			{
				SetProperty(ref _isBusy, value);
			}
		}

		protected void BeginValidation()
		{
			IsBusy = true;

			ValidationResults.Clear();

			ValidationState = ValidationState.Checking;

			ValidationStatus = "Checking";

			ValidationSummary = "Checking the current selection...";

			ResultCount = "Check in progress";
		}

		protected void ApplyValidationResult(OperationResult result)
		{
			ValidationResults.Clear();

			if (result.ValidationResults != null)
			{
				foreach (Models.ValidationResult validationResult in result.ValidationResults)
				{
					ValidationResults.Add(validationResult);
				}
			}

			if (result.Success)
			{
				ValidationState = ValidationState.Passed;

				ValidationStatus = "Check passed";

				ValidationSummary = result.Message;

				ResultCount = "No failed parts";
			}
			else
			{
				ValidationState = ValidationState.Failed;

				ValidationStatus = result.Cancelled ? "Check cancelled" : "Check failed";

				ValidationSummary = result.Message;

				ResultCount = ValidationResults.Count == 1 ? "1 part requires attention" : ValidationResults.Count + " parts require attention";
			}

			IsBusy = false;
		}

		protected void ResetValidation()
		{
			ValidationResults.Clear();

			ValidationState = ValidationState.NotChecked;

			ValidationStatus = "Not checked";

			ValidationSummary = "Run the check to continue.";

			ResultCount = "No check completed";

			IsBusy = false;
		}
	}
}