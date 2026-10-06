using Tekla.Structures.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Prism
{
	public static class FabEmailBuilder
	{
		public static string Build(PrismProjectData projData, SelectedObjects objects, string issueNumber, string phaseNumber, string siteDate,
			bool zipFileCanBeAttached, string teklaVersion)
		{
			const int highSecondaryPartLimit = 10;
			const double highWftLimit = 2000;

			FabEmailSummary summary = BuildFabEmailSummary(objects, highSecondaryPartLimit, highWftLimit);

			string projectNumber = Encode(projData.ProjNumber);
			string projectName = Encode(projData.ProjName);
			string phase = Encode(phaseNumber);
			string issue = Encode(issueNumber);
			string formattedSiteDate = Encode(FormatSiteDate(siteDate));
			string modelName = Encode(projData.ModelName);
			string fabViewName = Encode(ModelUDA.FabViewAndFilterStamp(phaseNumber));
			string teklaVersionText = Encode(teklaVersion);

			string variationHtml = BuildVariationHtml(projData);
			string headerStatusHtml = BuildHeaderStatusHtml(zipFileCanBeAttached);
			string packageSummaryHtml = BuildPackageSummaryHtml(summary);
			string productionAwarenessHtml = BuildProductionAwarenessHtml(summary);
			string paintShopAwarenessHtml = BuildPaintShopAwarenessHtml(summary, highWftLimit);

			return BuildFabEmailHtml(projectNumber, projectName, phase, issue, formattedSiteDate, modelName, fabViewName, teklaVersionText,
				variationHtml, headerStatusHtml, packageSummaryHtml, productionAwarenessHtml, paintShopAwarenessHtml);
		}

		private static FabEmailSummary BuildFabEmailSummary(SelectedObjects objects, int highSecondaryPartLimit, double highWftLimit)
		{
			FabEmailSummary summary = new FabEmailSummary();

			if (objects == null) return summary;

			List<PrismPart> mainParts = objects.GetMainParts();

			summary.AssemblyCount = mainParts.Count;
			summary.PartCount = objects.PrismParts != null ? objects.PrismParts.Count : 0;
			summary.TotalWeight = objects.MainPartWeight + objects.FittingWeight;
			summary.FittingWeight = objects.FittingWeight;

			summary.ShopBoltCount = objects.PrismBoltGroups != null
				? objects.PrismBoltGroups
					.Where(bolt => bolt != null && bolt.isShop && bolt.BoltGroup != null)
					.Sum(bolt => bolt.BoltGroup.BoltPositions.Count)
				: 0;

			summary.BoughtOutObjectCount = objects.PrismParts != null
				? objects.PrismParts.Count(part => !string.IsNullOrWhiteSpace(part.PartPrefix) &&
					part.PartPrefix.Equals("BO", StringComparison.OrdinalIgnoreCase))
				: 0;

			summary.OverSixMetreAssemblyCount = mainParts.Count(part => part.Part != null && ModelModifiers.GetPartLength(part.Part) > 6000);
			summary.AbnormalAssemblyCount = mainParts.Count(part => part.IsAbnormal);
			summary.HighSecondaryAssemblyCount = CountHighSecondaryAssemblies(mainParts, highSecondaryPartLimit);

			summary.FinishCount = mainParts
				.Where(part => !string.IsNullOrWhiteSpace(part.Finish))
				.Select(part => part.Finish.Trim())
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.Count();

			summary.HighWftPartCount = objects.PrismParts != null
				? objects.PrismParts.Count(part => GetWft(part) > highWftLimit)
				: 0;

			CalculatePaintSurfacePercentages(mainParts, summary);

			return summary;
		}

		private static int CountHighSecondaryAssemblies(List<PrismPart> mainParts, int secondaryPartLimit)
		{
			if (mainParts == null) return 0;

			return mainParts
				.Where(part => part.Assembly != null)
				.GroupBy(part => part.Assembly.Identifier.GUID)
				.Select(group => group.First())
				.Count(part => part.Assembly.GetSecondaries().OfType<Part>().Count() > secondaryPartLimit);
		}

		private static void CalculatePaintSurfacePercentages(List<PrismPart> mainParts, FabEmailSummary summary)
		{
			double intumescentArea = 0;
			double galvanisedArea = 0;
			double otherArea = 0;

			foreach (PrismPart mainPart in mainParts
				.Where(part => part.Assembly != null)
				.GroupBy(part => part.Assembly.Identifier.GUID)
				.Select(group => group.First()))
			{
				double assemblyArea = GetAssemblySurfaceArea(mainPart.Assembly);
				string finish = mainPart.Finish == null ? string.Empty : mainPart.Finish.Trim();

				if (finish.StartsWith("IP", StringComparison.OrdinalIgnoreCase))
					intumescentArea += assemblyArea;
				else if (finish.StartsWith("G", StringComparison.OrdinalIgnoreCase))
					galvanisedArea += assemblyArea;
				else
					otherArea += assemblyArea;
			}

			double totalArea = intumescentArea + galvanisedArea + otherArea;

			if (totalArea <= 0) return;

			summary.IntumescentAreaPercentage = (intumescentArea / totalArea) * 100;
			summary.GalvanisedAreaPercentage = (galvanisedArea / totalArea) * 100;
			summary.OtherFinishAreaPercentage = (otherArea / totalArea) * 100;
		}

		private static double GetAssemblySurfaceArea(Assembly assembly)
		{
			if (assembly == null) return 0;

			double area = 0;

			if (assembly.GetMainPart() is Part mainPart)
				area += GetPartSurfaceArea(mainPart);

			ArrayList secondaries = assembly.GetSecondaries();

			foreach (object secondary in secondaries)
			{
				if (secondary is Part part)
					area += GetPartSurfaceArea(part);
			}

			return area;
		}

		private static double GetPartSurfaceArea(Part part)
		{
			if (part == null) return 0;

			double area = 0;
			part.GetReportProperty("AREA", ref area);

			return area;
		}

		private static double GetWft(PrismPart part)
		{
			if (part == null) return 0;

			double sherwinWft = 0;
			double hempelWft = 0;

			double.TryParse(part.SherwinWft, out sherwinWft);
			double.TryParse(part.HempelWft, out hempelWft);

			return Math.Max(sherwinWft, hempelWft);
		}

		private static string BuildVariationHtml(PrismProjectData projData)
		{
			if (!projData.IsVariation) return string.Empty;

			return $@"
	<tr>
		<td colspan='4'
			style='
				padding:7px 0 2px 0;
				border-top:1px solid #E5E5E5;
				font-size:9.5pt;
				color:#666666;'>
			Variation:
			<span style='
				font-weight:600;
				color:#222222;'>
				{Encode(projData.VariationNumber)}
			</span>
		</td>
	</tr>";
		}

		private static string BuildHeaderStatusHtml(bool zipFileCanBeAttached)
		{
			string attachmentBackground = zipFileCanBeAttached ? "#4A5960" : "#8A5A18";
			string attachmentText = zipFileCanBeAttached ? "PACKAGE ATTACHED" : "PACKAGE TOO LARGE TO ATTACH";
			string actionText = zipFileCanBeAttached ? "Issue to works" : "See link in Additional Information";

			return $@"
	<table cellpadding='0'
		   cellspacing='0'
		   border='0'
		   align='right'
		   style='
			   border-collapse:collapse;
			   text-align:right;'>

		<tr>
			<td style='
				padding:5px 9px;
				background-color:{attachmentBackground};
				font-size:9pt;
				font-weight:600;
				color:#ffffff;'>
				{attachmentText}
			</td>
		</tr>

		<tr>
			<td style='
				padding-top:7px;
				font-size:9pt;
				color:#DDE4E7;'>
				<span style='font-weight:600;'>ACTION:</span>
				&nbsp;{actionText}
			</td>
		</tr>

	</table>";
		}

		private static string BuildPackageSummaryHtml(FabEmailSummary summary)
		{
			return $@"
	<table width='100%'
		   cellpadding='0'
		   cellspacing='0'
		   border='0'
		   style='
			   width:100%;
			   table-layout:fixed;
			   border-collapse:collapse;'>

		<tr>
			{BuildSummaryCell(summary.AssemblyCount.ToString(), "Assemblies", "25%", true)}
			{BuildSummaryCell(summary.PartCount.ToString(), "Parts", "25%", true)}
			{BuildSummaryCell($"{summary.TotalWeight:0.###} t", "Total Weight", "25%", true)}
			{BuildSummaryCell($"{summary.FittingWeight:0.###} t", "PLT / RSA Weight", "25%", false)}
		</tr>

	</table>";
		}

		private static string BuildProductionAwarenessHtml(FabEmailSummary summary)
		{
			string highSecondaryBackground = WarningBackground(summary.HighSecondaryAssemblyCount > 0);
			string highSecondaryText = WarningText(summary.HighSecondaryAssemblyCount > 0);

			string shopBoltBackground = WarningBackground(summary.ShopBoltCount > 0);
			string shopBoltText = WarningText(summary.ShopBoltCount > 0);

			string boughtOutBackground = WarningBackground(summary.BoughtOutObjectCount > 0);
			string boughtOutText = WarningText(summary.BoughtOutObjectCount > 0);

			string longAssemblyBackground = WarningBackground(summary.OverSixMetreAssemblyCount > 0);
			string longAssemblyText = WarningText(summary.OverSixMetreAssemblyCount > 0);

			string abnormalBackground = WarningBackground(summary.AbnormalAssemblyCount > 0);
			string abnormalText = WarningText(summary.AbnormalAssemblyCount > 0);

			return $@"
	{BuildSectionHeading("Production Awareness")}

	<table width='100%'
		   cellpadding='0'
		   cellspacing='0'
		   border='0'
		   style='
			   width:100%;
			   table-layout:fixed;
			   border-collapse:collapse;'>

		<tr>
			{BuildSummaryCell(summary.HighSecondaryAssemblyCount.ToString(), "Assemblies with >10 Fittings", "20%", true,
				highSecondaryBackground, highSecondaryText)}

			{BuildSummaryCell(summary.ShopBoltCount.ToString(), "Shop Bolts", "20%", true,
				shopBoltBackground, shopBoltText)}

			{BuildSummaryCell(summary.BoughtOutObjectCount.ToString(), "Bought Out Items", "20%", true,
				boughtOutBackground, boughtOutText)}

			{BuildSummaryCell(summary.OverSixMetreAssemblyCount.ToString(), "Assemblies Over 6m", "20%", true,
				longAssemblyBackground, longAssemblyText)}

			{BuildSummaryCell(summary.AbnormalAssemblyCount.ToString(), "Abnormal Assemblies", "20%", false,
				abnormalBackground, abnormalText)}
		</tr>

	</table>";
		}

		private static string BuildPaintShopAwarenessHtml(FabEmailSummary summary, double highWftLimit)
		{
			string finishBackground = WarningBackground(summary.FinishCount >= 2);
			string finishText = WarningText(summary.FinishCount >= 2);

			string highWftBackground = WarningBackground(summary.HighWftPartCount > 0);
			string highWftText = WarningText(summary.HighWftPartCount > 0);

			return $@"
	{BuildSectionHeading("Paint Shop Awareness")}

	<table width='100%'
		   cellpadding='0'
		   cellspacing='0'
		   border='0'
		   style='
			   width:100%;
			   table-layout:fixed;
			   border-collapse:collapse;'>

		<tr>
			{BuildSummaryCell($"{summary.IntumescentAreaPercentage:0.#}%", "Intumescent", "20%", true)}
			{BuildSummaryCell($"{summary.GalvanisedAreaPercentage:0.#}%", "Galvanised", "20%", true)}
			{BuildSummaryCell($"{summary.OtherFinishAreaPercentage:0.#}%", "Other Finish", "20%", true)}

			{BuildSummaryCell(summary.FinishCount.ToString(), "Different Finishes", "20%", true,
				finishBackground, finishText)}

			{BuildSummaryCell(summary.HighWftPartCount.ToString(), $"Parts Over {highWftLimit:0} WFT", "20%", false,
				highWftBackground, highWftText)}
		</tr>

	</table>";
		}

		private static string BuildSectionHeading(string title)
		{
			return $@"
	<table width='100%'
		   cellpadding='0'
		   cellspacing='0'
		   border='0'
		   style='
			   width:100%;
			   border-collapse:collapse;
			   margin-top:15px;
			   margin-bottom:8px;'>

		<tr>
			<td style='
				padding:0 0 6px 0;
				font-size:9pt;
				font-weight:600;
				color:#777777;
				border-bottom:1px solid #D9D9D9;'>
				{Encode(title)}
			</td>
		</tr>

	</table>";
		}

		private static string BuildSummaryCell(string value, string label, string width, bool addRightBorder,
			string backgroundColor = "#F5F6F7", string textColor = "#333333")
		{
			string rightBorder = addRightBorder ? "border-right:5px solid #ffffff;" : string.Empty;

			return $@"
	<td width='{width}'
		align='center'
		valign='middle'
		style='
			width:{width};
			padding:11px 5px;
			background-color:{backgroundColor};
			{rightBorder}'>

		<div style='
			font-size:18px;
			font-weight:600;
			color:{textColor};'>
			{Encode(value)}
		</div>

		<div style='
			margin-top:3px;
			font-size:9pt;
			color:{textColor};'>
			{Encode(label)}
		</div>

	</td>";
		}

		private static string WarningBackground(bool warning)
		{
			return warning ? "#FFF4E5" : "#F5F6F7";
		}

		private static string WarningText(bool warning)
		{
			return warning ? "#8A4B08" : "#666666";
		}

		private static string BuildFabEmailHtml(string projectNumber, string projectName, string phase, string issue, string siteDate, string modelName,
			string fabViewName, string teklaVersion, string variationHtml, string headerStatusHtml, string packageSummaryHtml,
			string productionAwarenessHtml, string paintShopAwarenessHtml)
		{
			return $@"
<html>

<body style='
	margin:0;
	padding:0;
	font-family:Segoe UI, Arial, sans-serif;
	font-size:11pt;
	color:#333333;
	background-color:#ffffff;'>

<table width='800'
	   cellpadding='0'
	   cellspacing='0'
	   border='0'
	   style='
		   width:800px;
		   border-collapse:collapse;'>

	<!-- HEADER -->
	<tr>
		<td style='
			padding:16px 22px;
			background-color:#2F3E46;
			color:#ffffff;'>

			<table width='100%'
				   cellpadding='0'
				   cellspacing='0'
				   border='0'
				   style='
					   width:100%;
					   border-collapse:collapse;'>

				<tr>

					<td width='65%'
						valign='middle'
						style='width:65%;'>

						<div style='
							font-size:20px;
							font-weight:600;
							margin-bottom:4px;'>
							Fabrication Package
						</div>

						<div style='
							font-size:11pt;
							color:#E3E7E9;'>
							{projectNumber} &nbsp;|&nbsp; {projectName}
						</div>

					</td>

					<td width='35%'
						valign='middle'
						align='right'
						style='width:35%;'>

						{headerStatusHtml}

					</td>

				</tr>

			</table>

		</td>
	</tr>


	<!-- PACKAGE INFORMATION -->
	<tr>
		<td style='padding:14px 22px 7px 22px;'>

			<table width='100%'
				   cellpadding='0'
				   cellspacing='0'
				   border='0'
				   style='
					   width:100%;
					   table-layout:fixed;
					   border-collapse:collapse;'>

				<tr>

					<td width='20%'
						valign='top'
						style='
							width:20%;
							padding:7px 10px 7px 0;
							border-right:1px solid #E1E1E1;'>

						<div style='
							font-size:9.5pt;
							color:#777777;
							margin-bottom:2px;'>
							Phase / Issue
						</div>

						<div style='
							font-size:11pt;
							font-weight:600;
							color:#222222;
							white-space:normal;
							word-wrap:break-word;
							overflow-wrap:break-word;'>
							{phase} / {issue}
						</div>

					</td>

					<td width='20%'
						valign='top'
						style='
							width:20%;
							padding:7px 10px;
							border-right:1px solid #E1E1E1;'>

						<div style='
							font-size:9.5pt;
							color:#777777;
							margin-bottom:2px;'>
							Site Date
						</div>

						<div style='
							font-size:11pt;
							font-weight:600;
							color:#222222;
							white-space:normal;
							word-wrap:break-word;
							overflow-wrap:break-word;'>
							{siteDate}
						</div>

					</td>

					<td width='45%'
						valign='top'
						style='
							width:45%;
							padding:7px 10px;
							border-right:1px solid #E1E1E1;'>

						<div style='
							font-size:9.5pt;
							color:#777777;
							margin-bottom:2px;'>
							Model / View
						</div>

						<div style='
							font-size:11pt;
							font-weight:600;
							color:#222222;
							white-space:normal;
							word-wrap:break-word;
							overflow-wrap:break-word;'>
							{modelName} / {fabViewName}
						</div>

					</td>

					<td width='15%'
						valign='top'
						style='
							width:15%;
							padding:7px 0 7px 10px;'>

						<div style='
							font-size:9.5pt;
							color:#777777;
							margin-bottom:2px;'>
							Tekla
						</div>

						<div style='
							font-size:11pt;
							font-weight:600;
							color:#222222;
							white-space:normal;
							word-wrap:break-word;
							overflow-wrap:break-word;'>
							{teklaVersion}
						</div>

					</td>

				</tr>

				{variationHtml}

			</table>

		</td>
	</tr>


	<!-- ADDITIONAL INFORMATION -->
<tr>
	<td style='padding:10px 22px 4px 22px;'>

		<div style='
			font-size:12pt;
			font-weight:600;
			margin-bottom:6px;'>
			Additional Information
		</div>

		<div style='
			padding:9px 12px;
			background-color:#FFFBEA;
			border-left:4px solid #D6B656;
			color:#666666;'>

			[Add any project, area or fabrication-specific information here.]

		</div>

	</td>
</tr>

	<!-- PACKAGE SUMMARY -->
	<tr>
		<td style='padding:15px 22px 14px 22px;'>

			<div style='
				font-size:12pt;
				font-weight:600;
				margin-bottom:8px;'>
				Package Summary
			</div>

			{packageSummaryHtml}

			{productionAwarenessHtml}

			{paintShopAwarenessHtml}

		</td>
	</tr>

</table>

</body>
</html>";
		}

		private static string Encode(string value)
		{
			return System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
		}

		private static string FormatSiteDate(string siteDate)
		{
			if (string.IsNullOrWhiteSpace(siteDate)) return "Unconfirmed";

			if (DateTime.TryParse(siteDate, out DateTime parsedDate))
				return parsedDate.ToString("dd/MM/yyyy");

			return siteDate.Trim();
		}

		private sealed class FabEmailSummary
		{
			public int AssemblyCount { get; set; }
			public int PartCount { get; set; }
			public double TotalWeight { get; set; }
			public double FittingWeight { get; set; }

			public int HighSecondaryAssemblyCount { get; set; }
			public int ShopBoltCount { get; set; }
			public int BoughtOutObjectCount { get; set; }
			public int OverSixMetreAssemblyCount { get; set; }
			public int AbnormalAssemblyCount { get; set; }

			public int FinishCount { get; set; }
			public int HighWftPartCount { get; set; }
			public double IntumescentAreaPercentage { get; set; }
			public double GalvanisedAreaPercentage { get; set; }
			public double OtherFinishAreaPercentage { get; set; }
		}
	}
}