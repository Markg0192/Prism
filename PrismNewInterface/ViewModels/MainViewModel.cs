using PrismNewInterface.Infrastructure;
using PrismNewInterface.Models;
using PrismNewInterface.Services;

namespace PrismNewInterface.ViewModels
{
	public sealed class MainViewModel : ObservableObject
	{
		private string _progressMessage;
		private int _progressPercentage;
		private bool _isBusy;

		public MainViewModel(PrismOperations prismOperations)
		{
			ProgressMessage = "Ready";

			MaterialOrder = new MaterialOrderViewModel(prismOperations, ReportProgress);

			Drawing = new DrawingViewModel(prismOperations, ReportProgress);

			FabPack = new FabPackViewModel(prismOperations, ReportProgress);
		}

		public void SetProgress(int percentage, string message)
		{
			ProgressPercentage = percentage;
			ProgressMessage = message;
		}

		public MaterialOrderViewModel MaterialOrder
		{
			get;
			private set;
		}

		public DrawingViewModel Drawing
		{
			get;
			private set;
		}

		public FabPackViewModel FabPack
		{
			get;
			private set;
		}

		public string ProgressMessage
		{
			get
			{
				return _progressMessage;
			}
			private set
			{
				SetProperty(ref _progressMessage, value);
			}
		}

		public int ProgressPercentage
		{
			get
			{
				return _progressPercentage;
			}
			private set
			{
				SetProperty(ref _progressPercentage, value);
			}
		}

		public bool IsBusy
		{
			get
			{
				return _isBusy;
			}
			private set
			{
				SetProperty(ref _isBusy, value);
			}
		}

		private void ReportProgress(OperationProgress progress)
		{
			if (progress == null)
			{
				return;
			}

			ProgressMessage = progress.Message;

			ProgressPercentage = progress.Percentage;

			IsBusy = progress.Percentage < 100;

			if (progress.Percentage >= 100)
			{
				ProgressMessage = "Ready";
			}
		}
	}
}