using System.Collections.Generic;

namespace PrismNewInterface.Models
{
	public sealed class OperationResult
	{
		public bool Success
		{
			get;
			private set;
		}

		public bool Cancelled
		{
			get;
			private set;
		}

		public string Message
		{
			get;
			private set;
		}

		public IList<ValidationResult> ValidationResults
		{
			get;
			private set;
		}

		private OperationResult()
		{
			Message = string.Empty;

			ValidationResults =
				new List<ValidationResult>();
		}

		public static OperationResult Successful(
			string message)
		{
			return new OperationResult
			{
				Success = true,
				Cancelled = false,
				Message = message ?? string.Empty
			};
		}

		public static OperationResult Failed(string message)
		{
			return new OperationResult
			{
				Success = false,
				Cancelled = false,
				Message = message ?? string.Empty
			};
		}

		public static OperationResult Failed(string message, IList<ValidationResult> validationResults)
		{
			OperationResult result = Failed(message);

			result.ValidationResults = validationResults ?? new List<ValidationResult>();

			return result;
		}

		public static OperationResult CancelledResult(
			string message)
		{
			return new OperationResult
			{
				Success = false,
				Cancelled = true,
				Message = message ?? string.Empty
			};
		}
	}
}