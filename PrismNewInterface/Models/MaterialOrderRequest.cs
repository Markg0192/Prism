using System;

namespace PrismNewInterface.Models
{
	public sealed class MaterialOrderRequest
	{
		public string OrderAction
		{
			get;
			set;
		}

		public string OrderType
		{
			get;
			set;
		}

		public string PhaseNumber
		{
			get;
			set;
		}

		public string IssueNumber
		{
			get;
			set;
		}

		public string VariationType
		{
			get;
			set;
		}

		public string VariationNumber
		{
			get;
			set;
		}

		public string RequiredBy
		{
			get;
			set;
		}

		public MaterialOrderRequest()
		{
			OrderAction = string.Empty;
			OrderType = string.Empty;
			PhaseNumber = string.Empty;
			IssueNumber = string.Empty;
			VariationType = string.Empty;
			VariationNumber = string.Empty;
		}
	}
}