using Prism.ButtonOperations;
using Prism.ExternalService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tekla.Structures;

using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using static Prism.Enums;
using ModelObjectSelector = Tekla.Structures.Model.UI.ModelObjectSelector;
using Task = System.Threading.Tasks.Task;

namespace Prism
{
	public sealed class PrismWorkflowRunner
	{
		private Model _model;
		private PrismProjectData _projectData;
		private SelectedObjects _selectedObjects;
		private WebService1 _webService;
		private string _teklaVersion;

		public PrismProjectData ProjectData
		{
			get { return _projectData; }
		}

		public SelectedObjects SelectedObjects
		{
			get { return _selectedObjects; }
		}

		public Model Model
		{
			get { return _model; }
		}

		#region Initialisation

		public bool Initialise(Action<string> progress)
		{
			progress?.Invoke("Starting Prism...");

			string key = Prism.Properties.Settings.Default.UniqueId;

			progress?.Invoke("Connecting to Prism services...");
			_webService = WebService.SetupWebService(key);

			if (_webService == null)
			{
				return false;
			}

			progress?.Invoke("Validating user...");

			if (!_webService.ValidUser())
			{
				return false;
			}

			progress?.Invoke("Validating version...");
			Validation.VersionValidation.ValidateAppVersion(Constants.NewPrismLatestVersion, Constants.NewPrismLatestVersionTracking);

			progress?.Invoke("Connecting to Tekla...");
			_model = new Model();

			if (!_model.GetConnectionStatus())
			{
				return false;
			}

			progress?.Invoke("Reading project information...");
			_projectData = new PrismProjectData(_model.GetProjectInfo(), _model.GetInfo(), _webService);

			progress?.Invoke("Reading Tekla version...");
			_teklaVersion = ReadTeklaVersion();

			progress?.Invoke("Prism ready.");

			return true;
		}

		public async Task RunStartupLoggingAsync()
		{
			try
			{
				await Task.Run(() =>
				{
					Logging.LoginMessage(_projectData.ProjName, "Login Successful", Constants.NewPrismLoginLogLocation);
					Logging.CreateModelLog(_projectData);
				});
			}
			catch
			{
				// Startup logging should never prevent Prism being used.
			}
		}

		public async Task LogProgress(string modelName, string buttonPress, int autoFixCount, int totalObjects)
		{
			try
			{
				await Task.Run(() =>
				{
					Logging.LogProgress(modelName, buttonPress, autoFixCount, totalObjects, Constants.NewPrismUserUserLogLocation,
						Constants.NewPrismTotalUseLogLocation, Constants.NewPrismLogLocation);
				});
			}
			catch
			{
				// If logging fails the odd time it's not the end of the world.. 
			}
		}

		public async Task AddToMaterialOrderProcessedCount()
		{
			try
			{
				await Task.Run(() =>
				{
					Logging.AddToMaterialOrderProcessedCount(Constants.ModelProjectInforLocation(_projectData.ProjNumberAndGuid));

				});
			}
			catch { }
		}

		public async Task AddToFabCompleteCount()
		{
			try
			{
				await Task.Run(() =>
				{
					Logging.AddToFabCompleteCount(Constants.NewPrismTotalUseLogLocation);
				});
			}
			catch { }
		}

		public async Task UpdateFrozenDrawingCount(string modelName, int frozenDrawings, int unfrozenDrawings)
		{
			try
			{
				await Task.Run(() =>
				{
					Logging.UpdateFrozenDrawingCount(modelName, frozenDrawings, unfrozenDrawings, Constants.PrismModelData);
				});
			}
			catch { }
		}

		public async Task LogNcCreated(string modelName, string phaseNumber, string issueNumber, string ncLocation, int totalNcRequired, int totalNcCreated)
		{
			try
			{
				await Task.Run(() =>
				{
					Logging.NCCreated(modelName, phaseNumber, issueNumber, _teklaVersion, ncLocation, totalNcRequired, totalNcCreated, Constants.NewPrismNCPassed);
				});
			}
			catch { }
		}

		public async Task LogNcFailed(string modelName, string phaseNumber, string issueNumber, string ncLocation, int totalNcRequired, int totalNcCreated)
		{
			try
			{
				await Task.Run(() =>
				{
					Logging.NCFailed(modelName, phaseNumber, issueNumber, _teklaVersion, ncLocation, totalNcRequired, totalNcCreated, Constants.NewPrismNCFailed);
				});
			}
			catch { }
		}

		public async Task LogExceptionError(string modelName, Exception ex)
		{
			try
			{
				await Task.Run(() =>
				{
					Logging.ExceptionError(modelName, _teklaVersion, ex.Message, ex.StackTrace, Constants.NewPrismExceptions);
				});
			}
			catch { }
		}

		#endregion

		#region Material Workflows

		public async Task<bool> RunMaterial1Async(int startNumber, Action<int, string> progress)
		{
			progress?.Invoke(5, "Running initial material checks...");

			bool setupSuccessful = await Task.Run(() => InitialSetup(StageTypes.Prelim1, false, "x", "x", false, progress));

			if (!setupSuccessful)
			{
				return false;
			}

			progress?.Invoke(25, "Running material checks...");

			bool operationSuccessful = await Task.Run(() => _selectedObjects.MaterialCheckbutton(_projectData, startNumber, progress));

			if (!operationSuccessful)
			{
				return false;
			}

			_ = LogProgress(_projectData.ModelName, "Material 1", 0, _selectedObjects.PrismParts.Count);

			progress?.Invoke(98, "Updating Tekla views...");

			ModelModifiers.RedrawViews();

			return true;
		}

		public async Task<bool> RunPrepareFabsecMaterialAsync()
		{
			bool setupSuccessful = await Task.Run(() => InitialSetup(StageTypes.Prelim2, false, "x", "x", false));

			if (!setupSuccessful)
			{
				return false;
			}

			bool operationSuccessful = await Task.Run(() => _selectedObjects.PrepareFabsecs((int)StageTypes.Prelim2, _projectData, _model));

			if (!operationSuccessful)
			{
				return false;
			}

			_ = LogProgress(_projectData.ModelName, "Prepare Fabsec", 0, _selectedObjects.PrismParts.Count);

			ModelModifiers.RedrawViews();

			return true;
		}

		public async Task<bool> RunCreateFabsecCarcassAsync()
		{
			bool setupSuccessful = await Task.Run(() => InitialSetup(StageTypes.Prelim2, false, "x", "x", true));

			if (!setupSuccessful)
			{
				return false;
			}

			bool operationSuccessful = await Task.Run(() => _selectedObjects.CreateFabsecCarcasses(_projectData, _model, true));

			if (!operationSuccessful)
			{
				return false;
			}

			_ = LogProgress(_projectData.ModelName, "Create Fabsec Carcass", 0, _selectedObjects.PrismParts.Count);

			ModelModifiers.RedrawViews();

			return true;
		}

		public async Task<MaterialOrderRunResult> RunMaterialOrder(string orderCall, string typeOfOrder, string phaseNumber, string issueNumber,
			string variationNumber, string variationType, string matSiteDate, Action<int, string> progress)
		{
			progress?.Invoke(5, "Running initial material order checks...");

			try
			{
				StageTypes stageType = StageTypes.Prelim3;

				progress?.Invoke(25, "Preparing material order...");

				string orderType = $"{orderCall} {typeOfOrder}";

				progress?.Invoke(30, "Preparing material reports...");

				ReportManager reportManager = new ReportManager(_projectData, phaseNumber, issueNumber);

				progress?.Invoke(35, "Checking variation details...");

				ModelModifiers.VariationCheck(_projectData, variationNumber, variationType);

				if (orderType.Contains("Special Fittings"))
				{
					bool setupSuccessful = await Task.Run(() => InitialSetup(stageType, false, "x", "x", false, progress));

					if (!setupSuccessful)
					{
						return MaterialOrderRunResult.InitialSetupFailed;
					}

					progress?.Invoke(40, "Processing special fittings...");

					bool specialFittingsSuccessful = await Task.Run(() => ProcessSpecialFittings(reportManager, orderType, matSiteDate, progress));

					if (!specialFittingsSuccessful)
					{
						return MaterialOrderRunResult.OperationFailed;
					}
				}
				else
				{
					bool checkPreviousStep = orderType.Contains("Material");

					bool setupSuccessful = await Task.Run(() => InitialSetup(stageType, checkPreviousStep, "x", "x", true, progress));

					if (!setupSuccessful)
					{
						return MaterialOrderRunResult.InitialSetupFailed;
					}

					progress?.Invoke(55, "Creating material order...");

					bool operationSuccessful = await Task.Run(() => _selectedObjects.MaterialButton3op(_projectData, reportManager, _teklaVersion,
							orderType, StageTypes.Prelim3, _model, matSiteDate, progress));

					if (!operationSuccessful)
					{
						return MaterialOrderRunResult.OperationFailed;
					}

					progress?.Invoke(75, "Committing model changes...");

					_model.CommitChanges();
				}

				progress?.Invoke(82, "Preparing material order files...");

				AdvancedSettingType setting;
				string materialFolder = MaterialFolderType(orderType, reportManager.Folders, out setting);

				progress?.Invoke(88, "Moving material order files...");

				MovePackToDirectory(materialFolder, setting, phaseNumber);

				progress?.Invoke(95, "Finalising material order...");

				_ = AddToMaterialOrderProcessedCount();

				_ = LogProgress(_projectData.ModelName, "Prepare Fabsec", 0, _selectedObjects.PrismParts.Count);

				return MaterialOrderRunResult.Success;
			}
			catch (Exception)
			{
				return MaterialOrderRunResult.Exception;
			}
		}

		#endregion

		#region Drawing Workflows

		public async Task<DrawingCheckRunResult> RunDrawingChecksAsync(Action<int, string> progress)
		{
			progress?.Invoke(10, "Running initial drawing checks...");

			StageTypes stageType = StageTypes.Check1;

			bool setupSuccessful = await Task.Run(() => InitialSetup(stageType, false, "x", "x", false, progress));

			if (!setupSuccessful)
			{
				return DrawingCheckRunResult.InitialSetupFailed;
			}

			progress?.Invoke(25, "Running drawing checks - stage 1...");

			bool stageOnePassed = await Task.Run(() => _selectedObjects.RunDrawingStageOneChecks(progress));

			if (!stageOnePassed)
			{
				return DrawingCheckRunResult.StageOneFailed;
			}

			progress?.Invoke(60, "Running drawing checks - stage 2...");

			bool stageTwoPassed = await Task.Run(() => _selectedObjects.RunDrawingStageTwoChecks(progress));

			if (!stageTwoPassed)
			{
				return DrawingCheckRunResult.StageTwoFailed;
			}

			if (!_selectedObjects.PrismParts.ModifyAttributes(stageType, _projectData, progress, null)) return DrawingCheckRunResult.InitialSetupFailed;
			if (!_selectedObjects.PrismParts.ModifyAttributes(StageTypes.Check2, _projectData, progress, null)) return DrawingCheckRunResult.InitialSetupFailed;

			progress?.Invoke(95, "Finalising drawing checks...");

			_ = LogProgress(_projectData.ModelName, "Detail 1", 0, _selectedObjects.PrismParts.Count);

			ModelModifiers.RedrawViews();

			return DrawingCheckRunResult.Success;
		}

		public async Task<bool> RunDetailOrientationHolesAsync(string detailType, int? pltOnFlange)
		{
			bool setupSuccessful = await Task.Run(() => InitialSetup(StageTypes.Check1, false, "x", "x", false));

			if (!setupSuccessful)
			{
				return false;
			}

			int columnCount = 0;

			await Task.Run(() =>
			{
				columnCount = ColumnOrientation.DetailColumnOrientationHoles(_selectedObjects, detailType,
					pltOnFlange.HasValue ? pltOnFlange.Value.ToString() : string.Empty, true);
			});

			ModelModifiers.RedrawViews();

			_ = LogProgress(_projectData.ModelName, "Detail Orientation Holes", 0, columnCount);

			return true;
		}

		public async Task<DrawingCheckRunResult> RunCreateDrawingsAsync(Action<int, string> progress)
		{
			progress?.Invoke(10, "Running initial drawing checks...");

			bool setupSuccessful = await Task.Run(() => InitialSetup(StageTypes.Check3, true, "x", "x", false, progress));

			if (!setupSuccessful)
			{
				return DrawingCheckRunResult.InitialSetupFailed;
			}

			progress?.Invoke(25, "Running drawing checks - stage 1...");

			bool stageOnePassed = await Task.Run(() => _selectedObjects.RunDrawingStageOneChecks(progress));

			if (!stageOnePassed)
			{
				return DrawingCheckRunResult.StageOneFailed;
			}

			progress?.Invoke(60, "Running drawing checks - stage 2...");

			bool stageTwoPassed = await Task.Run(() => _selectedObjects.RunDrawingStageTwoChecks(progress));

			if (!stageTwoPassed)
			{
				return DrawingCheckRunResult.StageTwoFailed;
			}

			progress?.Invoke(90, "Checking shear stud order status...");
			ModelChecker.ConfirmShearStudOrder(_projectData.Full, _selectedObjects.PrismBoltGroups, false);

			progress?.Invoke(92, "Creating drawings...");

			await Task.Run(() =>
			{
				_selectedObjects.CreateMyDrawings(StageTypes.Check3, _projectData, progress);
			});

			_ = LogProgress(_projectData.ModelName, "Create Drawings", 0, _selectedObjects.PrismParts.Count);

			progress?.Invoke(98, "Updating Tekla views...");

			ModelModifiers.RedrawViews();

			return DrawingCheckRunResult.Success;
		}

		#endregion

		#region Material Order Helpers

		private async Task<bool> ProcessSpecialFittings(ReportManager reportManager, string orderType, string matSiteDate, Action<int, string> progress)
		{
			if (orderType.Contains("Omit"))
			{
				if (!reportManager.Folders.CreateMatFolder(false))
				{
					return false;
				}

				reportManager.CreateMaterialReports(_selectedObjects, orderType, StageTypes.Prelim3);

				if (!MaterialButton3.FinishOrder(_selectedObjects, StageTypes.Prelim3, _projectData, reportManager.MatReportPrefix, orderType, reportManager, matSiteDate, progress))
				{
					return false;
				}

				return true;
			}

			if (!await OrderSpecials(orderType, StageTypes.Prelim3, reportManager, progress))
			{
				return false;
			}

			if (!MaterialButton3.FinishOrder(_selectedObjects, StageTypes.Prelim3, _projectData, reportManager.MatReportPrefix, orderType, reportManager, matSiteDate, progress, true))
			{
				return false;
			}

			return true;
		}

		private async Task<bool> OrderSpecials(string orderType, StageTypes stageType, ReportManager reportManager, Action<int, string> progress)
		{
			if (!reportManager.Folders.CreateMatFolder(false, true))
			{
				return false;
			}

			List<PrismPart> taggedSpecialFittings = ModelModifiers.SelectSpecialTaggedInSelection(_selectedObjects);

			if (taggedSpecialFittings.Count == 0)
			{
				//then the user has selected to tagged speical fittings.
				return false;
			}

			if (!ModelModifiers.AddPrelimMarks(_selectedObjects, _projectData))
			{
				return false;
			}

			reportManager.CreateMaterialReports(_selectedObjects, orderType, stageType);

			DrawingManager drawingManager = await DrawingManager.CreateAsync(_model, _projectData, _selectedObjects.PrismParts, reportManager.PhaseNum, reportManager.IssueNum, reportManager.Folders.MatPath, progress);

			if (drawingManager == null)
			{
				return false;
			}

			if (drawingManager.GetDrawingFolder(Enums.DrawingFolder.Default).Count != 0)
			{
				PrismWarnings.IncorrectlyAssignedDrawings();
				return false;
			}

			List<int> drawingCount = new List<int> { 0, drawingManager.GetDrawingByType("W").Count };

			DrawingManager.PrintAndIssueDrawings(reportManager.Folders.MatFolder, reportManager.Folders.MatPath, drawingCount, "\\SPC", 0, 1, reportManager, false, _teklaVersion);

			PrismMacroBuilder.ClearPrintDialog();

			return true;
		}

		private string MaterialFolderType(string orderType, FolderManager folders, out AdvancedSettingType setting)
		{
			if (orderType.Contains("Material") || orderType.Contains("Special"))
			{
				setting = AdvancedSettingType.DirectoryMaterial;
				return folders.MatPath;
			}

			if (orderType.Contains("Carcass"))
			{
				setting = AdvancedSettingType.DirectoryCarcasses;
				return folders.FabsecCarcassPath;
			}

			if (orderType.Contains("Bolt"))
			{
				setting = AdvancedSettingType.DirectoryBolts;
				return folders.BoltPath;
			}

			if (orderType.Contains("Seversafe"))
			{
				setting = AdvancedSettingType.DirectorySeversafe;
				return folders.EpoPath;
			}

			setting = AdvancedSettingType.Default;
			return string.Empty;
		}

		#endregion

		#region Directory Helpers

		private bool MovePackToDirectory(string sourcePath, AdvancedSettingType settingType, string phaseNumber)
		{
			string directory = Logging.GetAdvancedSetting(_projectData.ProjNumberAndGuid, settingType);
			string variationDirectory = string.Empty;

			if (_projectData.IsVariation)
			{
				variationDirectory = Logging.GetAdvancedSetting(_projectData.ProjNumberAndGuid, AdvancedSettingType.DirectoryVariation);
			}

			if (directory == string.Empty && variationDirectory == string.Empty)
			{
				return true;
			}

			string variationWithSubFolder = VariationSubFolderName(variationDirectory, settingType);

			if (!PrismWarnings.MoveToFabDirectory(directory, variationWithSubFolder, _projectData.IsVariation))
			{
				return false;
			}

			return CopyAndDeleteDirectory(sourcePath, directory, variationWithSubFolder, phaseNumber);
		}

		public async Task<FabPackCheckRunResult> RunFabPackChecksAsync(Action<int, string> progress, string phaseNumber, string issueNumber)
		{
			progress?.Invoke(10, "Running initial Fab Pack checks...");

			bool setupSuccessful = await Task.Run(() => InitialSetup(StageTypes.FAB, true, phaseNumber, issueNumber, false, progress));

			if (!setupSuccessful)
			{
				return FabPackCheckRunResult.InitialSetupFailed;
			}

			progress?.Invoke(55, "Checking drawing information...");

			bool drawingChecksPassed = await Task.Run(() => RunFabPackDrawingChecks(_selectedObjects));

			if (!drawingChecksPassed)
			{
				return FabPackCheckRunResult.DrawingChecksFailed;
			}

			progress?.Invoke(75, "Checking shear stud order status...");
			if (!ModelChecker.ConfirmShearStudOrder(_projectData.Full, _selectedObjects.PrismBoltGroups, true))
			{
				return FabPackCheckRunResult.ChecksFailed;
			}

			progress?.Invoke(90, "Recording Fab Pack check completion...");
			if (!ModelModifiers.StampFabCheckComplete(_selectedObjects.PrismParts, _projectData))
			{
				return FabPackCheckRunResult.ChecksFailed;
			}

			_model.CommitChanges();
			_ = LogProgress(_projectData.ModelName, "Fab Checks", 0, _selectedObjects.PrismParts.Count);

			progress?.Invoke(95, "Fab Pack checks complete.");

			return FabPackCheckRunResult.Success;
		}

		public static bool RunFabPackDrawingChecks(SelectedObjects selectedObjects)
		{
			foreach (PrismPart part in selectedObjects.PrismParts)
			{
				if (!part.HasProperlyAssignedFolder ||
					!part.HasRevision ||
					!part.HasDrawing)
				{
					return false;
				}
			}

			return true;
		}

		private string VariationSubFolderName(string variation, AdvancedSettingType setting)
		{
			if (variation == string.Empty)
			{
				return variation;
			}

			if (setting == AdvancedSettingType.DirectoryMaterial)
			{
				variation = Path.Combine(variation, "Material");
			}
			else if (setting == AdvancedSettingType.DirectoryBolts)
			{
				variation = Path.Combine(variation, "Bolt Orders");
			}
			else if (setting == AdvancedSettingType.DirectoryCarcasses)
			{
				variation = Path.Combine(variation, "Fabsec Carcasses");
			}
			else if (setting == AdvancedSettingType.DirectorySeversafe)
			{
				variation = Path.Combine(variation, "Seversafe Orders");
			}
			else if (setting == AdvancedSettingType.DirectoryFabPack)
			{
				variation = Path.Combine(variation, "Fabrication Packages");
			}

			if (!Directory.Exists(variation))
			{
				Directory.CreateDirectory(variation);
			}

			return variation;
		}

		public bool CopyAndDeleteDirectory(string sourceDir, string destDir, string secondDestDir, string phaseNumber)
		{
			try
			{
				if (!Directory.Exists(sourceDir))
				{
					PrismWarnings.DirectoryCannotBeReached(sourceDir);
					return false;
				}

				if (!RunCopyLogicForGivenDirectory(sourceDir, destDir, phaseNumber))
				{
					return false;
				}

				if (!RunCopyLogicForGivenDirectory(sourceDir, secondDestDir, phaseNumber))
				{
					return false;
				}

				Directory.Delete(sourceDir, true);
				return true;
			}
			catch (Exception ex)
			{
				PrismWarnings.DirectoryCannotBeCopied(ex.Message);
				return false;
			}
		}

		private bool RunCopyLogicForGivenDirectory(string sourceDir, string destDir, string phaseNumber)
		{
			if (destDir == string.Empty)
			{
				return true;
			}

			if (!Directory.Exists(destDir))
			{
				PrismWarnings.DirectoryCannotBeReached(destDir);
				return false;
			}

			destDir = Path.Combine(destDir, "Phase " + phaseNumber);

			string newDestDir = Path.Combine(destDir, Path.GetFileName(sourceDir));

			CopyDirectory(sourceDir, newDestDir);

			return true;
		}

		private void CopyDirectory(string sourceDir, string destDir)
		{
			Directory.CreateDirectory(destDir);

			foreach (string file in Directory.GetFiles(sourceDir))
			{
				string destFile = Path.Combine(destDir, Path.GetFileName(file));
				File.Copy(file, destFile, true);
			}

			foreach (string directory in Directory.GetDirectories(sourceDir))
			{
				string destDirectory = Path.Combine(destDir, Path.GetFileName(directory));
				CopyDirectory(directory, destDirectory);
			}
		}

		#endregion

		#region Shared Setup

		private bool InitialSetup(StageTypes stageType, bool checkForPreviousSteps, string phaseNumber, string issueNumber,
			bool checkFabsecsHaveBeenPrepped, Action<int, string> progress = null)
		{
			ModelModifiers.ResetWorkPlane(_model);

			progress?.Invoke(5, "Preparing selected parts...");

			ModelChecker.ClearOldLists();

			if (stageType != StageTypes.RocketPacket)
			{
				progress?.Invoke(8, "Reading selected model objects...");

				_selectedObjects = new SelectedObjects(_projectData.ProjPath, stageType, phaseNumber, issueNumber, _model, Constants.SpecialOperationUser(), progress: progress);
			}

			progress?.Invoke(12, "Checking parts are selected...");

			if (!PartsSelected())
			{
				return false;
			}

			progress?.Invoke(15, "Checking numbering...");
			bool numberingUpToDate = NumberingIsUpToDate(stageType);

			progress?.Invoke(18, "Checking previous workflow steps...");
			bool previousStepsComplete = PreviousStepsComplete(checkForPreviousSteps, stageType);

			progress?.Invoke(21, "Checking for locked parts...");
			bool partsUnlocked = AnyPartsLocked();

			progress?.Invoke(24, "Checking Fabsec preparation...");
			bool fabsecPreparationPassed = FabsecPreparationCheckPassed(checkFabsecsHaveBeenPrepped, _selectedObjects);

			return numberingUpToDate && previousStepsComplete && partsUnlocked && fabsecPreparationPassed;
		}

		private bool PartsSelected()
		{
			if (_selectedObjects == null)
			{
				return false;
			}

			if (_selectedObjects.PrismParts.Count == 0)
			{
				PrismWarnings.NoPartsSelected();
				return false;
			}

			return true;
		}

		private bool NumberingIsUpToDate(StageTypes stageType)
		{
			bool requiresCurrentNumbers = stageType == StageTypes.FAB || stageType == StageTypes.RocketPacket;

			if (!requiresCurrentNumbers)
			{
				return true;
			}

			if (_selectedObjects.GetPartsWithOutOfDateNumbers().Count != 0)
			{
				PrismWarnings.NumberingIsNotUpToDate();
				return false;
			}

			return true;
		}

		public void SelectAndZoomToPart(string guid)
		{
			ModelObject modelObject = GetModelObjectByGuid(guid);

			if (modelObject == null)
			{
				return;
			}

			ArrayList objectsToSelect = new ArrayList
			{
				modelObject
			};

			ModelObjectSelector selector = new ModelObjectSelector();
			selector.Select(objectsToSelect);

			Part part = modelObject as Part;

			if (part == null)
			{
				return;
			}

			Solid solid = part.GetSolid();
			AABB boundingBox = new AABB(solid.MinimumPoint, solid.MaximumPoint);

			ViewHandler.ZoomToBoundingBox(boundingBox);
		}

		private ModelObject GetModelObjectByGuid(string guid)
		{
			Identifier identifier = _model.GetIdentifierByGUID(guid);

			if (identifier == null)
			{
				return null;
			}

			return _model.SelectModelObject(identifier);
		}

		public void ReselectOriginalParts()
		{
			if (_selectedObjects == null ||
				_selectedObjects.PrismParts == null ||
				_selectedObjects.PrismParts.Count == 0)
			{
				return;
			}

			ArrayList objectsToSelect = new ArrayList(
				_selectedObjects.PrismParts
					.Select(part => part.ModelObject)
					.ToList());

			ModelObjectSelector selector = new ModelObjectSelector();
			selector.Select(objectsToSelect);
		}


		private bool PreviousStepsComplete(bool checkForPreviousSteps, StageTypes stageType)
		{
			if (!checkForPreviousSteps)
			{
				return true;
			}

			return ModelChecker.ArePreviousStepsComplete(_selectedObjects, (int)stageType, true);
		}

		private bool AnyPartsLocked()
		{
			List<PrismPart> lockedParts = _selectedObjects.GetLockedParts();

			return lockedParts.Count == 0;
		}

		private bool FabsecPreparationCheckPassed(bool checkFabsecsHaveBeenPrepped, SelectedObjects selectedObjects)
		{
			if (!checkFabsecsHaveBeenPrepped)
			{
				return true;
			}

			List<PrismPart> unprocessedFabsecs = selectedObjects.GetFabsecParts()
				.Where(part =>
				{
					string userProperty = string.Empty;
					part.Part.GetUserProperty(ModelUDA.FabsecNote(), ref userProperty);
					return userProperty == string.Empty;
				}).ToList();

			if (!unprocessedFabsecs.Any())
			{
				return true;
			}

			foreach (PrismPart part in unprocessedFabsecs)
			{
				if (!part.PartErrors.Contains(Error.FabsecNotProcessed))
				{
					part.PartErrors.Add(Error.FabsecNotProcessed);
				}
			}

			return false;
		}

		#endregion

		public async Task<FabPackRunResult> RunCreateFabPackAsync(string phaseNumber, string issueNumber, string siteDate, string variationType,
			string variationNumber, Action<int, string> progress)
		{
			Stopwatch totalTimer = Stopwatch.StartNew();
			Stopwatch stageTimer = new Stopwatch();
			Dictionary<string, double> timings = new Dictionary<string, double>();

			Action<string, double> recordTiming = delegate (string name, double milliseconds)
			{
				double existing;
				timings.TryGetValue(name, out existing);
				timings[name] = existing + milliseconds;
			};

			progress?.Invoke(1, "Preparing Fab Pack...");

			stageTimer.Restart();
			ModelModifiers.VariationCheck(_projectData, variationNumber, variationType);
			stageTimer.Stop();
			recordTiming("VariationCheck", stageTimer.Elapsed.TotalMilliseconds);

			progress?.Invoke(3, "Running initial Fab Pack checks...");

			stageTimer.Restart();
			bool setupSuccessful = await Task.Run(() => InitialSetup(StageTypes.FAB, true, phaseNumber, issueNumber, false, progress));
			stageTimer.Stop();
			recordTiming("InitialSetup", stageTimer.Elapsed.TotalMilliseconds);

			if (!setupSuccessful)
			{
				totalTimer.Stop();
				WriteFabPackPerformanceLog(phaseNumber, issueNumber, "InitialSetupFailed", timings, totalTimer.Elapsed.TotalMilliseconds);
				return FabPackRunResult.InitialSetupFailed;
			}

			progress?.Invoke(22, "Checking Fab Pack checks are complete...");
			if (!ModelChecker.AreFabPackChecksComplete(_selectedObjects))
			{
				totalTimer.Stop();
				WriteFabPackPerformanceLog(phaseNumber, issueNumber, "FabChecksIncomplete", timings, totalTimer.Elapsed.TotalMilliseconds);
				return FabPackRunResult.InitialSetupFailed;
			}

			progress?.Invoke(25, "Checking drawing information...");

			stageTimer.Restart();
			bool drawingChecksPassed = await Task.Run(() => RunFabPackDrawingChecks(_selectedObjects));
			stageTimer.Stop();
			recordTiming("DrawingChecks", stageTimer.Elapsed.TotalMilliseconds);

			if (!drawingChecksPassed)
			{
				totalTimer.Stop();
				WriteFabPackPerformanceLog(phaseNumber, issueNumber, "DrawingChecksFailed", timings, totalTimer.Elapsed.TotalMilliseconds);
				return FabPackRunResult.DrawingChecksFailed;
			}

			progress?.Invoke(30, "Preparing Fab Pack options...");
			await Task.Yield();

			bool runSeversafe = false;
			if (_selectedObjects.SeversafePresent)
			{
				stageTimer.Restart();
				runSeversafe = PrismWarnings.ShowTopmostYesNoMessage("You have Seversafe in your selection, would you like to create an order for this?", "Order Seversafe");
				stageTimer.Stop();
				recordTiming("SeversafeUserWait", stageTimer.Elapsed.TotalMilliseconds);
			}

			progress?.Invoke(33, "Creating Fab Pack...");
			await Task.Yield();

			ReportManager myReportManager = new ReportManager(_projectData, phaseNumber, issueNumber);
			Action<int, string> packageProgress = CreateProgressRange(progress, 33, 87);

			stageTimer.Restart();
			var (success, drawingManager) = await _selectedObjects.CreateFabPackage(_model, _projectData, phaseNumber, issueNumber, StageTypes.FAB,
				siteDate, runSeversafe, _teklaVersion, packageProgress, recordTiming);
			stageTimer.Stop();
			recordTiming("CreateFabPackageTotal", stageTimer.Elapsed.TotalMilliseconds);

			if (!success)
			{
				totalTimer.Stop();
				WriteFabPackPerformanceLog(phaseNumber, issueNumber, "OperationFailed", timings, totalTimer.Elapsed.TotalMilliseconds);
				return FabPackRunResult.OperationFailed;
			}

			progress?.Invoke(87, "Completing Fab Pack operations...");

			bool boltOrderAdded = false;

			stageTimer.Restart();
			await Task.Run(() => FabMisc.FabMiscOp(_model, siteDate, _selectedObjects, myReportManager, runSeversafe,
				_projectData, false, out boltOrderAdded));
			stageTimer.Stop();
			recordTiming("FabMisc", stageTimer.Elapsed.TotalMilliseconds);

			progress?.Invoke(93, "Checking NC data...");

			stageTimer.Restart();
			bool ncCreated = CheckNcCreation(myReportManager, drawingManager.NumberOfNcRequired, out int numberOfFilesCreated, out HashSet<string> uniqueFiles);

			HashSet<string> filesNotInComparisonFolder = null;
			if (!ncCreated)
			{
				HashSet<string> comparisonFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				AddFilesFromFolder(myReportManager.Folders.NcPath, comparisonFiles);
				filesNotInComparisonFolder = new HashSet<string>(uniqueFiles.Except(comparisonFiles), StringComparer.OrdinalIgnoreCase);
			}

			stageTimer.Stop();
			recordTiming("NCValidation", stageTimer.Elapsed.TotalMilliseconds);

			if (!ncCreated)
			{
				stageTimer.Restart();
				PrismWarnings.NcDataCreationFailed(filesNotInComparisonFolder.ToList());
				stageTimer.Stop();
				recordTiming("NCUserWait", stageTimer.Elapsed.TotalMilliseconds);

				_ = LogNcFailed(_projectData.ProjNumberAndName, phaseNumber, issueNumber, myReportManager.Folders.NcPath,
					drawingManager.NumberOfNcRequired, numberOfFilesCreated);
			}
			else
			{
				_ = LogNcCreated(_projectData.ProjNumberAndName, phaseNumber, issueNumber, myReportManager.Folders.NcPath,
					drawingManager.NumberOfNcRequired, numberOfFilesCreated);
			}

			progress?.Invoke(96, "Moving Fab Pack to project directories...");

			stageTimer.Restart();
			MovePackToDirectory(myReportManager.Folders.FabPath, AdvancedSettingType.DirectoryFabPack, phaseNumber);

			if (boltOrderAdded)
			{
				progress?.Invoke(98, "Moving bolt order to project directories...");
				MovePackToDirectory(myReportManager.Folders.BoltPath, AdvancedSettingType.DirectoryBolts, phaseNumber);
			}
			stageTimer.Stop();
			recordTiming("MovePackages", stageTimer.Elapsed.TotalMilliseconds);

			progress?.Invoke(99, "Finalising Fab Pack...");

			stageTimer.Restart();
			_ = UpdateFrozenDrawingCount(_projectData.ProjNumberAndGuid, drawingManager.GetFrozenDrawings().Count, drawingManager.GetUnFrozenDrawings().Count);
			_ = LogProgress(_projectData.ProjNumberAndName, "Fabrication", 0, _selectedObjects.GetMainParts().Count);
			_ = AddToFabCompleteCount();
			stageTimer.Stop();
			recordTiming("FinalLogging", stageTimer.Elapsed.TotalMilliseconds);

			progress?.Invoke(100, "Fab Pack created successfully.");

			totalTimer.Stop();
			WriteFabPackPerformanceLog(phaseNumber, issueNumber, "Success", timings, totalTimer.Elapsed.TotalMilliseconds);

			return FabPackRunResult.Success;
		}

		private void WriteFabPackPerformanceLog(string phaseNumber, string issueNumber, string result, Dictionary<string, double> timings, double totalMs)
		{
			try
			{
				string logPath = Path.Combine(_projectData.ProjPath, "FabPack_Performance.log");
				string[] orderedStages =
				{
					"VariationCheck", "InitialSetup", "DrawingChecks", "SeversafeUserWait",
					"FolderCpuCounter", "FolderReportManager", "FolderFab", "FolderBolt", "FolderSeversafe", "FolderSeversafeSelection", "FolderUserWait", "Folders",
					"DrawingReport", "DrawingReportRead", "DrawingValidation", "DrawingNcCount", "DrawingUserWait", "DrawingManager",
					"QRPreparation", "Printing", "PdfQR", "BSWX", "ReportPreparation", "TeklaReports", "NCSecondarySelection", "NCSecondaryPlates", "NCSecondaryProfiles",
					"NCMainSelection", "NCMainHollow", "NCMainProfiles", "NC2021Plates", "NC2021Profiles", "NCWait", "ReportPDF", "RestorePartSelection", "ReportsNC",
					"ModifyAttributes", "IndividualIFC", "RemoveUnusedFolders", "Zip",
					"FabPackCompleteUserWait", "EmailBuild", "EmailOutlookStart", "EmailPrepare", "EmailUserWait", "EmailWall",
					"CreateFabPackageTotal", "FabMisc", "NCValidation", "NCUserWait", "MovePackages", "FinalLogging"
				};

				string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +
					" | Phase=" + phaseNumber +
					" | Issue=" + issueNumber +
					" | Result=" + result +
					" | Parts=" + (_selectedObjects == null ? 0 : _selectedObjects.PrismParts.Count) +
					" | Bolts=" + (_selectedObjects == null ? 0 : _selectedObjects.PrismBoltGroups.Count);

				HashSet<string> writtenStages = new HashSet<string>(StringComparer.Ordinal);

				foreach (string stage in orderedStages)
				{
					double milliseconds;
					if (timings.TryGetValue(stage, out milliseconds))
					{
						line += " | " + stage + "=" + milliseconds.ToString("0.0") + "ms";
						writtenStages.Add(stage);
					}
				}

				foreach (KeyValuePair<string, double> timing in timings.Where(item => !writtenStages.Contains(item.Key)).OrderBy(item => item.Key))
				{
					line += " | " + timing.Key + "=" + timing.Value.ToString("0.0") + "ms";
				}

				double userWaitMs = timings.Where(item => item.Key.EndsWith("UserWait", StringComparison.Ordinal)).Sum(item => item.Value);
				double processingMs = Math.Max(0, totalMs - userWaitMs);

				line += " | ProcessingTotal=" + processingMs.ToString("0.0") + "ms" +
					" | UserWaitTotal=" + userWaitMs.ToString("0.0") + "ms" +
					" | WallTotal=" + totalMs.ToString("0.0") + "ms" + Environment.NewLine;

				File.AppendAllText(logPath, line);
			}
			catch
			{
				// Performance logging must never interrupt Fab Pack creation.
			}
		}

		private bool CheckNcCreation(ReportManager reportManager, int totalNcRequired, out int numberOfFilesCreated, out HashSet<string> uniqueFiles)
		{
			// Get the location of the folder to search
			string fabPackageLocation = reportManager.Folders.FabPath;

			// Path of the file to check
			string fileToCheck = fabPackageLocation + "\\NC";

			numberOfFilesCreated = Directory.Exists(fileToCheck) ? Directory.GetFiles(fileToCheck).Length : 0;

			// Get files from the folders
			uniqueFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			AddFilesFromFolder(reportManager.Folders.FabPath + "\\ASS", uniqueFiles);
			AddFilesFromFolder(reportManager.Folders.FabPath + "\\PRT", uniqueFiles);
			AddFilesFromFolder(reportManager.Folders.FabPath + "\\FIT", uniqueFiles);


			// Count the unique files
			return uniqueFiles.Count == numberOfFilesCreated;
		}

		private static Action<int, string> CreateProgressRange(Action<int, string> progress, int startPercentage, int endPercentage)
		{
			if (progress == null) return null;

			return (percentage, message) =>
			{
				int clampedPercentage = Math.Max(0, Math.Min(100, percentage));
				int mappedPercentage = startPercentage + (int)Math.Round((endPercentage - startPercentage) * (clampedPercentage / 100.0));

				progress(mappedPercentage, message);
			};
		}

		static void AddFilesFromFolder(string folderPath, HashSet<string> uniqueFiles)
		{
			if (Directory.Exists(folderPath))
			{
				var files = Directory.GetFiles(folderPath);
				foreach (var file in files)
				{
					string fileName = Path.GetFileNameWithoutExtension(file);

					// Split by dash and take the first part, then remove any remaining file extension
					string baseFileName = fileName.Contains('-') ? fileName.Split('-')[0].TrimEnd() : fileName.Split('.')[0].TrimEnd();
					uniqueFiles.Add(baseFileName);
				}
			}
			else
			{
				Console.WriteLine($"Folder does not exist: {folderPath}");
			}
		}

		private async Task<bool> RunAutoCompleteAsync(Error error, Action<List<PrismPart>> autoFix)
		{
			if (_selectedObjects == null || _selectedObjects.PrismParts == null)
			{
				return false;
			}

			List<PrismPart> partsToFix = _selectedObjects.PrismParts
				.Where(part => part.PartErrors != null && part.PartErrors.Contains(error))
				.ToList();

			if (partsToFix.Count == 0)
			{
				return true;
			}

			await Task.Run(() => autoFix(partsToFix));

			_ = LogProgress(_projectData.ModelName, "Auto-Fix-" + error.ToString(), partsToFix.Count, SelectedObjects.PrismParts.Count);
			ModelModifiers.RedrawViews();

			return true;
		}

		public Task<bool> RunAutoCompleteNameAndClassAsync()
		{
			return RunAutoCompleteAsync(Error.NameAndClass, AutoFix.PartNameAndClass);
		}

		public Task<bool> RunAutoCompleteExecutionClassAsync(int executionClass)
		{
			return RunAutoCompleteAsync(
				Error.Execution,
				parts => AutoFix.ExecutionClass(parts, executionClass));
		}

		public Task<bool> RunAutoCompleteOrientationAsync()
		{
			return RunAutoCompleteAsync(Error.Orientation, AutoFix.MemberOrientation);
		}

		public Task<bool> RunAutoCompleteFittingStartNumberMismatchAsync()
		{
			return RunAutoCompleteAsync(Error.SecondaryNumberingMismatch, AutoFix.AssemblyToPartStartNumber);
		}

		public Task<bool> RunAutoCompleteFittingPhaseNumberMismatchAsync()
		{
			return RunAutoCompleteAsync(Error.SecondaryPhasingMismatch, AutoFix.AssemblyToPartPhaseNumber);
		}

		#region General Helpers

		private static string ReadTeklaVersion()
		{
			string version = "2021.0";

			try
			{
				System.Reflection.AssemblyName name = System.Reflection.Assembly.GetCallingAssembly().GetName();

				using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Severfield\\" + name.Name + "\\"))
				{
					if (key == null)
					{
						return version;
					}

					object storedVersion = key.GetValue("TeklaVersion");

					if (storedVersion != null)
					{
						version = storedVersion as string ?? version;
					}
				}
			}
			catch
			{
				// Retain the default version.
			}

			return version;
		}

		#endregion
	}
}