using System.IO;
using Tekla.Structures.Model.Operations;
using System.Collections.Generic;
using static Prism.Enums;
using System.Drawing.Printing;
using System.Drawing;
using System;
using System.Linq;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace Prism
{
	/// <summary>
	/// The Report Manager class is where all reports are created.
	/// NC files are also created from here.
	/// </summary>
	public class ReportManager
	{
		private string _title1;
		private string _title2;
		private string _title3;

		#region group accepted reports
		//Material Procurement
		private const string _output1Pname = "-1-PrelimHotRolledMemList.xsr";
		private string _report1Pname = $"{_output1Pname.Replace("xsr", "rpt")}";
		private const string _output1PAname = "-1a-PrelimHotRolledMemList-ADD.xsr";
		private string _report1PAname = $"{_output1PAname.Replace("xsr", "rpt")}";
		private const string _output1POname = "-1o-PrelimHotRolledMemList-OMIT.xsr";
		private string _report1POname = $"{_output1POname.Replace("xsr", "rpt")}";

		private const string _output1PFname = "-1F-PrelimSpecialFitList.xsr";
		private string _report1PFname = $"{_output1PFname.Replace("xsr", "rpt")}";
		private const string _output1PFAname = "-1Fa-PrelimSpecialFitList-ADD.xsr";
		private string _report1PFAname = $"{_output1PFAname.Replace("xsr", "rpt")}";
		private const string _output1PFOname = "-1Fo-PrelimSpecialFitList-OMIT.xsr";
		private string _report1PFOname = $"{_output1PFOname.Replace("xsr", "rpt")}";
		public const string _g2ReportOutput = "-G2_Assy7.xsr";
		public string _g2ReportName = $"{_g2ReportOutput.Replace("xsr", "rpt")}";

		//Bolt ordering
		private const string _outputBolts = "-SEV-BOLTS-STRUMIS-SUMMARY_v3.xsr";
		private const string _reportBoltsName = "-SEV-BOLTS-STRUMIS-SUMMARY_v3.rpt";

		private const string _outputSelectedBolts = "-SEV-BOLTS-STRUMIS-ONLY-SELECTED_v1.xsr";
		private const string _reportSelectedBoltsName = "-SEV-BOLTS-STRUMIS-ONLY-SELECTED_v1.rpt";

		private const string _output5OName = "-5o-Bolt-OMIT.xsr";
		private string _report5OName = $"{_output5OName.Replace("xsr", "rpt")}";
		private const string _output5ONameSelected = "-5o-Bolt-OMIT-SelectedOnly.xsr";
		private string _report5ONameSelected = $"{_output5ONameSelected.Replace("xsr", "rpt")}";

		//Fab Packages
		private const string _output2Name = "-2-HotRolledMemList.xsr";
		private string _report2Name = $"{_output2Name.Replace("xsr", "rpt")}";
		private const string _output2AName = "-2a-HotRolledMemList-ADD.xsr";
		private string _report2AName = $"{_output2AName.Replace("xsr", "rpt")}";
		private const string _output2OName = "-2o-HotRolledMemList-OMIT.xsr";
		private string _report2OName = $"{_output2OName.Replace("xsr", "rpt")}";
		private const string _output2PgName = "-2PG-HotRolledPgMemList.xsr";
		private string _report2PgName = $"{_output2PgName.Replace("xsr", "rpt")}";

		private const string _output3Name = "-3-HotRolledFitList.xsr";
		private string _report3Name = $"{_output3Name.Replace("xsr", "rpt")}";
		private const string _output3AName = "-3a-HotRolledFitList-ADD.xsr";
		private string _report3AName = $"{_output3AName.Replace("xsr", "rpt")}";
		private const string _output3OName = "-3o-HotRolledFitList-OMIT.xsr";
		private string _report3OName = $"{_output3OName.Replace("xsr", "rpt")}";

		private const string _output4Name = "-4-ShopBoltList-SelectedOnly.xsr";
		private string _report4Name = $"{_output4Name.Replace("xsr", "rpt")}";
		private const string _output4AName = "-4a-ShopBoltList-ADD.xsr";
		private string _report4AName = $"{_output4AName.Replace("xsr", "rpt")}";
		private const string _output4OName = "-4o-ShopBoltList-OMIT.xsr";
		private string _report4OName = $"{_output4OName.Replace("xsr", "rpt")}";
		private const string _output4LName = "-4l-ShopBoltLocationList.xsr";
		private string _report4LName = $"{_output4LName.Replace("xsr", "rpt")}";

		private const string _output5Name = "-5-SiteBoltList-SelectedOnly.xsr";
		private string _report5Name = $"{_output5Name.Replace("xsr", "rpt")}";
		private const string _output5AName = "-5a-SiteBoltList-ADD.xsr";
		private string _report5AName = $"{_output5AName.Replace("xsr", "rpt")}";

		private const string _output5LName = "-5l-SiteBoltLocationList.xsr";
		private string _report5LName = $"{_output5LName.Replace("xsr", "rpt")}";

		private const string _output6Name = "-6-AssemblyList.xsr";
		private string _report6Name = $"{_output6Name.Replace("xsr", "rpt")}";

		//Extras
		private const string _reportQSname = "-QSreport.csv.rpt";
		private const string _outputQSname = "-QSreport.csv";
		private const string _report7Name = "-7-FusionMap.csv.rpt";
		private const string _output7Name = "-FusionMap.csv";
		#endregion

		private bool create3PGReport = false;
		public PrismProjectData ProjectData;
		public string PhaseNum;
		public string IssueNum;

		public ReportManager(PrismProjectData projectData, string phaseNum, string issueNum)
		{
			ProjectData = projectData;
			PhaseNum = phaseNum;
			IssueNum = issueNum;
			Folders = new FolderManager(projectData, phaseNum, issueNum);
			FabReportPrefix = ($"{projectData.ProjNumber}-{phaseNum}-FAB-ISSUE{issueNum}");
			MatReportPrefix = ($"{projectData.ProjNumber}-{phaseNum}-PRELIM-ISSUE{issueNum}");
			EpoReportPrefix = ($"{projectData.ProjNumber}-{phaseNum}-EPO-ISSUE{issueNum}");
			CarcassReportPrefix = ($"{projectData.ProjNumber}-{phaseNum}-FABSEC-ISSUE{issueNum}");
			BoltReportPrefix = $"{projectData.ProjNumber}-{phaseNum}-BOLT-ISSUE{issueNum}";
			_title1 = phaseNum;
			_title2 = projectData.Initials;
			_title3 = issueNum;
		}

		public FolderManager Folders;
		public readonly string FabReportPrefix;
		public readonly string MatReportPrefix;
		public readonly string EpoReportPrefix;
		public readonly string BoltReportPrefix;
		public readonly string CarcassReportPrefix;

		public void CreateMaterialReports(SelectedObjects selectedObjects, string orderType, StageTypes stageType, ToolStrip toolStrip, ToolStripStatusLabel tssl)
		{
			if (!orderType.Contains("Order Bolts"))
			{
				bool fabsecPresent = selectedObjects.GetFabsecParts().Count > 0;
				bool nonFabsecPresent = selectedObjects.GetNonFabsecParts().Count > 0;

				switch (orderType)
				{
					case "Order Material":
						ProcessMaterial(selectedObjects, stageType, true, fabsecPresent, nonFabsecPresent,
							_report1Pname, _output1Pname, toolStrip, tssl, _report2PgName, _output2PgName);
						break;

					case "Add Material":
						ProcessMaterial(selectedObjects, stageType, true, fabsecPresent, nonFabsecPresent,
							_report1PAname, _output1PAname, toolStrip, tssl, _report2PgName, _output2PgName);
						break;

					case "Omit Material":
						ProcessMaterial(selectedObjects, stageType, false, fabsecPresent, nonFabsecPresent,
							_report1POname, _output1POname, toolStrip, tssl, _report2PgName, _output2PgName);
						break;

					case "Order Special Fittings":
						ProcessMaterial(selectedObjects, stageType, true, false, nonFabsecPresent,
							  _report1PFname, _output1PFname, toolStrip, tssl);
						break;

					case "Add Special Fittings":
						ProcessMaterial(selectedObjects, stageType, true, false, nonFabsecPresent,
							_report1PFAname, _output1PFAname, toolStrip, tssl);
						break;

					case "Omit Special Fittings":
						ProcessMaterial(selectedObjects, stageType, false, false, nonFabsecPresent,
							_report1PFOname, _output1PFOname, toolStrip, tssl);
						break;

					default:
						throw new ArgumentException($"Unknown order type: {orderType}");
				}

				TextToPDF(Folders.MatPath);
			}
		}

		private async void ProcessMaterial(SelectedObjects selectedObjects, StageTypes stageType, bool createBSWX, bool fabsecPresent, bool nonFabsecPresent, string nonFabsecReportName,
			string nonFabsecOutputName, ToolStrip toolStrip, ToolStripStatusLabel tssl, string fabsecReportName = null, string fabsecOutputName = null)
		{
			if (createBSWX)
			{
				await selectedObjects.ExportBSWX(Folders.MatPath, ProjectData, PhaseNum, IssueNum, stageType, toolStrip, tssl);
				ModelModifiers.RemoveLog(Folders.MatPath);
			}

			if (nonFabsecPresent && !string.IsNullOrEmpty(nonFabsecReportName) && !string.IsNullOrEmpty(nonFabsecOutputName))
			{
				CreateReport(nonFabsecReportName, nonFabsecOutputName);
			}

			if (fabsecPresent && !string.IsNullOrEmpty(fabsecReportName) && !string.IsNullOrEmpty(fabsecOutputName))
			{
				CreateReport(fabsecReportName, fabsecOutputName);
			}
		}

		private void CreateReport(string reportTemplateName, string outputFileName)
		{
			string reportPath = Path.Combine(FirmFolderLoc.ReportTemplates(), reportTemplateName);
			string outputPath = Path.Combine(Folders.MatPath, $"{MatReportPrefix}{outputFileName}");

			Operation.CreateReportFromSelected(reportPath, outputPath, _title1, _title2, _title3);
		}

		public void CreateG2Assy()
		{
			string g2ReportName = Path.Combine(FirmFolderLoc.ReportTemplates(), _g2ReportName);
			Operation.CreateReportFromSelected(g2ReportName, Path.Combine(Folders.CarcassOrderPath, $"{CarcassReportPrefix}{_g2ReportOutput}"), _title1, _title2, _title3);
		}

		public void CreateBoltList(string reportPrefix, string orderType)
		{
			string boltReportName = _reportBoltsName;
			string boltListOutputName = _outputBolts;

			if (orderType.Contains("Omit")) { boltReportName = _report5OName; boltListOutputName = _output5OName; }

			string boltReport = Path.Combine(FirmFolderLoc.ReportTemplates(), boltReportName);
			Operation.CreateReportFromSelected(boltReport, Path.Combine(Folders.BoltPath, $"{reportPrefix}{boltListOutputName}"), _title1, _title2, _title3);
			//  TextToPDF(Folders.BoltPath);
		}

		public void CreateSelectedBoltList(string reportPrefix, string orderType)
		{
			string boltReportName = _reportSelectedBoltsName;
			string boltListOutputName = _outputSelectedBolts;

			if (orderType.Contains("Omit")) { boltReportName = _report5ONameSelected; boltListOutputName = _output5ONameSelected; }

			string reportBolts = Path.Combine(FirmFolderLoc.ReportTemplates(), boltReportName);
			Operation.CreateReportFromSelected(reportBolts, Path.Combine(Folders.BoltPath, $"{reportPrefix}{boltListOutputName}"), _title1, _title2, _title3);
			// TextToPDF(Folders.BoltPath);
		}

		public void CreateHDBoltList()
		{

		}

		public async Task CreateFabReports(SelectedObjects myObjects, List<PrismBoltGroup> boltList, string teklaVersion, ToolStrip toolStrip, ToolStripStatusLabel statusLabel)
		{
		/*	while (Operation.IsMacroRunning()) // Wait until macro for selecting drawings in the document manager is complete before moving on
			{
				await System.Threading.Tasks.Task.Delay(10);
			}*/

			bool create3Report = false;
			bool create4Report = false;
			bool createPgReport = false;
			string sectionSize;

			bool shopBoltsPresent = false;
			bool siteBoltsPresent = false;
			if (boltList.Count > 0)
			{
				shopBoltsPresent = boltList.Any(pbg => pbg != null && pbg.isShop && !pbg.isShearStud && !pbg.isOrdered);   //is a shop bolt but not a shear stud
				siteBoltsPresent = boltList.Any(pbg => !pbg.isShop && !pbg.isShearStud && !pbg.isOrdered); //Is neither shop bolt or shear stud
			}
			
			List<PrismPart> nonSeversafeParts = myObjects.GetNonSeversafeParts();
			
			foreach (PrismPart part in nonSeversafeParts)
			{
				sectionSize = part.Part.Profile.ProfileString.Substring(0, 2);
				bool isFitting = sectionSize == "PL" || sectionSize == "RS" || sectionSize == "FL";
				bool isPlateGirder = sectionSize == "PG";
				if (!isFitting && !isPlateGirder) create3Report = true;
				if (isPlateGirder) createPgReport = true;
				if (isFitting) create4Report = true;
			}

			UpdateStatusLabel(toolStrip, statusLabel, "Creating Reports");
			await Task.Yield();
			await Task.Run(() => CreateReports(boltList, create3Report, create4Report, createPgReport, shopBoltsPresent, siteBoltsPresent));

			UpdateStatusLabel(toolStrip, statusLabel, "Creating NC Data");
			await Task.Yield(); // Allow UI to update
			await Task.Run(() => CreateNC(teklaVersion, myObjects));

			UpdateStatusLabel(toolStrip, statusLabel, "Converting Reports To PDF");
			await Task.Yield();
			await Task.Run(() => TextToPDF(Folders.ReportPath));

			/*	UpdateStatusLabel(toolStrip, statusLabel, "Creating NC Data");

				CreateNC(teklaVersion);

				UpdateStatusLabel(toolStrip, statusLabel, "Creating Reports");

				CreateReports(boltList, create3Report, create4Report, createPgReport, shopBoltsPresent, siteBoltsPresent);

				UpdateStatusLabel(toolStrip, statusLabel, "Converting Reports To PDF");

				TextToPDF(Folders.ReportPath);*/

			ModelModifiers.SelectParts(nonSeversafeParts);
		}

		private void UpdateStatusLabel(ToolStrip toolStrip, ToolStripStatusLabel statusLabel, string labelMessage)
		{
			toolStrip.Invoke(new System.Action(() =>
			{
				statusLabel.Text = $"{labelMessage}";

			}));
		}

		private void CreateReports(List<PrismBoltGroup> boltList, bool create3Report, bool create4Report, bool createPgReport, bool shopBoltsPresent, bool siteBoltsPresent)
		{
			string qsReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _reportQSname);
			string hrMemberReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report2Name);
			string pgMemberReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report2PgName);
			string hrFittingReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report3Name);
			string shopBoltReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report4Name);
			string siteBoltReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report5Name);
			string assemblyReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report6Name);
			string fusionMapReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _report7Name);
			string boltReport = Path.Combine(FirmFolderLoc.ReportTemplates(), _outputSelectedBolts);

			Operation.CreateReportFromSelected(qsReport, Path.Combine(Folders.DspPath, $"{FabReportPrefix}{_outputQSname}"), _title1, _title2, _title3);
			Operation.CreateReportFromSelected(assemblyReport, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output6Name}"), _title1, _title2, _title3);
			// Operation.CreateReportFromSelected(fusionMapReport, Path.Combine(Folders.DspPath, $"{FabReportPrefix}{_output7Name}"), _title1, _title2, _title3);

			if (create3Report)
			{
				Operation.CreateReportFromSelected(hrMemberReport, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output2Name}"), _title1, _title2, _title3);
			}
			if (createPgReport)
			{
				Operation.CreateReportFromSelected(pgMemberReport, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output2PgName}"), _title1, _title2, _title3);
			}
			if (create4Report)
			{
				Operation.CreateReportFromSelected(hrFittingReport, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output3Name}"), _title1, _title2, _title3);
			}
			if (shopBoltsPresent || siteBoltsPresent) //then create our strumis summary report
			{
				ModelModifiers.SelectBolts(boltList.Where(pbg => !pbg.isOrdered && !pbg.isShearStud).Select(pbg => pbg.BoltGroup).ToList());
				Operation.CreateReportFromSelected(boltReport, Path.Combine(Folders.DspPath, $"{FabReportPrefix}{_outputSelectedBolts}"), _title1, _title2, _title3);
			}
			if (shopBoltsPresent) //Then create a shop bolts summary
			{
				ModelModifiers.SelectBolts(boltList.Where(pbg => !pbg.isOrdered && !pbg.isShearStud && pbg.isShop).Select(pbg => pbg.BoltGroup).ToList());
				Operation.CreateReportFromSelected(shopBoltReport, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output4Name}"), _title1, _title2, _title3);
			}
			if (siteBoltsPresent) //Then create a site bolts summary
			{
				ModelModifiers.SelectBolts(boltList.Where(pbg => !pbg.isOrdered && !pbg.isShearStud && !pbg.isShop).Select(pbg => pbg.BoltGroup).ToList());
				Operation.CreateReportFromSelected(siteBoltReport, Path.Combine(Folders.ReportPath, $"{FabReportPrefix}{_output5Name}"), _title1, _title2, _title3);
			}
		}

		private void CreateNC(string version, SelectedObjects myObjects)
		{
			//The name of the settings used changed from tekla 2021 -> 2023, so, we set up and run both here,
			if (version.Contains("2021"))
			{
				//2021 NC
				string plateSetting2021 = "-SNI-PLATES";
				string profileSetting2021 = "-SNI-PROFILES";
				// string shpRolePlates = "DSTV for plates";
				//  string shpRoleForProfiles = "DSTV for profiles";
				Operation.CreateNCFilesFromSelected(plateSetting2021, Folders.NcPath + "\\", true);
				Operation.CreateNCFilesFromSelected(profileSetting2021, Folders.NcPath + "\\", true);
				//   Operation.CreateNCFilesFromSelected(shpRolePlates, Folders.NcPath + "\\", false, "", true);
				//   Operation.CreateNCFilesFromSelected(shpRoleForProfiles, Folders.NcPath + "\\", false, "", true);
			}
			else
			{
				//2023 NC
				string platesSec2023 = "-SEV-PLATES-SEC";
				string profilesMain2023 = "-SEV-PROFILES-MAIN";
				string profilesSec2023 = "-SEV-PROFILES-SEC";
				string profilesHollow2023 = "-SEV-PROFILES-MAIN-HOLLOW";

				ModelModifiers.SelectParts(myObjects.GetSecondaryParts());
				Operation.CreateNCFilesFromSelected(platesSec2023, Folders.NcPath + "\\", false, "", true);
				Operation.CreateNCFilesFromSelected(profilesSec2023, Folders.NcPath + "\\", false, "", true);

				ModelModifiers.SelectParts(myObjects.GetMainParts());
				Operation.CreateNCFilesFromSelected(profilesHollow2023, Folders.NcPath + "\\", true, "", true);
				Operation.CreateNCFilesFromSelected(profilesMain2023, Folders.NcPath + "\\", true, "", true);
			}
			// Wait for the folder to have contents or timeout after 10 seconds
			WaitForFolderContents(Folders.NcPath, TimeSpan.FromSeconds(30));
		}

		private void WaitForFolderContents(string folderPath, TimeSpan timeout)
		{
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();
			while (stopwatch.Elapsed < timeout)
			{
				if (Directory.GetFiles(folderPath).Length > 0)
				{
					break;
				}
				System.Threading.Tasks.Task.Delay(500).Wait(); // Wait for 500 milliseconds before checking again
			}
		}

		public static async void SelectDrawingsInDocManager(List<PrismPart> selectedParts)
		{
			if (selectedParts != null) selectedParts.SelectParts();

			await PrismMacroBuilder.DrawingOperations();
		}

		private static void TextToPDF(string folderPath)
		{
			foreach (string subFile in Directory.GetFiles(folderPath))
			{
				if (subFile.EndsWith(".xsr") && !subFile.Contains("G2"))
				{
					VirtualPrinter(subFile);
					File.Delete(subFile);
				}
			}
		}

		private static void VirtualPrinter(string filePath)
		{
			Font font = new Font("Lucida Console", 10, FontStyle.Regular);

			string printerName = "Microsoft Print to PDF"; // name of the printer

			PrintDocument printDocument = new PrintDocument();
			printDocument.PrinterSettings.PrinterName = printerName;

			printDocument.PrinterSettings.PrintToFile = true;
			printDocument.PrinterSettings.PrintFileName = Path.ChangeExtension(filePath, "pdf");
			printDocument.DefaultPageSettings.PaperSize = new PaperSize("A4", 2100, 2970);
			printDocument.DefaultPageSettings.Margins = new Margins(40, 40, 40, 40); // 0.5 inch margins
			printDocument.DefaultPageSettings.Landscape = false; // portrait ori0entation
			printDocument.DefaultPageSettings.Color = false; // black and white output

			printDocument.DocumentName = Path.GetFileNameWithoutExtension(filePath);

			// Variables to keep track of current position in file and number of lines printed
			int linesPerPage = 74;
			int lineNumber = 0;
			int position = 0;

			printDocument.PrintPage += (sender, e) =>
			{
				// Read a portion of the file starting from the current position
				using (StreamReader reader = new StreamReader(filePath))
				{
					string text = reader.ReadToEnd();
					string[] lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
					int linesToPrint = Math.Min(linesPerPage, lines.Length - lineNumber);
					string portion = string.Join(Environment.NewLine, lines.Skip(lineNumber).Take(linesToPrint));
					e.Graphics.DrawString(portion, font, Brushes.Black, e.MarginBounds);

					// Update variables for next page
					lineNumber += linesToPrint;
					position += portion.Length;
					if (lineNumber >= lines.Length)
					{
						e.HasMorePages = false;
					}
					else
					{
						e.HasMorePages = true;
					}
				}
			};

			printDocument.Print();
		}
	}
}