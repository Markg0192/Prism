using System.Collections.Generic;

namespace PrismNewInterface.Models
{
	public sealed class ClassificationCodeSetting
	{
		public string SelectionFilter { get; set; }
		public string Code { get; set; }
		public string Title { get; set; }

		public ClassificationCodeSetting()
		{
			SelectionFilter = string.Empty;
			Code = string.Empty;
			Title = string.Empty;
		}

		public ClassificationCodeSetting(string selectionFilter, string code, string title)
		{
			SelectionFilter = selectionFilter ?? string.Empty;
			Code = code ?? string.Empty;
			Title = title ?? string.Empty;
		}
	}

	public sealed class ProjectUserSettings
	{
		public string ProjectManagement { get; set; }
		public string DrawingOfficeManager { get; set; }
		public string DocumentControl { get; set; }
		public string Others { get; set; }

		public ProjectUserSettings()
		{
			ProjectManagement = string.Empty;
			DrawingOfficeManager = string.Empty;
			DocumentControl = string.Empty;
			Others = string.Empty;
		}
	}

	public sealed class AdvancedPrismSettings
	{
		public string PrelimPrefix { get; set; }
		public string FabPackType { get; set; }
		public string FabsecGreen { get; set; }

		public AdvancedPrismSettings()
		{
			PrelimPrefix = string.Empty;
			FabPackType = string.Empty;
			FabsecGreen = "100";
		}
	}

	public sealed class PackageDirectorySettings
	{
		public string Material { get; set; }
		public string Carcasses { get; set; }
		public string Bolts { get; set; }
		public string Seversafe { get; set; }
		public string FabPack { get; set; }
		public string Variation { get; set; }

		public PackageDirectorySettings()
		{
			Material = string.Empty;
			Carcasses = string.Empty;
			Bolts = string.Empty;
			Seversafe = string.Empty;
			FabPack = string.Empty;
			Variation = string.Empty;
		}
	}
}