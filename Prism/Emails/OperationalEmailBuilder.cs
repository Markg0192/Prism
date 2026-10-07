using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace Prism
{
	public static class OperationalEmailBuilder
	{
		private const string MaterialColour = "#365F4D";
		private const string BoltColour = "#7A3E3E";
		private const string EpoColour = "#34656D";
		private const string FabsecColour = "#56516E";

		public static string BuildMaterial(PrismProjectData projData, SelectedObjects objects, string orderType, string phaseNumber, string issueNumber, string siteDate, bool zipFileCanBeAttached = true)
		{
			string title = GetMaterialTitle(orderType);
			string action = GetMaterialAction(orderType);
			string dateValue = orderType.Contains("Omit") ? "Not applicable" : FormatDate(siteDate);
			int fabsecCount = objects != null ? objects.GetFabsecParts().Count : 0;

			string summary = BuildSummaryRow(new[]
			{
				new SummaryItem(objects != null && objects.PrismParts != null ? objects.PrismParts.Count.ToString() : "0", "Parts"),
				new SummaryItem(objects != null ? $"{objects.MainPartWeight + objects.FittingWeight:0.###} t" : "0 t", "Total Weight"),
				new SummaryItem(fabsecCount.ToString(), "Fabsec Material")
			});

			return BuildEmail(projData, title, MaterialColour, action, phaseNumber, issueNumber, "Material Required", dateValue, summary, zipFileCanBeAttached);
		}

		public static string BuildBoltOrder(PrismProjectData projData, string phaseNumber, string issueNumber, string siteDate, int shopBoltCount, int siteBoltCount, bool zipFileCanBeAttached = true)
		{
			return BuildEmail(projData, "Bolt Order", BoltColour, "Order these bolts", phaseNumber, issueNumber, "Site Date", FormatDate(siteDate),
				BuildSummaryRow(new[]
				{
					new SummaryItem(shopBoltCount.ToString(), "Shop Bolts"),
					new SummaryItem(siteBoltCount.ToString(), "Site Bolts")
				}), zipFileCanBeAttached);
		}

		public static string BuildEpoOrder(PrismProjectData projData, string phaseNumber, string issueNumber, string siteDate, bool zipFileCanBeAttached = true)
		{
			string summary = BuildSummaryRow(new[]
			{
				new SummaryItem($"{SeversafeOrder.LinMeterRun1mSystem:0.##} m", "1m Edge"),
				new SummaryItem($"{SeversafeOrder.LinMeterRun1_8mSystem:0.##} m", "1.8m Edge"),
				new SummaryItem($"{SeversafeOrder.LinMeterRun1mPhaseBreak:0.##} m", "1m Phase Break"),
				new SummaryItem($"{SeversafeOrder.LinMeterRun1_8mPhaseBreak:0.##} m", "1.8m Phase Break")
			});

			return BuildEmail(projData, "Seversafe / EPO Order", EpoColour, "Make this order available", phaseNumber, issueNumber, "Site Date", FormatDate(siteDate),
				summary, zipFileCanBeAttached);
		}

		public static string BuildFabsecCarcassOrder(PrismProjectData projData, List<PrismPart> fabsecs, string phaseNumber, string issueNumber, string siteDate, bool zipFileCanBeAttached = true)
		{
			List<PrismPart> parts = fabsecs ?? new List<PrismPart>();
			int uniqueFabsecs = parts.Where(part => part != null && !string.IsNullOrWhiteSpace(part.PartMark))
				.Select(part => part.PartMark.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count();
			double totalWeight = parts.Where(part => part != null).Sum(part => part.Weight);

			return BuildEmail(projData, "Fabsec Carcass Order", FabsecColour, "Process these carcasses", phaseNumber, issueNumber, "Required for Fab", FormatDate(siteDate),
				BuildSummaryRow(new[]
				{
					new SummaryItem(parts.Count.ToString(), "Fabsec Parts"),
					new SummaryItem(uniqueFabsecs.ToString(), "Unique Fabsecs"),
					new SummaryItem($"{totalWeight:0.###} t", "Total Weight")
				}), zipFileCanBeAttached);
		}

		private static string BuildEmail(PrismProjectData projData, string title, string colour, string action, string phaseNumber, string issueNumber,
			string dateLabel, string dateValue, string summaryHtml, bool zipFileCanBeAttached)
		{
			string attachmentText = zipFileCanBeAttached ? "PACKAGE ATTACHED" : "PACKAGE TOO LARGE TO ATTACH";
			string actionText = zipFileCanBeAttached ? action : "See link in Additional Information";

			return $@"
<html>
<body style='margin:0;padding:0;font-family:Segoe UI,Arial,sans-serif;font-size:11pt;color:#333333;background-color:#ffffff;'>
<table width='800' cellpadding='0' cellspacing='0' border='0' style='width:800px;border-collapse:collapse;'>
	<tr>
		<td style='padding:16px 22px;background-color:{colour};color:#ffffff;'>
			<table width='100%' cellpadding='0' cellspacing='0' border='0' style='width:100%;border-collapse:collapse;'>
				<tr>
					<td width='65%' valign='middle' style='width:65%;'>
						<div style='font-size:20px;font-weight:600;margin-bottom:4px;color:#ffffff;'>{Encode(title)}</div>
						<div style='font-size:11pt;color:#F0F2F2;'>{Encode(projData.ProjNumber)} &nbsp;|&nbsp; {Encode(projData.ProjName)}</div>
					</td>
					<td width='35%' valign='middle' align='right' style='width:35%;'>
						<table cellpadding='0' cellspacing='0' border='0' align='right' style='border-collapse:collapse;text-align:right;'>
							<tr><td style='padding:5px 0;color:#ffffff;font-size:9pt;font-weight:600;'>{attachmentText}</td></tr>
							<tr><td style='padding-top:7px;font-size:9pt;color:#F0F2F2;'><span style='font-weight:600;'>ACTION:</span>&nbsp; {Encode(actionText)}</td></tr>
						</table>
					</td>
				</tr>
			</table>
		</td>
	</tr>
	<tr>
		<td style='padding:14px 22px 7px 22px;'>
			<table width='100%' cellpadding='0' cellspacing='0' border='0' style='width:100%;table-layout:fixed;border-collapse:collapse;'>
				<tr>
					{BuildInfoCell("Phase / Issue", phaseNumber + " / " + issueNumber, "35%", true)}
					{BuildInfoCell(dateLabel, dateValue, "35%", true)}
					{BuildInfoCell("Model", projData.ModelName, "30%", false)}
				</tr>
				{BuildVariationRow(projData)}
			</table>
		</td>
	</tr>
	<tr>
		<td style='padding:10px 22px 4px 22px;'>
			<div style='font-size:12pt;font-weight:600;margin-bottom:6px;'>Additional Information</div>
			<div style='padding:9px 12px;background-color:#FFFBEA;border-left:4px solid #D6B656;color:#666666;'>[Add any project, area or order-specific information here.]</div>
		</td>
	</tr>
	<tr><td style='padding:15px 22px 14px 22px;'>{summaryHtml}</td></tr>
</table>
</body>
</html>";
		}

		private static string BuildInfoCell(string label, string value, string width, bool rightBorder)
		{
			string border = rightBorder ? "border-right:1px solid #E1E1E1;" : string.Empty;
			return $@"<td width='{width}' valign='top' style='width:{width};padding:7px 10px;{border}'><div style='font-size:9.5pt;color:#777777;margin-bottom:2px;'>{Encode(label)}</div><div style='font-size:11pt;font-weight:600;color:#222222;white-space:normal;word-wrap:break-word;'>{Encode(value)}</div></td>";
		}

		private static string BuildSummaryRow(IEnumerable<SummaryItem> items)
		{
			List<SummaryItem> values = items.Where(item => item != null).ToList();
			if (values.Count == 0) return string.Empty;

			string width = (100.0 / values.Count).ToString("0.##", CultureInfo.InvariantCulture) + "%";
			string cells = string.Empty;

			for (int i = 0; i < values.Count; i++)
			{
				string border = i < values.Count - 1 ? "border-right:5px solid #ffffff;" : string.Empty;
				cells += $@"<td width='{width}' align='center' valign='middle' style='width:{width};padding:11px 5px;background-color:#F5F6F7;{border}'><div style='font-size:18px;font-weight:600;color:#333333;'>{Encode(values[i].Value)}</div><div style='margin-top:3px;font-size:9pt;color:#666666;'>{Encode(values[i].Label)}</div></td>";
			}

			return $@"<table width='100%' cellpadding='0' cellspacing='0' border='0' style='width:100%;table-layout:fixed;border-collapse:collapse;'><tr>{cells}</tr></table>";
		}

		private static string BuildVariationRow(PrismProjectData projData)
		{
			if (!projData.IsVariation) return string.Empty;
			return $@"<tr><td colspan='3' style='padding:7px 10px 2px 10px;border-top:1px solid #E5E5E5;font-size:9.5pt;color:#666666;'>Variation: <span style='font-weight:600;color:#222222;'>{Encode(projData.VariationNumber)}</span></td></tr>";
		}

		private static string GetMaterialTitle(string orderType)
		{
			if (orderType.Contains("Special Fittings"))
			{
				if (orderType.Contains("Omit")) return "Special Fittings Omit";
				if (orderType.Contains("Add")) return "Additional Special Fittings";
				return "Special Fittings Order";
			}

			if (orderType.Contains("Omit")) return "Material Omit";
			if (orderType.Contains("Add")) return "Additional Material Order";
			return "Material Order";
		}

		private static string GetMaterialAction(string orderType)
		{
			if (orderType.Contains("Omit")) return "Remove from the phase material order";
			if (orderType.Contains("Add")) return "Add to the phase material order";
			return "Order this material";
		}

		private static string GetMaterialDescription(string orderType)
		{
			if (orderType.Contains("Omit")) return "The attached package identifies material to be omitted from this phase.";
			if (orderType.Contains("Add")) return "The attached package identifies additional material required for this phase.";
			return "The attached package contains the material required for this phase.";
		}

		private static string FormatDate(string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return "Unconfirmed";
			DateTime parsed;
			return DateTime.TryParse(value, out parsed) ? parsed.ToString("dd/MM/yyyy") : value.Trim();
		}

		private static string Encode(string value)
		{
			return WebUtility.HtmlEncode(value ?? string.Empty);
		}

		private sealed class SummaryItem
		{
			public SummaryItem(string value, string label) { Value = value; Label = label; }
			public string Value { get; private set; }
			public string Label { get; private set; }
		}
	}
}
