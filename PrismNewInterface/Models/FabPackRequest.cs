using System;

namespace PrismNewInterface.Models
{
	public sealed class FabPackRequest
	{
		public string PhaseNumber { get; set; }

		public string IssueNumber { get; set; }

		public DateTime? SiteDate { get; set; }

		public string VariationType { get; set; }

		public string VariationNumber { get; set; }
	}
}