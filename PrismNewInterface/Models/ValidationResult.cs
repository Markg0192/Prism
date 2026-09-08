using Prism;
using System.Collections.Generic;

namespace PrismNewInterface.Models
{
	public sealed class ValidationResult
	{
		public string PartMark { get; set; }

		public string Guid { get; set; }

		public List<ValidationCheckResult> Checks { get; set; }

		public ValidationResult(PrismPart part)
		{
			PartMark = part.PartMark;
			Guid = part.Guid;
			Checks = new List<ValidationCheckResult>();
		}
	}

	public sealed class ValidationCheckResult
	{
		public string CheckName { get; set; }

		public bool Passed { get; set; }

		public AutoCompleteType AutoCompleteType { get; set; }

		public string Result
		{
			get { return Passed ? "Pass" : "Fail"; }
		}

		public bool CanAutoComplete
		{
			get { return AutoCompleteType != AutoCompleteType.None; }
		}

		public ValidationCheckResult()
		{
			CheckName = string.Empty;
			AutoCompleteType = AutoCompleteType.None;
		}
	}
}