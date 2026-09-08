using Prism;
using PrismNewInterface.Models;
using System;
using System.Collections.Generic;

namespace PrismNewInterface.Services
{
	internal sealed class PrismSettingsService
	{
		private const int ClassificationCodeCount = 8;
		private const int ClassificationCodeStartLine = 8;

		private const int ProjectManagementLine = 17;
		private const int DrawingOfficeManagerLine = 18;
		private const int DocumentControlLine = 19;
		private const int OthersLine = 20;

		private const string ClassificationHeader = "---Filter------------Code----------------Title";

		private readonly Func<PrismProjectData> _projectDataProvider;

		public PrismSettingsService(Func<PrismProjectData> projectDataProvider)
		{
			_projectDataProvider = projectDataProvider ?? throw new ArgumentNullException(nameof(projectDataProvider));
		}

		public IList<ClassificationCodeSetting> GetClassificationCodes()
		{
			List<ClassificationCodeSetting> results = new List<ClassificationCodeSetting>();

			PrismProjectData projectData = GetProjectData();

			if (projectData == null)
			{
				AddEmptyClassificationRows(results);
				return results;
			}

			string fileLocation = Constants.ModelProjectInforLocation(projectData.ProjNumberAndGuid);
			string[] lines = WebService.ReadAllLinesIntoArray(Constants.PrismDataLogLocation, fileLocation);

			bool startReading = false;

			foreach (string line in lines)
			{
				if (line.Contains(ClassificationHeader))
				{
					startReading = true;
					continue;
				}

				if (!startReading)
				{
					continue;
				}

				string[] parts = line.Split(new[] { '*' }, StringSplitOptions.RemoveEmptyEntries);

				if (parts.Length >= 3)
				{
					results.Add(new ClassificationCodeSetting(parts[0].Trim(), parts[1].Trim(), parts[2].Trim()));
				}
				else
				{
					results.Add(new ClassificationCodeSetting());
				}

				if (results.Count == ClassificationCodeCount)
				{
					break;
				}
			}

			AddEmptyClassificationRows(results);

			return results;
		}

		public bool SaveClassificationCodes(IList<ClassificationCodeSetting> settings)
		{
			PrismProjectData projectData = GetProjectData();

			if (projectData == null || settings == null)
			{
				return false;
			}

			string fileLocation = Constants.ModelProjectInforLocation(projectData.ProjNumberAndGuid);

			for (int i = 0; i < ClassificationCodeCount; i++)
			{
				ClassificationCodeSetting setting = i < settings.Count
					? settings[i]
					: new ClassificationCodeSetting();

				string line =
					(setting.SelectionFilter ?? string.Empty) + "****" +
					(setting.Code ?? string.Empty) + "****" +
					(setting.Title ?? string.Empty);

				WebService.WriteToSpecificLine(Constants.PrismModelData, ClassificationCodeStartLine + i, line, fileLocation);
			}

			return true;
		}

		public ProjectUserSettings GetProjectUserSettings()
		{
			PrismProjectData projectData = GetProjectData();

			if (projectData == null)
			{
				return new ProjectUserSettings();
			}

			string fileLocation = Constants.ModelProjectInforLocation(projectData.ProjNumberAndGuid);

			return new ProjectUserSettings
			{
				ProjectManagement = WebService.ReadSpecificLine(Constants.PrismModelData, ProjectManagementLine, fileLocation),
				DrawingOfficeManager = WebService.ReadSpecificLine(Constants.PrismModelData, DrawingOfficeManagerLine, fileLocation),
				DocumentControl = WebService.ReadSpecificLine(Constants.PrismModelData, DocumentControlLine, fileLocation),
				Others = WebService.ReadSpecificLine(Constants.PrismModelData, OthersLine, fileLocation)
			};
		}

		public bool SaveProjectUserSettings(ProjectUserSettings settings)
		{
			PrismProjectData projectData = GetProjectData();

			if (projectData == null || settings == null)
			{
				return false;
			}

			string fileLocation = Constants.ModelProjectInforLocation(projectData.ProjNumberAndGuid);

			WebService.WriteToSpecificLine(Constants.PrismModelData, ProjectManagementLine, settings.ProjectManagement ?? string.Empty, fileLocation);
			WebService.WriteToSpecificLine(Constants.PrismModelData, DrawingOfficeManagerLine, settings.DrawingOfficeManager ?? string.Empty, fileLocation);
			WebService.WriteToSpecificLine(Constants.PrismModelData, DocumentControlLine, settings.DocumentControl ?? string.Empty, fileLocation);
			WebService.WriteToSpecificLine(Constants.PrismModelData, OthersLine, settings.Others ?? string.Empty, fileLocation);

			return true;
		}

		public int? GetCurrentPrelimStartPoint()
		{
			PrismProjectData projectData = GetProjectData();

			if (projectData == null)
			{
				return null;
			}

			object result = Logging.GetLastUsedPrelim(projectData.ProjNumberAndGuid);

			if (result is int prelimNumber)
			{
				return prelimNumber;
			}

			return null;
		}

		public bool SetPrelimStartPoint(int newStartPoint)
		{
			PrismProjectData projectData = GetProjectData();

			if (projectData == null)
			{
				return false;
			}

			int? previousStartPoint = GetCurrentPrelimStartPoint();

			if (!previousStartPoint.HasValue)
			{
				return false;
			}

			if (!Logging.SetLastUsedPrelim(projectData.ProjNumberAndGuid, newStartPoint))
			{
				return false;
			}

			Logging.LogProgress(
				projectData.ProjNumberAndName,
				"PRELIM RESET - before/after",
				newStartPoint,
				previousStartPoint.Value);

			return true;
		}

		public AdvancedPrismSettings GetAdvancedSettings()
		{
			PrismProjectData projectData = GetProjectData();

			if (projectData == null)
			{
				return new AdvancedPrismSettings();
			}

			AdvancedPrismSettings settings = new AdvancedPrismSettings
			{
				PrelimPrefix = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, Enums.AdvancedSettingType.PrelimPrefix),
				FabPackType = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, Enums.AdvancedSettingType.FabPackType),
				FabsecGreen = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, Enums.AdvancedSettingType.FabsecGreen)
			};

			if (string.IsNullOrWhiteSpace(settings.FabsecGreen))
			{
				settings.FabsecGreen = "100";
			}

			return settings;
		}

		public bool SaveAdvancedSettings(AdvancedPrismSettings settings)
		{
			PrismProjectData projectData = GetProjectData();

			if (projectData == null || settings == null)
			{
				return false;
			}

			WriteAdvancedSetting(projectData, Enums.AdvancedSettingType.PrelimPrefix, settings.PrelimPrefix);
			WriteAdvancedSetting(projectData, Enums.AdvancedSettingType.FabPackType, settings.FabPackType);
			WriteAdvancedSetting(projectData, Enums.AdvancedSettingType.FabsecGreen, settings.FabsecGreen);

			return true;
		}

		public PackageDirectorySettings GetPackageDirectorySettings()
		{
			PrismProjectData projectData = GetProjectData();

			if (projectData == null)
			{
				return new PackageDirectorySettings();
			}

			return new PackageDirectorySettings
			{
				Material = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, Enums.AdvancedSettingType.DirectoryMaterial),
				Carcasses = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, Enums.AdvancedSettingType.DirectoryCarcasses),
				Bolts = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, Enums.AdvancedSettingType.DirectoryBolts),
				Seversafe = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, Enums.AdvancedSettingType.DirectorySeversafe),
				FabPack = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, Enums.AdvancedSettingType.DirectoryFabPack),
				Variation = Logging.GetAdvancedSetting(projectData.ProjNumberAndGuid, Enums.AdvancedSettingType.DirectoryVariation)
			};
		}

		public bool SavePackageDirectorySettings(PackageDirectorySettings settings)
		{
			PrismProjectData projectData = GetProjectData();

			if (projectData == null || settings == null)
			{
				return false;
			}

			WriteAdvancedSetting(projectData, Enums.AdvancedSettingType.DirectoryMaterial, settings.Material);
			WriteAdvancedSetting(projectData, Enums.AdvancedSettingType.DirectoryCarcasses, settings.Carcasses);
			WriteAdvancedSetting(projectData, Enums.AdvancedSettingType.DirectoryBolts, settings.Bolts);
			WriteAdvancedSetting(projectData, Enums.AdvancedSettingType.DirectorySeversafe, settings.Seversafe);
			WriteAdvancedSetting(projectData, Enums.AdvancedSettingType.DirectoryFabPack, settings.FabPack);
			WriteAdvancedSetting(projectData, Enums.AdvancedSettingType.DirectoryVariation, settings.Variation);

			return true;
		}

		private PrismProjectData GetProjectData()
		{
			return _projectDataProvider();
		}

		private static void AddEmptyClassificationRows(List<ClassificationCodeSetting> results)
		{
			while (results.Count < ClassificationCodeCount)
			{
				results.Add(new ClassificationCodeSetting());
			}
		}

		private static void WriteAdvancedSetting(PrismProjectData projectData, Enums.AdvancedSettingType setting, string value)
		{
			WebService.WriteToSpecificLine(
				Constants.PrismDataLogLocation,
				(int)setting,
				setting + ":split: " + (value ?? string.Empty),
				Constants.ModelProjectAdvancedSettingLocation(projectData.ProjNumberAndGuid));
		}
	}
}