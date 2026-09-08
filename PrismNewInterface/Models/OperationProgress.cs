namespace PrismNewInterface.Models
{
	public sealed class OperationProgress
	{
		public int Percentage
		{
			get;
			set;
		}

		public string Message
		{
			get;
			set;
		}

		public OperationProgress()
		{
			Message = string.Empty;
		}

		public OperationProgress(
			int percentage,
			string message)
		{
			Percentage = percentage;
			Message = message ?? string.Empty;
		}
	}
}