using Prism;
using PrismNewInterface.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tekla.Structures.Model;
using static Prism.Enums;
using Task = System.Threading.Tasks.Task;

namespace PrismNewInterface.Services
{
	/// <summary>
	/// Connects the WPF interface to the main Prism functionality.
	/// Runs the different Prism workflows and returns their results back to the interface.
	/// </summary>
	public sealed class PrismOperations
	{
		private readonly PrismWorkflowRunner _workflowRunner;
		private readonly PrismSettingsService _settingsService;

		public PrismOperations()
		{
			_workflowRunner = new PrismWorkflowRunner();
			_settingsService = new PrismSettingsService(() => _workflowRunner.ProjectData);
		}

		#region Startup and Model Access

		public bool Initialise(Action<string> startupProgress)
		{
			return _workflowRunner.Initialise(startupProgress);
		}

		public Model Model
		{
			get { return _workflowRunner.Model; }
		}

		public PrismProjectData ProjectData
		{
			get { return _workflowRunner.ProjectData; }
		}

		public Task RunStartupLoggingAsync()
		{
			return _workflowRunner.RunStartupLoggingAsync();
		}

		private Task LogException(Exception ex)
		{
			return _workflowRunner.LogExceptionError(ProjectData.ModelName, ex);
		}

		#endregion

		#region Material Workflow

		public async Task<OperationResult> CheckMaterialSelectionAsync(string startNumber, IProgress<OperationProgress> progress)
		{
			try
			{
				int startNumberValue = Convert.ToInt32(startNumber);
				Action<int, string> progressCallback = CreateProgressCallback(progress);

				bool operationCompleted = await _workflowRunner.RunMaterial1Async(startNumberValue, progressCallback);

				if (!operationCompleted)
				{
					ReportProgress(progress, 100, "Material check could not be completed.");
					return OperationResult.Failed("The material check could not be completed.");
				}

				ReportProgress(progress, 80, "Reading material check results...");

				IList<ValidationResult> validationResults = BuildMaterialValidationResults();

				if (validationResults.Count > 0)
				{
					HighlightPartsWithErrors(progress);

					ReportProgress(progress, 100, "Material check completed with issues.");

					string message = validationResults.Count == 1
						? "1 part requires attention."
						: validationResults.Count + " parts require attention.";

					return OperationResult.Failed(message, validationResults);
				}

				ReportProgress(progress, 100, "Material check complete.");

				return OperationResult.Successful("The selected parts passed all material checks.");
			}
			catch (Exception ex)
			{
				return HandleException(ex, progress, "Material check failed.");
			}
		}

		public async Task<OperationResult> CreateMaterialOrderAsync(MaterialOrderRequest request, IProgress<OperationProgress> progress)
		{
			try
			{
				Action<int, string> progressCallback = CreateProgressCallback(progress);

				MaterialOrderRunResult result = await _workflowRunner.RunMaterialOrder(
					request.OrderAction,
					request.OrderType,
					request.PhaseNumber,
					request.IssueNumber,
					request.VariationNumber,
					request.VariationType,
					request.RequiredBy,
					progressCallback);

				if (result == MaterialOrderRunResult.InitialSetupFailed)
				{
					HighlightPartsWithErrors(progress);

					IList<ValidationResult> validationResults = BuildInitialSetupResults();

					ReportProgress(progress, 100, "Material order checks failed.");

					return OperationResult.Failed("Material order checks failed.", validationResults);
				}

				if (result == MaterialOrderRunResult.OperationFailed)
				{
					ReportProgress(progress, 100, "Material order failed.");
					return OperationResult.Failed("Material order failed.");
				}

				if (result == MaterialOrderRunResult.Exception)
				{
					ReportProgress(progress, 100, "Material order failed.");
					return OperationResult.Failed("An error occurred while creating the material order.");
				}

				ReportProgress(progress, 100, "Material order complete.");

				return OperationResult.Successful("Material order prepared.");
			}
			catch (Exception ex)
			{
				return HandleException(ex, progress, "Material order failed.");
			}
		}

		public async Task<OperationResult> PrepareFabsecMaterialAsync()
		{
			try
			{
				bool success = await _workflowRunner.RunPrepareFabsecMaterialAsync();

				if (!success)
				{
					return OperationResult.Failed("Preparing the material member failed.");
				}

				return OperationResult.Successful("Material member prepared.");
			}
			catch (Exception ex)
			{
				return HandleException(ex);
			}
		}

		public async Task<OperationResult> CreateFabsecCarcassAsync()
		{
			try
			{
				bool success = await _workflowRunner.RunCreateFabsecCarcassAsync();

				if (!success)
				{
					return OperationResult.Failed("Creating the carcass failed.");
				}

				return OperationResult.Successful("Carcass created.");
			}
			catch (Exception ex)
			{
				return HandleException(ex);
			}
		}

		#endregion

		#region Drawing Workflow

		public async Task<OperationResult> CheckDrawingSelectionAsync(IProgress<OperationProgress> progress)
		{
			try
			{
				Action<int, string> progressCallback = CreateProgressCallback(progress);

				DrawingCheckRunResult result = await _workflowRunner.RunDrawingChecksAsync(progressCallback);

				OperationResult failureResult = BuildDrawingCheckFailureResult(result, progress, "Drawing checks failed.");

				if (failureResult != null)
				{
					return failureResult;
				}

				ReportProgress(progress, 100, "Drawing checks complete.");

				return OperationResult.Successful("The selected parts passed all drawing checks.");
			}
			catch (Exception ex)
			{
				return HandleException(ex, progress, "Drawing check failed.");
			}
		}

		public async Task<OperationResult> CreateDrawingsAsync(IProgress<OperationProgress> progress)
		{
			try
			{
				Action<int, string> progressCallback = CreateProgressCallback(progress);

				DrawingCheckRunResult result = await _workflowRunner.RunCreateDrawingsAsync(progressCallback);

				OperationResult failureResult = BuildDrawingCheckFailureResult(result, progress, "Drawing checks failed.");

				if (failureResult != null)
				{
					return failureResult;
				}

				ReportProgress(progress, 100, "Drawing creation complete.");

				return OperationResult.Successful("Drawings created.");
			}
			catch (Exception ex)
			{
				return HandleException(ex, progress, "Drawing creation failed.");
			}
		}

		public async Task<OperationResult> DetailOrientationHolesAsync(string detailType, string pltOnFlange, IProgress<OperationProgress> progress)
		{
			try
			{
				ReportProgress(progress, 10, "Preparing orientation detailing...");

				int? pltOnFlangeValue = null;

				if (detailType == "Holes and Plate")
				{
					pltOnFlangeValue = Convert.ToInt32(pltOnFlange);
				}

				ReportProgress(progress, 30, "Detailing orientation markers...");

				bool success = await _workflowRunner.RunDetailOrientationHolesAsync(detailType, pltOnFlangeValue);

				if (!success)
				{
					ReportProgress(progress, 100, "Orientation detailing failed.");
					return OperationResult.Failed("Orientation detailing could not be completed.");
				}

				ReportProgress(progress, 100, "Orientation detailing complete.");

				return OperationResult.Successful("Orientation detailing complete.");
			}
			catch (Exception ex)
			{
				return HandleException(ex, progress, "Orientation detailing failed.");
			}
		}

		#endregion

		#region Fab Pack Workflow

		public async Task<OperationResult> CheckFabPackSelectionAsync(FabPackRequest request, IProgress<OperationProgress> progress)
		{
			try
			{
				Action<int, string> progressCallback = CreateProgressCallback(progress);

				FabPackCheckRunResult result = await _workflowRunner.RunFabPackChecksAsync(progressCallback, request.PhaseNumber, request.IssueNumber);

				if (result == FabPackCheckRunResult.InitialSetupFailed)
				{
					HighlightPartsWithErrors(progress);

					IList<ValidationResult> validationResults = BuildInitialSetupResults();

					ReportProgress(progress, 100, "Fab pack model checks failed.");

					return OperationResult.Failed("Fab pack model checks failed.", validationResults);
				}

				if (result == FabPackCheckRunResult.DrawingChecksFailed)
				{
					IList<ValidationResult> validationResults = BuildFabPackDrawingResults();

					ReportProgress(progress, 100, "Fab pack drawing checks failed.");

					return OperationResult.Failed("Fab pack drawing checks failed.", validationResults);
				}

				ReportProgress(progress, 100, "Fab pack checks complete.");

				return OperationResult.Successful("Selected parts are ready for Fab Pack issuing.");
			}
			catch (Exception ex)
			{
				return HandleException(ex, progress, "Fab pack checks failed.");
			}
		}

		public async Task<OperationResult> CreateFabPackAsync(FabPackRequest request, IProgress<OperationProgress> progress)
		{
			try
			{
				Action<int, string> progressCallback = CreateProgressCallback(progress);

				FabPackRunResult result = await _workflowRunner.RunCreateFabPackAsync(
					request.PhaseNumber,
					request.IssueNumber,
					request.SiteDate,
					request.VariationType,
					request.VariationNumber,
					progressCallback);

				if (result == FabPackRunResult.InitialSetupFailed)
				{
					HighlightPartsWithErrors(progress);

					IList<ValidationResult> validationResults = BuildInitialSetupResults();

					ReportProgress(progress, 100, "Fab Pack creation failed.");

					return OperationResult.Failed("Fab Pack model checks failed.", validationResults);
				}

				if (result == FabPackRunResult.DrawingChecksFailed)
				{
					IList<ValidationResult> validationResults = BuildFabPackDrawingResults();

					ReportProgress(progress, 100, "Fab Pack drawing checks failed.");

					return OperationResult.Failed("Fab Pack drawing checks failed.", validationResults);
				}

				if (result == FabPackRunResult.OperationFailed)
				{
					ReportProgress(progress, 100, "Fab Pack creation failed.");

					return OperationResult.Failed("Fab Pack could not be created.");
				}

				ReportProgress(progress, 100, "Fab Pack complete.");

				return OperationResult.Successful("Fab Pack created successfully.");
			}
			catch (Exception ex)
			{
				return HandleException(ex, progress, "Fab Pack creation failed.");
			}
		}

		#endregion

		#region Auto-Complete and Model Selection

		public async Task<OperationResult> AutoCompleteAsync(AutoCompleteType autoCompleteType, int? executionClass = null)
		{
			try
			{
				bool success;

				switch (autoCompleteType)
				{
					case AutoCompleteType.NameAndClass:
						success = await _workflowRunner.RunAutoCompleteNameAndClassAsync();
						break;

					case AutoCompleteType.ExecutionClass:
						if (!executionClass.HasValue)
						{
							return OperationResult.Failed("An execution class must be selected.");
						}

						success = await _workflowRunner.RunAutoCompleteExecutionClassAsync(executionClass.Value);
						break;

					case AutoCompleteType.Orientation:
						success = await _workflowRunner.RunAutoCompleteOrientationAsync();
						break;

					case AutoCompleteType.SecondaryNumberingMismatch:
						success = await _workflowRunner.RunAutoCompleteFittingStartNumberMismatchAsync();
						break;

					case AutoCompleteType.SecondaryPhasingMismatch:
						success = await _workflowRunner.RunAutoCompleteFittingPhaseNumberMismatchAsync();
						break;

					default:
						return OperationResult.Failed("Auto-Complete is not available for this check.");
				}

				if (!success)
				{
					return OperationResult.Failed("Auto-Complete could not be completed.");
				}

				return OperationResult.Successful("Auto-Complete completed.");
			}
			catch (Exception ex)
			{			
				return HandleException(ex);
			}
		}

		public void ReselectOriginalParts()
		{
			_workflowRunner.ReselectOriginalParts();
		}

		public void SelectAndZoomToPart(string guid)
		{
			_workflowRunner.SelectAndZoomToPart(guid);
		}

		#endregion

		#region Validation

		private IList<ValidationResult> BuildInitialSetupResults()
		{
			return BuildPartValidationResults(
				new ValidationDefinition("Numbering", Error.NumberingNotUpToDate),
				new ValidationDefinition("Previous Step", Error.PreviousStepIncomplete),
				new ValidationDefinition("Locked Part", Error.PartLocked),
				new ValidationDefinition("Fabsec Prepared", Error.FabsecNotProcessed));
		}

		private IList<ValidationResult> BuildMaterialValidationResults()
		{
			return BuildPartValidationResults(
				new ValidationDefinition("Name and Class", Error.NameAndClass, AutoCompleteType.NameAndClass),
				new ValidationDefinition("Execution", Error.Execution, AutoCompleteType.ExecutionClass),
				new ValidationDefinition("Orientation", Error.Orientation, AutoCompleteType.Orientation));
		}

		private IList<ValidationResult> BuildDrawingStageOneResults()
		{
			return BuildPartValidationResults(
				new ValidationDefinition("Ordered", Error.PartNotOrdered),
				new ValidationDefinition("Name and Class", Error.NameAndClass, AutoCompleteType.NameAndClass),
				new ValidationDefinition("Execution Class", Error.Execution, AutoCompleteType.ExecutionClass));
		}

		private IList<ValidationResult> BuildDrawingStageTwoResults()
		{
			return BuildPartValidationResults(
				new ValidationDefinition("Finish", Error.FinishMissing),
				new ValidationDefinition("Intumescent Loading", Error.IntumescentLoadingMissing),
				new ValidationDefinition("Secondary Numbering", Error.SecondaryNumberingMismatch, AutoCompleteType.SecondaryNumberingMismatch),
				new ValidationDefinition("Secondary Phasing", Error.SecondaryPhasingMismatch, AutoCompleteType.SecondaryPhasingMismatch));
		}

		private IList<ValidationResult> BuildFabPackDrawingResults()
		{
			List<ValidationResult> results = new List<ValidationResult>();

			if (_workflowRunner.SelectedObjects == null || _workflowRunner.SelectedObjects.PrismParts == null)
			{
				return results;
			}

			foreach (PrismPart part in _workflowRunner.SelectedObjects.PrismParts)
			{
				if (part.HasProperlyAssignedFolder && part.HasRevision && part.HasDrawing)
				{
					continue;
				}

				ValidationResult result = new ValidationResult(part);

				result.Checks.Add(new ValidationCheckResult
				{
					CheckName = "Drawing Folder",
					Passed = part.HasProperlyAssignedFolder
				});

				result.Checks.Add(new ValidationCheckResult
				{
					CheckName = "Revision",
					Passed = part.HasRevision
				});

				result.Checks.Add(new ValidationCheckResult
				{
					CheckName = "Drawing Found",
					Passed = part.HasDrawing
				});

				results.Add(result);
			}

			return results;
		}

		private OperationResult BuildDrawingCheckFailureResult(DrawingCheckRunResult result, IProgress<OperationProgress> progress, string progressMessage)
		{
			IList<ValidationResult> validationResults;

			switch (result)
			{
				case DrawingCheckRunResult.InitialSetupFailed:
					validationResults = BuildInitialSetupResults();
					break;

				case DrawingCheckRunResult.StageOneFailed:
					validationResults = BuildDrawingStageOneResults();
					break;

				case DrawingCheckRunResult.StageTwoFailed:
					validationResults = BuildDrawingStageTwoResults();
					break;

				default:
					return null;
			}

			HighlightPartsWithErrors(progress);
			ReportProgress(progress, 100, progressMessage);

			return OperationResult.Failed("Drawing checks failed.", validationResults);
		}

		private IList<ValidationResult> BuildPartValidationResults(params ValidationDefinition[] definitions)
		{
			List<ValidationResult> results = new List<ValidationResult>();

			if (_workflowRunner.SelectedObjects == null || _workflowRunner.SelectedObjects.PrismParts == null)
			{
				return results;
			}

			foreach (PrismPart part in _workflowRunner.SelectedObjects.PrismParts)
			{
				if (part.PartErrors == null)
				{
					continue;
				}

				if (!definitions.Any(definition => part.PartErrors.Contains(definition.Error)))
				{
					continue;
				}

				ValidationResult result = new ValidationResult(part);

				foreach (ValidationDefinition definition in definitions)
				{
					AddValidationCheck(result, part, definition.CheckName, definition.Error, definition.AutoCompleteType);
				}

				results.Add(result);
			}

			return results;
		}

		private static void AddValidationCheck(ValidationResult result, PrismPart part, string checkName, Error error, AutoCompleteType autoCompleteType = AutoCompleteType.None)
		{
			result.Checks.Add(new ValidationCheckResult
			{
				CheckName = checkName,
				Passed = !part.PartErrors.Contains(error),
				AutoCompleteType = autoCompleteType
			});
		}

		private void HighlightPartsWithErrors(IProgress<OperationProgress> progress)
		{
			ReportProgress(progress, 90, "Highlighting parts with errors...");

			if (_workflowRunner.SelectedObjects == null || _workflowRunner.SelectedObjects.PrismParts == null)
			{
				return;
			}

			List<PrismPart> errorParts = _workflowRunner.SelectedObjects.PrismParts
				.Where(part => part.PartErrors != null && part.PartErrors.Count > 0)
				.ToList();

			if (errorParts.Count == 0)
			{
				return;
			}

			ModelModifiers.SetPartsRed(Convertor.PrismPartsToModelObjects(errorParts), true);
		}

		private sealed class ValidationDefinition
		{
			public string CheckName { get; private set; }
			public Error Error { get; private set; }
			public AutoCompleteType AutoCompleteType { get; private set; }

			public ValidationDefinition(string checkName, Error error, AutoCompleteType autoCompleteType = AutoCompleteType.None)
			{
				CheckName = checkName;
				Error = error;
				AutoCompleteType = autoCompleteType;
			}
		}

		#endregion

		#region Progress

		private static Action<int, string> CreateProgressCallback(IProgress<OperationProgress> progress)
		{
			return (percentage, message) => ReportProgress(progress, percentage, message);
		}

		private static void ReportProgress(IProgress<OperationProgress> progress, int percentage, string message)
		{
			progress?.Report(new OperationProgress(percentage, message));
		}

		private OperationResult HandleException(Exception ex)
		{
			_ = LogException(ex);
			return OperationResult.Failed(ex.Message);
		}

		private OperationResult HandleException(Exception ex, IProgress<OperationProgress> progress, string progressMessage)
		{
			ReportProgress(progress, 100, progressMessage);
			return HandleException(ex);
		}

		#endregion

		#region Settings and Support

		public IList<ClassificationCodeSetting> GetClassificationCodes()
		{
			return _settingsService.GetClassificationCodes();
		}

		public bool SaveClassificationCodes(IList<ClassificationCodeSetting> settings)
		{
			return _settingsService.SaveClassificationCodes(settings);
		}

		public ProjectUserSettings GetProjectUserSettings()
		{
			return _settingsService.GetProjectUserSettings();
		}

		public bool SaveProjectUserSettings(ProjectUserSettings settings)
		{
			return _settingsService.SaveProjectUserSettings(settings);
		}

		public int? GetCurrentPrelimStartPoint()
		{
			return _settingsService.GetCurrentPrelimStartPoint();
		}

		public bool SetPrelimStartPoint(int newStartPoint)
		{
			return _settingsService.SetPrelimStartPoint(newStartPoint);
		}

		public AdvancedPrismSettings GetAdvancedSettings()
		{
			return _settingsService.GetAdvancedSettings();
		}

		public bool SaveAdvancedSettings(AdvancedPrismSettings settings)
		{
			return _settingsService.SaveAdvancedSettings(settings);
		}

		public PackageDirectorySettings GetPackageDirectorySettings()
		{
			return _settingsService.GetPackageDirectorySettings();
		}

		public bool SavePackageDirectorySettings(PackageDirectorySettings settings)
		{
			return _settingsService.SavePackageDirectorySettings(settings);
		}

		public string GetPrismVersion()
		{
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetEntryAssembly();

			if (assembly == null)
			{
				return "Unknown";
			}

			return System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
		}

		public void WriteHelpEmail()
		{
			EmailWriter.WriteHelpEmail(GetPrismVersion());
		}

		#endregion
	}
}