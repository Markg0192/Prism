using Prism.ButtonOperations;
using Prism.CustomDialogs;
using Prism.ExternalService;
using Prism.Managers.ChangeManager;
using Prism.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Prism.Enums;
using Model = Tekla.Structures.Model.Model;
using Task = System.Threading.Tasks.Task;
using TextBox = System.Windows.Forms.TextBox;
using System.IO;
using Application = System.Windows.Forms.Application;
using System.Linq;
using Tekla.Structures;

namespace Prism
{
	public partial class PrismUI : Form
	{
		private Model _model;
		private PrismProjectData _projectData;
		public static SelectedObjects _selectedObjects;
		private WebService1 _webService;
		private string _teklaVersion;

		public PrismUI()
		{
			InitializeComponent();
		}

		private async void btn_Material1_Click_1(object sender, EventArgs e)
		{
			StartFunction();

			try
			{
				if (!await Task.Run(() => InitialSetup(StageTypes.Prelim1, false, "x", "x", statusStrip_Mat, MaterialStatusLabel))) { EndFunction(0); return; }

				if (!await Task.Run(() => _selectedObjects.MaterialButton1op(_projectData, (int)StageTypes.Prelim1, statusStrip_Mat, MaterialStatusLabel))) { EndFunction(0); return; }

				ModelModifiers.RedrawViews();
			}
			catch (Exception ex)
			{
				EndWithException(ex); return;
			}

			EndFunction(1);
		}

		private async void btn_Material2_Click_1(object sender, EventArgs e)
		{
			StartFunction();

			try
			{
				if (!await Task.Run(() => InitialSetup(StageTypes.Prelim2, true, "x", "x", statusStrip_Mat, MaterialStatusLabel))) { EndFunction(0); return; };

				if (!await Task.Run(() => _selectedObjects.MaterialButton2op(txt_StartNumber.Text, (int)StageTypes.Prelim2, _projectData, _model, statusStrip_Mat, MaterialStatusLabel))) { EndFunction(0); return; }

				ModelModifiers.RedrawViews();
			}
			catch (Exception ex)
			{
				EndWithException(ex); return;
			}

			EndFunction(1);
		}

		private async void btn_Material3_Click_1(object sender, EventArgs e)
		{
			StartFunction();

			string orderType;

			try
			{
				orderType = $"{cmb_OrderCall.Text} {cmb_OrderMaterial.Text}";

				ReportManager myReportManager = new ReportManager(_projectData, txt_MaterialPhaseNumber.Text, txt_MaterialIssueNumber.Text);
				ModelModifiers.VariationCheck(_projectData, txt_VariationNumber.Text, cmb_VariationType.Text);

				if (orderType.Contains("Special Fittings"))
				{
					if (!await Task.Run(() => ProcessSpecialFittings(orderType, statusStrip_Mat, MaterialStatusLabel))) return;
				}
				else
				{
					if (!orderType.Contains("Bolts"))
					{
						if (!await Task.Run(() => InitialSetup(StageTypes.Prelim3, true, "x", "x", statusStrip_Mat, MaterialStatusLabel))) { EndFunction(0); return; };
					}
					else { ModelChecker.ClearOldLists(); }


					if (!await Task.Run(() => _selectedObjects.MaterialButton3op(_projectData, myReportManager,
						orderType, (int)StageTypes.Prelim3, StageTypes.Prelim3, _model, txt_MatSiteDate.Text, statusStrip_Mat, MaterialStatusLabel))) { EndFunction(0); return; }

					_model.CommitChanges();
				}

				SetNextPrelimToUseLabel();

				MovePackToDirectory(MaterialFolderType(orderType, myReportManager.Folders, out AdvancedSettingType setting), setting, txt_MaterialPhaseNumber.Text);

				Logging.AddToMaterialOrderProcessedCount(Constants.ModelProjectInforLocation(_projectData.ProjNumberAndGuid));
			}
			catch (Exception ex)
			{
				EndWithException(ex); return;
			}

			EndFunction(1, orderType.Contains("Omit"));
		}

		private string MaterialFolderType(string orderType, FolderManager folders, out AdvancedSettingType setting)
		{
			if (orderType.Contains("Material"))
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
			return "";
		}

		private async void btn_Detail1_Click_1(object sender, EventArgs e)
		{
			StartFunction();

			try
			{
				if (!await Task.Run(() => InitialSetup(StageTypes.Check1, false, "x", "x", statusStrip_Det, DetailingStatusLabel))) { EndFunction(0); return; }

				if (!await Task.Run(() => _selectedObjects.DetailButton1op(_projectData, (int)StageTypes.Check1, statusStrip_Mat, MaterialStatusLabel))) { EndFunction(0); return; }
			}
			catch (Exception ex)
			{
				EndWithException(ex); return;
			}

			EndFunction(1);
		}

		private async void btn_Detail2_Click_1(object sender, EventArgs e)
		{
			StartFunction();

			try
			{
				ModelModifiers.ResetWorkPlane(_model);
				if (!await Task.Run(() => InitialSetup(StageTypes.Check2, true, "x", "x", statusStrip_Det, DetailingStatusLabel))) { EndFunction(0); return; }

				string orientationType = cmb_ColumnOrientationType.Text; //we need this to avoid cross threading. (unsure why...)
				if (!await Task.Run(() => _selectedObjects.DetailButton2op(_projectData, (int)StageTypes.Check2, orientationType, txt_PlateOnFlange.Text, statusStrip_Det, DetailingStatusLabel))) { return; }
			}
			catch (Exception ex)
			{
				EndWithException(ex); return;
			}

			EndFunction(1);
		}

		private async void btn_Detail3_Click_1(object sender, EventArgs e)
		{
			StartFunction();

			try
			{
				if (!await Task.Run(() => InitialSetup(StageTypes.Check3, true, "x", "x", statusStrip_Det, DetailingStatusLabel))) { EndFunction(0); return; }

				if (!_selectedObjects.DetailButton3op(_projectData, (int)StageTypes.Check3, statusStrip_Mat, MaterialStatusLabel)) { EndFunction(0); return; }
			}
			catch (Exception ex)
			{
				EndWithException(ex); return;
			}

			EndFunction(1);
		}

		private async void btn_FabMisc_Click(object sender, EventArgs e)
		{
			StartFunction();

			if (!await Task.Run(() => InitialSetup(StageTypes.Bolt, false))) { EndFunction(0); return; }

			/*   var mySolid = _selectedObjects.SelectedModelParts[0].GetSolid();
               var mySolid2 = _selectedObjects.SelectedModelParts[1].GetSolid();

               var bolts = _selectedObjects.SelectedModelParts[0].GetBolts();
               var bolt3s = _selectedObjects.SelectedModelParts[1].GetBolts();


               FaceEnumerator faceEnum = mySolid.GetFaceEnumerator();

               int solid1Faces = 0;
               while (faceEnum.MoveNext())
               {
                   solid1Faces++;
               }

               FaceEnumerator faceEnum2 = mySolid2.GetFaceEnumerator();

               int solid2Faces = 0;
               while (faceEnum2.MoveNext())
               {
                   solid2Faces++;
               }
            */
			// await Task.Run(() => FabMisc.FabMiscOp(txt_SiteDate.Text, _selectedObjects, false, null));

			EndFunction(1);
		}

		private void btnCreatePackage1_Click_1(object sender, EventArgs e)
		{
			try
			{
				Logging.DebugLog("create package start", "");
				CreatePackageAsync(StageTypes.FAB);
			}
			catch (Exception ex)
			{
				Logging.ExceptionError(_projectData.ProjName, TeklaStructuresInfo.GetCurrentProgramVersion(), ex.Message, ex.StackTrace);
				PrismWarnings.FatalError(ex.Message, this);
			}
		}

		private async void btn_RocketPacket_Click(object sender, EventArgs e)
		{
			try
			{
				if (!PrismWarnings.RocketButtonCheck()) return;

				if (!await Task.Run(() => InitialSetup(StageTypes.RocketPacket, false))) { EndFunction(0); return; }

				int rocketPartLimit = 100;
				int numberOfSelectedParts = _selectedObjects.PrismParts.Count;
				if (numberOfSelectedParts > rocketPartLimit)
				{
					PrismWarnings.TooManyPartsForRocket(numberOfSelectedParts.ToString(), rocketPartLimit.ToString());
					EndFunction(0); return;
				}

				CreatePackageAsync(StageTypes.RocketPacket);
			}
			catch (Exception ex)
			{
				EndWithException(ex); return;
			}
		}

		private async void btn_SpecialOperations_Click(object sender, EventArgs e)
		{
		//	PrismMacroBuilder.RunPrismDrawingReport(@"C:\\TeklaStructuresModels2023\\Sandbox\\PlotFiles");
			StartFunction();

			ReportManager repoman = new ReportManager(_projectData, "IFC", "02");
			repoman.Folders.CreateFabFolders();

			_selectedObjects = new SelectedObjects("C:\\TeklaStructuresModels\\2021 models\\Sandbox\\PrismIFCExportTest\\", StageTypes.Check2, repoman.PhaseNum, repoman.IssueNum, _model, Constants.SpecialOperationUser());
		   
			IFCExporter.ExportIndividualIFC(_selectedObjects, repoman.Folders.IfcPath, statusStrip_Fab, StatusLabel);
			//bool boltOrderAdded = false;
		//	await Task.Run(() => FabMisc.FabMiscOp(_model, txt_SiteDate.Text, _selectedObjects, false, repoman, 1, _projectData, false, statusStrip_Fab, StatusLabel, out boltOrderAdded));

			//  ModelModifiers.PerformNumbering();


			/*    string packType = "Orders\\Material";
				string serverLocation = $@"\\sfrplc.local\public\Do Issues\{repoman.ProjectData.ProjNumber}\{repoman.PhaseNum}\{packType}";

				CopyToServer(repoman.Folders.MatPath, serverLocation);

				repoman = new ReportManager(_projectData, "100", "02");
				packType = "Orders\\Material";
				serverLocation = $@"\\sfrplc.local\public\Do Issues\{repoman.ProjectData.ProjNumber}\{repoman.PhaseNum}\{packType}";

				CopyToServer(repoman.Folders.MatPath, serverLocation);

				repoman = new ReportManager(_projectData, "100", "01");
				packType = "Orders\\Bolts";
				serverLocation = $@"\\sfrplc.local\public\Do Issues\{repoman.ProjectData.ProjNumber}\{repoman.PhaseNum}\{packType}";

				CopyToServer(repoman.Folders.BoltPath, serverLocation);*/


			// 

			//	QrCodeGenerator.ApplyQrCode(_selectedObjects.PrismParts, _projectData, repoman.Folders.QrCodePath);


			// MovePackToDirectory("C:\\TeklaStructuresModels2023\\Sandbox\\Prism Packages\\C1991-120-FAB-ISSUE01");

			/*

             if (!await Task.Run(() => InitialSetup(StageTypes.Check1, false, "x", "x", statusStrip_Det, DetailingStatusLabel))) { EndFunction(0); return; }

             FolderManager fm = new FolderManager(_projectData, "100", "01");
             fm.CreateFabFolders();

             QrCodeGenerator.ApplyQrCode(_selectedObjects.PrismParts, _projectData, fm.IfcPath, fm.QrCodePath);
			*/
			//IFCExporter.ExportIndividualIFC(_selectedObjects, repoman.Folders.IfcPath, "");

			EndFunction(1);
		}

		private bool CopyToServer(string sourceDir, string serverLocation)
		{
			if (!Directory.Exists(sourceDir)) //check that it exists
			{
				PrismWarnings.DirectoryCannotBeReached(sourceDir); //if it doesn't tell the user and quit
				return false;
			}
			else
			{
				CopyDirectory2(sourceDir, serverLocation);
			}
			return true;
		}

		private void CopyDirectory2(string sourceDir, string destDir)
		{
			// Get the name of the source directory
			string sourceDirectoryName = Path.GetFileName(sourceDir.TrimEnd(Path.DirectorySeparatorChar));

			// Combine the destination directory with the source directory name
			string newDestDir = Path.Combine(destDir, sourceDirectoryName);

			// Check if the destination directory already exists
			if (Directory.Exists(newDestDir))
			{
				// Show a message box to the user to ask if they want to replace the existing directory
				DialogResult result = MessageBox.Show(this, "The directory already exists. Do you want to replace it?", "Directory Exists", MessageBoxButtons.YesNo);

				if (result == DialogResult.No)
				{
					// User chose not to replace the directory, so stop the operation
					return;
				}

				// If the user chooses Yes, you can optionally delete the existing directory before creating the new one
				Directory.Delete(newDestDir, true);
			}

			// Create the destination directory
			Directory.CreateDirectory(newDestDir);

			// Copy all the files from the source directory to the destination directory
			foreach (string file in Directory.GetFiles(sourceDir))
			{
				string destFile = Path.Combine(newDestDir, Path.GetFileName(file));
				File.Copy(file, destFile, true);
			}

			// Copy all the subdirectories from the source directory to the destination directory
			foreach (string directory in Directory.GetDirectories(sourceDir))
			{
				string destDirectory = Path.Combine(newDestDir, Path.GetFileName(directory));
				CopyDirectory2(directory, destDirectory);
			}
		}

		private void EndWithException(Exception ex)
		{
			Logging.ExceptionError(_projectData.ProjName, TeklaStructuresInfo.GetCurrentProgramVersion(), ex.Message, ex.StackTrace);
			PrismWarnings.FatalError(ex.Message, this);
			EndFunction(0);
		}

		public bool InitialSetup(StageTypes stageType, bool checkForPreviousSteps, string phaseNum = "x", string issueNum = "x", ToolStrip toolStrip = null, ToolStripStatusLabel statusLabel = null)
		{
			SetStatusLabels("Gathering Parts");
			ModelChecker.ClearOldLists();
			_selectedObjects = new SelectedObjects(_projectData.ProjPath, stageType, phaseNum, issueNum, _model, Constants.SpecialOperationUser(), toolStrip, statusLabel);
	
			if (!PartsSelected()) return false;

			if (!NumberingIsUpToDate(stageType)) return false;

			if (!PreviousStepsComplete(checkForPreviousSteps, stageType)) return false;

			if (!AnyPartsLocked()) return false;

			SetStatusLabels("Running Operation");
			return true;
		}

		private bool AnyPartsLocked()
		{
			if (_selectedObjects.PrismParts.Any(part => part.IsLocked))
			{
				SetStatusLabels("Locked Parts Selected");
				List<PrismPart> lockedParts = _selectedObjects.GetLockedParts();
				PrismWarnings.LockedPartsSelected(lockedParts);
				ModelModifiers.SetPartsRed(Convertor.PrismPartsToModelObjects(lockedParts));
				return false;
			}
			return true;
		}

		private bool PreviousStepsComplete(bool checkForPreviousSteps, StageTypes stageType)
		{
			if (checkForPreviousSteps && !ModelChecker.ArePreviousStepsComplete(_selectedObjects, (int)stageType))
			{
				SetStatusLabels("Previous Steps Incomplete");
				return false;
			}
			return true;
		}

		private bool PartsSelected()
		{
			if (_selectedObjects.PrismParts.Count == 0)
			{
				SetStatusLabels("No Parts Selected");
				PrismWarnings.NoPartsSelected();
				return false;
			}
			return true;
		}

		private bool NumberingIsUpToDate(StageTypes stageType)
		{
			if ((stageType == StageTypes.FAB || stageType == StageTypes.RocketPacket) && _selectedObjects.GetPartsWithOutOfDateNumbers().Count != 0)
			{
				SetStatusLabels("Numbering not up to date");
				PrismWarnings.NumberingIsNotUpToDate();
				return false;
			}
			return true;
		}

		private async void CreatePackageAsync(StageTypes stageType)
		{
			StartFunction();

			GetPhaseAndIssueNumber(out string phaseNum, out string issueNum, out bool runChangeManager);

			ModelModifiers.VariationCheck(_projectData, txt_Variation_Fab.Text, cmb_VariationType_Fab.Text);

			if (!await Task.Run(() => InitialSetup(StageTypes.FAB, stageType == StageTypes.FAB, phaseNum, issueNum, statusStrip_Fab, StatusLabel))) { EndFunction(0); return; }

			if (!_selectedObjects.NumbersUpToDate) { SetStatusLabels("Numbers not up to date"); EndFunction(0); return; }

			bool runSeversafe = false;
			int divisionNo = 0;
			if (_selectedObjects.SeversafePresent)
			{
				runSeversafe = PrismWarnings.ShouldSeverSafeBeProcessed();
				if (runSeversafe)
				{
					divisionNo = PrismWarnings.DivsionFrom();
					if (divisionNo == 0)
					{
						runSeversafe = false;
						PrismWarnings.SeversafeOrderCancelled();
					}
				}
			}

			SetStatusLabels("Creating Fab Package");

			ReportManager myReportManager = new ReportManager(_projectData, phaseNumber.Text, issueNum);

			var (success, totalNcRequired) = await _selectedObjects.CreateFabPackage(_model, _projectData, phaseNumber.Text, issueNum, stageType, txt_SiteDate.Text, runSeversafe, runChangeManager, statusStrip_Fab, StatusLabel, _teklaVersion);

			if (!success)
			{
				EndFunction(0);
			}

			bool boltOrderAdded = false;
			await Task.Run(() => FabMisc.FabMiscOp(_model, txt_SiteDate.Text, _selectedObjects, runSeversafe, myReportManager, divisionNo, _projectData, runChangeManager, statusStrip_Fab, StatusLabel, out boltOrderAdded));

			if (!CheckNcCreation(myReportManager, totalNcRequired, out int numberOfFilesCreated, out HashSet<string> uniqueFiles))
			{
				// Get files from the comparison folder
				HashSet<string> comparisonFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				AddFilesFromFolder(myReportManager.Folders.NcPath, comparisonFiles);

				// Get the list of files that exist in the unique set but not in the comparison folder
				List<string> filesNotInComparisonFolder = uniqueFiles.Except(comparisonFiles).ToList();

				PrismWarnings.NcDataCreationFailed(filesNotInComparisonFolder);
				Logging.NCFailed(_projectData.ProjNumberAndName, phaseNumber.Text, issueNum, TeklaStructuresInfo.GetCurrentProgramVersion(), myReportManager.Folders.NcPath, totalNcRequired, numberOfFilesCreated);
			}
			else
			{
				Logging.NCCreated(_projectData.ProjNumberAndName, phaseNumber.Text, issueNum, TeklaStructuresInfo.GetCurrentProgramVersion(), myReportManager.Folders.NcPath, totalNcRequired, numberOfFilesCreated);
			}

			MovePackToDirectory(myReportManager.Folders.FabPath, AdvancedSettingType.DirectoryFabPack, phaseNum);
			if (boltOrderAdded) MovePackToDirectory(myReportManager.Folders.BoltPath, AdvancedSettingType.DirectoryBolts, phaseNum);

			Logging.AddToFabCompleteCount(Constants.ModelProjectInforLocation(_projectData.ProjNumberAndGuid));

			EndFunction(1);
		}

		private bool MovePackToDirectory(string sourcePath1, AdvancedSettingType settingType, string phaseNumber)
		{
			string directory = Logging.GetAdvancedSetting(_projectData.ProjNumberAndGuid, settingType);
			string variationDirectory = "";
			if (_projectData.IsVariation)
			{
				variationDirectory = Logging.GetAdvancedSetting(_projectData.ProjNumberAndGuid, AdvancedSettingType.DirectoryVariation);
			}

			if (directory != "" || variationDirectory != "")
			{
				string variationWithSubFolder = VariationSubFolderName(variationDirectory, settingType);
				if (!PrismWarnings.MoveToFabDirectory(directory, variationWithSubFolder, _projectData.IsVariation)) return false;
				return CopyAndDeleteDirectory(sourcePath1, directory, variationWithSubFolder, phaseNumber);
			}
			return true;
		}

		private string VariationSubFolderName(string variation, AdvancedSettingType setting)
		{
			if (variation != "")
			{
				if (setting == AdvancedSettingType.DirectoryMaterial)
				{
					variation = Path.Combine(variation, "Material");
				}
				if (setting == AdvancedSettingType.DirectoryBolts)
				{
					variation = Path.Combine(variation, "Bolt Orders");
				}
				if (setting == AdvancedSettingType.DirectoryCarcasses)
				{
					variation = Path.Combine(variation, "Fabsec Carcasses");
				}
				if (setting == AdvancedSettingType.DirectorySeversafe)
				{
					variation = Path.Combine(variation, "Seversafe Orders");
				}
				if (setting == AdvancedSettingType.DirectoryFabPack)
				{
					variation = Path.Combine(variation, "Fabrication Packages");
				}
				if (!Directory.Exists(variation)) //check that it exists
				{
					// Create the destination directory if it doesn't exist
					Directory.CreateDirectory(variation);
				}
			}
			return variation;
		}

		public bool CopyAndDeleteDirectory(string sourceDir, string destDir, string secondDestDir, string phaseNumber)
		{
			try
			{
				// Ensure the source directory exists
				if (!Directory.Exists(sourceDir))
				{
					PrismWarnings.DirectoryCannotBeReached(sourceDir);
					return false;
				}

				if (!RunCopyLogicForGivenDirectory(sourceDir, destDir, phaseNumber)) return false;
				if (!RunCopyLogicForGivenDirectory(sourceDir, secondDestDir, phaseNumber)) return false;

				// Delete the source directory after copying
				Directory.Delete(sourceDir, true);
			}
			catch (Exception ex)
			{
				PrismWarnings.DirectoryCannotBeCopied(ex.Message);
				return false;
			}

			return true;
		}

		private bool RunCopyLogicForGivenDirectory(string sourceDir, string destDir, string phaseNumber)
		{
			// if the destination is given
			if (destDir != "")
			{
				if (!Directory.Exists(destDir)) //check that it exists
				{
					PrismWarnings.DirectoryCannotBeReached(destDir); //if it doesn't tell the user and quit
					return false;
				}
				else
				{ //if it does copy from source to dest(ination)
					destDir = Path.Combine(destDir, "Phase " + phaseNumber);
					// Create the destination directory
					string newDestDir = Path.Combine(destDir, Path.GetFileName(sourceDir));
					CopyDirectory(sourceDir, newDestDir);
				}
			}
			return true;
		}

		private void CopyDirectory(string sourceDir, string destDir)
		{
			// Create the destination directory if it doesn't exist
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

		static void AddFilesFromFolder(string folderPath, HashSet<string> uniqueFiles)
		{
			if (Directory.Exists(folderPath))
			{
				var files = Directory.GetFiles(folderPath);
				foreach (var file in files)
				{
					string fileName = Path.GetFileNameWithoutExtension(file);

					// Split by dash and take the first part, then remove any remaining file extension
					string baseFileName = fileName.Contains('-') ? fileName.Split('-')[0] : fileName.Split('.')[0];
					uniqueFiles.Add(baseFileName);
				}
			}
			else
			{
				Console.WriteLine($"Folder does not exist: {folderPath}");
			}
		}

		private void GetPhaseAndIssueNumber(out string phaseNum, out string issueNum, out bool runChangeManager)
		{
			issueNum = issueNumber.Text;
			phaseNum = phaseNumber.Text;
			runChangeManager = false;

			if (Constants.SpecialOperationUser())
			{
				runChangeManager = PrismWarnings.RunChangeManagement();
				if (runChangeManager)
				{
					issueNum = cmb_FabIssueNo.Text;
					// If the selectedItem contains "(new)", it implies this is a new issue, so we extract the numeric part only
					if (issueNum.Contains("(new)"))
					{
						issueNum = issueNum.Substring(0, 2); // or other appropriate substring logic if "(new)" is not at the end
					}
					if (issueNum != "01")
					{

					}
				}
			}
		}

		private bool OrderSpecials(int orderAction, string orderType, StageTypes stageType, ReportManager myReportManager, ToolStrip toolStrip, ToolStripStatusLabel statusLabel)
		{
			bool runDrawings = PrismWarnings.RunSpecialFittingDrawings();
			if (!myReportManager.Folders.CreateMatFolder(false, runDrawings)) return false;

			if (orderAction == 2) //User wants to order using special fitting tags
			{
				ModelModifiers.SelectSpecialTaggedInSelection(_selectedObjects);
				ModelModifiers.AddPrelimMarks(_selectedObjects, _projectData);
				_selectedObjects = new SelectedObjects(myReportManager.Folders.MatPath, stageType, myReportManager.PhaseNum, myReportManager.IssueNum, _model, Constants.SpecialOperationUser()); // we reset selected objects here (because we just changed the selection)
				myReportManager.CreateMaterialReports(_selectedObjects, orderType, stageType);
			}
			else
			{
				ModelModifiers.AddPrelimMarks(_selectedObjects, _projectData);
				myReportManager.CreateMaterialReports(_selectedObjects, orderType, stageType);
			}

			if (runDrawings)
			{
				ReportManager.SelectDrawingsInDocManager(null);
				DrawingManager dm = new DrawingManager(_model, _projectData, txt_MaterialPhaseNumber.Text, txt_MaterialIssueNumber.Text, myReportManager.Folders.MatFolder);
				if (dm.GetDrawingFolder(Enums.DrawingFolder.Default).Count != 0) { PrismWarnings.IncorrectlyAssignedDrawings(); return false; }

				List<int> drawingCount = new List<int> { 0, dm.GetDrawingByType("W").Count };
				DrawingManager.PrintAndIssueDrawings(myReportManager.Folders.MatFolder, myReportManager.Folders.MatPath, drawingCount, "\\SPC", 0, 1, myReportManager, false, toolStrip, statusLabel);
				PrismMacroBuilder.ClearPrintDialog();
			}
			return true;
		}

		private void CreatePackageNotAsync()
		{
			StartFunction();

			InitialSetup(StageTypes.FAB, true);

			if (!_selectedObjects.NumbersUpToDate) { SetStatusLabels("Numbers not up to date"); EndFunction(0); return; }

			SetStatusLabels("Creating Fab Package");

			bool doSeversafeOrder = false;
			// if (!_selectedObjects.CreateFabPackage(_model, _projectData, phaseNumber.Text, issueNumber.Text, StageTypes.FAB, txt_SiteDate.Text, doSeversafeOrder)) { EndFunction(0); return; }
			// FabMisc.FabMiscOp(phaseNumber.Text, issueNumber.Text, _projectData, txt_SiteDate.Text, _selectedObjects);

			EndFunction(1);
		}

		private void StartFunction()
		{
			Cursor = Cursors.AppStarting;
			flowLayoutPanel1.Enabled = false;
			ModelModifiers.ResetWorkPlane(_model);
		}

		private void EndFunction(int cancelledOrComplete, bool isOmit = false) //0 = cancelled 1 = Complete
		{
			if (!isOmit && _selectedObjects != null && _selectedObjects.PrismParts != null && cancelledOrComplete != 0) _selectedObjects.PrismParts.SelectParts();
			string message = cancelledOrComplete == 0 ? "Cancelled" : "Complete";
			flowLayoutPanel1.BackColor = cancelledOrComplete == 0 ? Color.Tomato : Color.PaleGreen;
			flowLayoutPanel1.Enabled = true;
			SetStatusLabels(message);
			Cursor = Cursors.Default;
		}

		private void InitializePrism()
		{
			string key = Prism.Properties.Settings.Default.UniqueId;

			if (key == "Default" && Environment.UserDomainName != "SFRPLC")
			{
				var form = new AuthKeyInput();
				form.ShowDialog();
				key = form.AuthKeyInputString;
			}
			_webService = WebService.SetupWebService(key);

			if (!_webService.ValidUser()) { Application.Exit(); return; }

			Prism.Properties.Settings.Default.UniqueId = key;
			Prism.Properties.Settings.Default.Save();


			//if webservice is a succes save the key
			if (!CheckModelConnection()) return;

			CheckSpecialUser();

			CompleteSetup();
		}

		private static string ReadTeklaVersion()
		{
			var version = "2021.0";

			try
			{
				var name = System.Reflection.Assembly.GetCallingAssembly().GetName();
				using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Severfield\\" + name.Name + "\\"))
				{
					if (key != null)
					{
						var o = key.GetValue("TeklaVersion");
						if (o != null)
						{
							version = o as string;
						}
					}
				}
			}
			catch (System.Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			return version;
		}

		private void CompleteSetup()
		{
			Logging.LoginMessage(_projectData.ProjName, "Login Succesful");
			_teklaVersion = ReadTeklaVersion();
			Logging.CreateModelLog(_projectData);
			SetNextPrelimToUseLabel();
			SetStatusLabels($"Connected to: {_projectData.ProjNumber}-{_projectData.ProjName}");
		}

		private void CheckSpecialUser()
		{
			if (Constants.SpecialOperationUser())
			{
				btn_SpecialOperations.BackgroundImage = Resources.Gears;
				btn_SpecialOperations.Enabled = true;
				cmb_FabIssueNo.Visible = true;
			}
		}

		private bool CheckModelConnection()
		{
			_model = new Model();
			if (!_model.GetConnectionStatus())
			{
				MessageBox.Show("Failed to connect to a correct version of Tekla Model");
				Logging.LoginMessage("", "Login Failed");
				Application.Exit();
				return false;
			}
			_projectData = new PrismProjectData(_model.GetProjectInfo(), _model.GetInfo().ModelPath, _webService);
			return true;
		}

		private async Task<bool> ProcessSpecialFittings(string orderType, ToolStrip toolStrip, ToolStripStatusLabel tssl)
		{
			ReportManager myReportManager = new ReportManager(_projectData, txt_MaterialPhaseNumber.Text, txt_MaterialIssueNumber.Text);
			if (orderType.Contains("Omit"))
			{

				if (!myReportManager.Folders.CreateMatFolder(false)) { EndFunction(0); return false; };
				myReportManager.CreateMaterialReports(_selectedObjects, orderType, StageTypes.Prelim3);
				if (!MaterialButton3.FinishOrder(_selectedObjects, (int)StageTypes.Prelim3, _projectData, myReportManager.MatReportPrefix,
					orderType, myReportManager, txt_MatSiteDate.Text, toolStrip, tssl)) { return false; }
			}
			else
			{
				int orderAction = PrismWarnings.SpecialFittingOrder(_model);

				if (!await Task.Run(() => InitialSetup(StageTypes.Prelim3, false))) { EndFunction(0); return false; };
				if (orderAction == 2 || orderAction == 3) //then user wants to create a material order
				{
					if (!OrderSpecials(orderAction, orderType, StageTypes.Prelim3, myReportManager, toolStrip, tssl)) { EndFunction(0); return false; }
					if (!MaterialButton3.FinishOrder(_selectedObjects, (int)StageTypes.Prelim3, _projectData, myReportManager.MatReportPrefix,
					   orderType, myReportManager, txt_MatSiteDate.Text, toolStrip, tssl, true)) { return false; }
				}
				if (orderAction == 4 || orderAction == 5) //then user wants to make drawings
				{
					//if order action == 4 then the user wants to run drawings on just the tagged stuff.
					List<PrismPart> selectedParts = orderAction == 4 ? ModelModifiers.SelectSpecialTaggedInSelection(_selectedObjects) : _selectedObjects.PrismParts;
					if (!ModelModifiers.PerformNumbering()) return false;
					ModelModifiers.CreateDrawings(selectedParts);
				}
			}
			return true;
		}

		private void txt_StartNumber_TextChanged(object sender, EventArgs e)
		{
			if (txt_StartNumber.Text.Length > 0)
			{
				txt_StartNumber.BackColor = Color.White;
				btn_Material2.BackColor = Color.Gold;
				btn_Material2.Enabled = true;
			}
			else
			{
				txt_StartNumber.BackColor = Color.LightCoral;
				btn_Material2.BackColor = Color.Gainsboro;
				btn_Material2.Enabled = false;
			}
		}

		private void txt_StartNumber_KeyPress(object sender, KeyPressEventArgs e)
		{
			AllowNumbersAndDeleteOnly(e);
		}

		private void txt_MaterialPhaseNumber_TextChanged(object sender, EventArgs e)
		{
			if (txt_MaterialPhaseNumber.Text.Length > 0)
			{
				txt_MaterialPhaseNumber.BackColor = Color.White;
			}
			else
			{
				txt_MaterialPhaseNumber.BackColor = Color.LightCoral;
			}
			CheckForMaterialButton();
		}

		private void txt_MaterialIssueNumber_KeyPress(object sender, KeyPressEventArgs e)
		{
			AllowNumbersAndDeleteOnly(e);
		}

		private void txt_MaterialIssueNumber_TextChanged(object sender, EventArgs e)
		{
			if (txt_MaterialIssueNumber.Text.Length > 1)
			{
				txt_MaterialIssueNumber.BackColor = Color.White;
			}
			else
			{
				txt_MaterialIssueNumber.BackColor = Color.LightCoral;
			}
			CheckForMaterialButton();
		}

		private void phaseNumber_TextChanged(object sender, EventArgs e)
		{
			if (phaseNumber.Text.Length > 0)
			{
				phaseNumber.BackColor = Color.White;
			}
			else
			{
				phaseNumber.BackColor = Color.LightCoral;
			}
			CheckForFabButton();
			CheckForBoltOrderButton();

			if (Constants.SpecialOperationUser())
			{
				ChangeHelper.PopulateIssueNumbers(ref cmb_FabIssueNo, ref phaseNumber, Constants.ModelDataLogLocation(_projectData.ProjNumberAndGuid + "\\FAB XMLs"));
			}
		}

		private void issueNumber_TextChanged(object sender, EventArgs e)
		{
			if (issueNumber.Text.Length > 1)
			{
				issueNumber.BackColor = Color.White;
			}
			else
			{
				issueNumber.BackColor = Color.LightCoral;
			}
			CheckForFabButton();
			CheckForBoltOrderButton();
		}

		private void issueNumber_KeyPress(object sender, KeyPressEventArgs e)
		{
			AllowNumbersAndDeleteOnly(e);
		}

		private void txt_SiteDate_TextChanged(object sender, EventArgs e)
		{
			CheckForAcceptableSiteDate(ref txt_SiteDate);
		}

		private void CheckForAcceptableSiteDate(ref TextBox textBox)
		{
			if (textBox.Text.Length > 7)
			{
				textBox.BackColor = Color.White;
			}
			else
			{
				textBox.BackColor = Color.Moccasin;
			}
		}

		private void userGuideToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Help.Open(_projectData.WebService);
		}

		private void PrismUI_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			Cursor = Cursors.Help;
		}

		private void info_Mat_Click(object sender, EventArgs e)
		{
			Help.OpenAt("Material Checks", "Index", _projectData.WebService);
		}

		private void info_Detail_Click(object sender, EventArgs e)
		{
			Help.OpenAt("Detailing Checks", "Index", _projectData.WebService);
		}

		private void info_Fab_Click(object sender, EventArgs e)
		{
			Help.OpenAt("Fabrication Packaging", "Index", _projectData.WebService);
		}

		private void info_Home_Click(object sender, EventArgs e)
		{
			Help.OpenAt("Home", "Index", _projectData.WebService);
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void cmb_OrderMaterial_SelectedIndexChanged(object sender, EventArgs e)
		{
			CheckForMaterialButton();
		}

		private void cmb_ColumnOrientationType_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cmb_ColumnOrientationType.Text == "Holes and plate")
			{
				txt_PlateOnFlange.Visible = true;
				lbl_PltOnFlange.Visible = true;
				btn_Detail2.Enabled = false;
				btn_Detail2.BackColor = Color.Gainsboro;
			}
			else
			{
				txt_PlateOnFlange.Text = "";
				txt_PlateOnFlange.Visible = false;
				lbl_PltOnFlange.Visible = false;
				btn_Detail2.Enabled = true;
				btn_Detail2.BackColor = Color.Gold;
			}
		}

		private void txt_PlateOnFlange_TextChanged(object sender, EventArgs e)
		{
			if (txt_PlateOnFlange.Text.Length > 0)
			{
				txt_PlateOnFlange.BackColor = Color.White;
				btn_Detail2.BackColor = Color.Gold;
				btn_Detail2.Enabled = true;
			}
			else
			{
				txt_PlateOnFlange.BackColor = Color.LightCoral;
				btn_Detail2.BackColor = Color.Gainsboro;
				btn_Detail2.Enabled = false;
			}
		}

		private void btn_HomeDetail_Click_1(object sender, EventArgs e)
		{
			pnl_Home.Visible = true;
			pnl_Detail.Visible = false;
			flowLayoutPanel1.BackColor = Color.DodgerBlue;
		}

		private void btn_HomePackage_Click_1(object sender, EventArgs e)
		{
			pnl_Home.Visible = true;
			pnl_Package.Visible = false;
			flowLayoutPanel1.BackColor = Color.DodgerBlue;
		}

		private void btn_HomeMaterial_Click_1(object sender, EventArgs e)
		{
			pnl_Home.Visible = true;
			pnl_Material.Visible = false;
			flowLayoutPanel1.BackColor = Color.DodgerBlue;
		}

		private void btn_MainMaterialCheck_Click_1(object sender, EventArgs e)
		{
			pnl_Material.Visible = true;
			pnl_Home.Visible = false;
		}

		private void btn_MainPackageCreation_Click_1(object sender, EventArgs e)
		{
			pnl_Home.Visible = false;
			pnl_Package.Visible = true;
		}

		private void btn_MainDetailCheck_Click_1(object sender, EventArgs e)
		{
			pnl_Home.Visible = false;
			pnl_Detail.Visible = true;
		}

		private void SetStatusLabels(string message)
		{
			MaterialStatusLabel.Text = message;
			DetailingStatusLabel.Text = message;
			StatusLabel.Text = message;
		}

		private void CheckForFabButton()
		{
			if (phaseNumber.Text.Length > 0 & issueNumber.Text.Length > 1)
			{
				btnCreatePackage1.Enabled = true;
				btnCreatePackage1.BackColor = Color.Chartreuse;
				btn_RocketPacket.Enabled = true;
				btn_RocketPacket.BackColor = Color.Chartreuse;
			}
			else
			{
				btnCreatePackage1.Enabled = false;
				btnCreatePackage1.BackColor = Color.White;
				btn_RocketPacket.Enabled = false;
				btn_RocketPacket.BackColor = Color.White;
			}
		}

		private void CheckForBoltOrderButton()
		{
			if (phaseNumber.Text.Length > 0 & issueNumber.Text.Length > 1)
			{
				btn_FabMisc.Enabled = true;
				btn_FabMisc.BackColor = Color.Chartreuse;
			}
			else
			{
				btn_FabMisc.Enabled = false;
				btn_FabMisc.BackColor = Color.Gainsboro;
			}
		}

		private void CheckForMaterialButton()
		{
			if (txt_MaterialIssueNumber.Text.Length > 1 && txt_MaterialPhaseNumber.Text.Length > 0 && cmb_OrderMaterial.Text != "Order HD Bolts")
			{
				btn_Material3.Enabled = true;
				btn_Material3.BackColor = Color.Chartreuse;
			}
			else
			{
				btn_Material3.Enabled = false;
				btn_Material3.BackColor = Color.Gainsboro;
			}
		}

		private void AllowNumbersAndDeleteOnly(KeyPressEventArgs e)
		{
			char delete = new char();
			delete = '\b'; //This is the code created when backspace is pressed.
			if (!Char.IsNumber(e.KeyChar) && (e.KeyChar != delete)) //If the key press is neither a number or delete key then ignore.
			{
				e.Handled = true;
				return;
			}
		}

		private void projectUsersToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var form = new ProjectControllers(_projectData.ProjNumberAndGuid, _webService, _model.GetProjectInfo());
			form.ShowDialog();
		}

		private void SetNextPrelimToUseLabel()
		{
			lbl_NextPrelim.Text = Logging.GetLastUsedPrelim(_projectData.ProjNumberAndGuid).ToString();
		}

		private void btn_PrelimLabelRefresh_Click(object sender, EventArgs e)
		{
			SetNextPrelimToUseLabel();
		}

		private void uniClassCodesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var form = new UniClass_Codes(_projectData.ProjNumberAndGuid);
			form.ShowDialog();
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var form = new About();
			form.ShowDialog();
		}

		private void txt_MatSiteDate_TextChanged(object sender, EventArgs e)
		{
			CheckForAcceptableSiteDate(ref txt_MatSiteDate);
		}

		private void advancedSettingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ResetPrelim resetPrelim = new ResetPrelim(_projectData);
			resetPrelim.ShowDialog();
			SetNextPrelimToUseLabel();
		}

		private void PrismUI_Shown(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			Update();
			InitializePrism();
			pnl_Home.BackgroundImage = Resources.watereddownlogo;
			Cursor = Cursors.Default;
		}

		private void miscToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AdvancedSettings advancedSettings = new AdvancedSettings(_projectData);
			if (!advancedSettings.CloseForm)
			{
				advancedSettings.ShowDialog();
			}
		}

		private void txt_VariationNumber_TextChanged(object sender, EventArgs e)
		{
			CheckForVariationNumber(ref txt_VariationNumber, ref cmb_VariationType);
		}

		private void CheckForVariationNumber(ref TextBox textBox, ref ComboBox combobox)
		{
			if (textBox.Text.Length > 0)
			{
				textBox.BackColor = Color.White;
				combobox.BackColor = Color.White;
			}
			else
			{
				textBox.BackColor = Color.Moccasin;
				combobox.BackColor = Color.Moccasin;
			}
		}

		private void txt_VariationNumber_KeyPress(object sender, KeyPressEventArgs e)
		{
			AllowNumbersAndDeleteOnly(e);
		}

		private void txt_Variation_Fab_TextChanged(object sender, EventArgs e)
		{
			CheckForVariationNumber(ref txt_Variation_Fab, ref cmb_VariationType_Fab);
		}

		private void txt_MaterialPhaseNumber_KeyPress(object sender, KeyPressEventArgs e)
		{
			AllowNumbersAndDeleteAndSW(e);
		}

		private void phaseNumber_KeyPress(object sender, KeyPressEventArgs e)
		{
			AllowNumbersAndDeleteAndSW(e);
		}

		private void AllowNumbersAndDeleteAndSW(KeyPressEventArgs e)
		{
			char delete = '\b'; //This is the code created when backspace is pressed.

			// Check if the pressed key is a number, delete key, or 's'/'w'
			if (!Char.IsNumber(e.KeyChar) && e.KeyChar != delete && e.KeyChar != 'S' && e.KeyChar != 'W' && e.KeyChar != '-')
			{
				e.Handled = true; // If it's not a number, delete, 's', or 'w', ignore the input.
				return;
			}
		}

	}
}