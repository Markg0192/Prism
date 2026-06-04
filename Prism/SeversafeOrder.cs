using System.Collections.Generic;
using Tekla.Structures.Model;
using Microsoft.Office.Interop.Excel;
using Prism.Geometry;
using System;
using System.Linq;
using System.IO;
using Application = Microsoft.Office.Interop.Excel.Application;
using Model = Tekla.Structures.Model.Model;
using Point = Tekla.Structures.Geometry3d.Point;
using Tekla.Structures.Geometry3d;

namespace Prism
{
	public static class SeversafeOrder
	{
		public static int Standards = 0;
		public static int ExtensionPieces = 0;
		public static int RailLengths = 0;
		public static int BottomBrickYard460x815 = 0;
		public static int BottomBrickYard920x815 = 0;
		public static int BottomBrickYard1780x815 = 0;
		public static int TopBrickYard460x815 = 0;
		public static int TopBrickYard920x815 = 0;
		public static int TopBrickYard1780x815 = 0;
		public static int ToeBoardLengths = 0;
		public static int SleeveJoints = 0;
		public static int ElbowJoints = 0;
		public static int AngleSwivelBends = 0;
		public static int EdgeTrimCradleFrames = 0;
		public static double LinMeterRun1mSystem = 0;
		public static double LinMeterRun1_8mSystem = 0;
		public static double LinMeterRun1mPhaseBreak = 0;
		public static double LinMeterRun1_8mPhaseBreak = 0;

		private static double RunningHandrailLength = 0;
		private static double KickFlatLength = 0;

		public static void CreateSeversafeOrder(Model model, List<PrismPart> selectedObjects, string siteDate, ReportManager reportManager, int divisionNo, string reportPrefix, PrismProjectData projData)
		{
			ResetAllNumbers();

			CollateOrderableParts(model, selectedObjects, projData);

			GetFullRailsRequired();

			GetFullKickFlatsRequired();

			string originalReportName = "\\\\GS Seversafe Order Form (EPO) Rev10.xlsx";
			string sourcePath = FirmFolderLoc.ReportTemplates() + originalReportName;

			string destinationPath = reportManager.Folders.EpoPath;
			string newFileName = reportManager.EpoReportPrefix + ".xlsx";


			CopyRenameAndWrite(reportManager.ProjectData, reportManager.PhaseNum, siteDate, sourcePath, destinationPath, newFileName, divisionNo);

			reportManager.Folders.ZipFolder(reportManager.Folders.EpoPath);
			EmailWriter.WriteEpoEmail(reportManager.ProjectData, reportPrefix, reportManager.IssueNum, reportManager.PhaseNum, siteDate, reportManager.Folders.EpoPath);
		}

		public static void CopyRenameAndWrite(PrismProjectData projData, string phaseNumber, string deliveryDate, string sourcePath, string destinationPath, string newFileName, int divisionNo)
		{
			try
			{
				if (File.Exists(sourcePath))
				{
					// Create destination directory if it doesn't exist
					Directory.CreateDirectory(destinationPath);

					// Combine destination path with new file name
					string destinationFilePath = Path.Combine(destinationPath, newFileName);

					// Copy the file to the new location
					File.Copy(sourcePath, destinationFilePath);

					// Call the ExcelWriter method to write data to the copied file
					ExcelWriter(projData, phaseNumber, deliveryDate, destinationFilePath, divisionNo);

					Console.WriteLine("File copied and modified successfully!");
				}
				else
				{
					Console.WriteLine("Source file not found!");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
			}
		}

		private static void ExcelWriter(PrismProjectData projData, string phaseNumber, string deliveryDate, string filePath, int divisionNo)
		{
			Application excelApp = new Application();
			Workbook workbook = excelApp.Workbooks.Open(filePath);

			try
			{
				Worksheet worksheet = workbook.Sheets["Sheet1"] as Worksheet;
				string projectNumber = projData.ProjNumber;
				string projectName = projData.ProjName;
				string projectAddress = projData.pInfo.Address;
				string postCode = projData.pInfo.PostalCode;
				string prepBy = projData.Initials.First() + "." + projData.Last;
				string date = DateTime.Now.ToString("MM/dd/yyyy");

				worksheet.Range["J3:L3"].Value = projectNumber;
				worksheet.Range["N3:S3"].Value = phaseNumber;
				worksheet.Range["C7:H7"].Value = projectName;
				worksheet.Range["C8:H8"].Value = projectAddress;
				worksheet.Range["F9:H9"].Value = postCode;
				worksheet.Range["J8:M8"].Value = prepBy;
				worksheet.Range["L9:M9"].Value = deliveryDate;
				worksheet.Range["Q8:S8"].Value = date;

				worksheet.Range["D12:E12"].Value = Standards;
				worksheet.Range["D15:E15"].Value = ExtensionPieces;
				worksheet.Range["D18:E18"].Value = RailLengths;
				worksheet.Range["F21:G21"].Value = BottomBrickYard460x815;
				worksheet.Range["I21"].Value = BottomBrickYard920x815;
				worksheet.Range["M21"].Value = BottomBrickYard1780x815;
				worksheet.Range["F24:G24"].Value = TopBrickYard460x815;
				worksheet.Range["I24"].Value = TopBrickYard920x815;
				worksheet.Range["M24"].Value = TopBrickYard1780x815;
				worksheet.Range["D27:E27"].Value = ToeBoardLengths;
				worksheet.Range["D30:E30"].Value = SleeveJoints;
				worksheet.Range["D33:E33"].Value = ElbowJoints;
				worksheet.Range["D36:E36"].Value = AngleSwivelBends;
				worksheet.Range["D39:E39"].Value = EdgeTrimCradleFrames;
				worksheet.Range["N14:Q14"].Value = LinMeterRun1mSystem;
				worksheet.Range["N15:Q15"].Value = LinMeterRun1_8mSystem;
				worksheet.Range["N16:Q16"].Value = LinMeterRun1mPhaseBreak;
				worksheet.Range["N17:Q17"].Value = LinMeterRun1_8mPhaseBreak;


				string textBoxName = DetermineDivisionTextBoxName(divisionNo);

				Shape textBoxShape = worksheet.Shapes.Cast<Shape>()
					.FirstOrDefault(shape => shape.Name == textBoxName);

				if (textBoxShape != null)
				{
					textBoxShape.TextFrame.Characters(0, 1).Text = "X";
				}


				workbook.Save();
			}
			finally
			{
				workbook.Close(false);
				excelApp.Quit();

				System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
				System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
			}
		}

		private static void RoundHandrailMeters()
		{
			LinMeterRun1mSystem = RoundToNearestHalf(LinMeterRun1mSystem);
			LinMeterRun1_8mSystem = RoundToNearestHalf(LinMeterRun1_8mSystem);
			LinMeterRun1mPhaseBreak = RoundToNearestHalf(LinMeterRun1mPhaseBreak);
			LinMeterRun1_8mPhaseBreak = RoundToNearestHalf(LinMeterRun1_8mPhaseBreak);
		}

		private static string DetermineDivisionTextBoxName(int divisionNo)
		{
			if (divisionNo == 1) //Commercial and industrial
			{
				return "TextBox 21";
			}
			if (divisionNo == 2) //Nuclear and infrastructure
			{
				return "TextBox 22";
			}
			if (divisionNo == 3) //Parts and processing
			{
				return "TextBox 23";
			}
			if (divisionNo == 4) //Other
			{
				return "TextBox 14";
			}
			return "void";
		}

		private static void GetFullKickFlatsRequired()
		{

			double exactKickFlatRequired = KickFlatLength / 3.700;
			int extraRails = exactKickFlatRequired > 5 ? 0 : 2; //Add 2 extra rails to have some spare (only when more than 5 is needed overall)
			ToeBoardLengths = (int)Math.Ceiling(exactKickFlatRequired) + extraRails;
		}

		private static void GetFullRailsRequired()
		{
			int extraRails = 2;
			double exactRailsRequired = RunningHandrailLength / 3.600;
			int fullRailsRequired = (int)Math.Ceiling(exactRailsRequired);
			RailLengths = fullRailsRequired + extraRails;
		}

		public static double RoundToNearestHalf(double value)
		{
			return Math.Round(value * 2) / 2.0;
		}

		public static void CollateOrderableParts(Model model, List<PrismPart> selectedObjects, PrismProjectData projData)
		{
			foreach (PrismPart p in selectedObjects)
			{
				if (p.Part is Brep post)
				{
					switch (post.Name)
					{
						case string name when name.Contains("STANDARD"):
							Standards++;
							break;
						case string name when name.Contains("EXTENSION"):
							ExtensionPieces++;
							break;
						case string name when name.Contains("SS-TRIMFRAME"):
							EdgeTrimCradleFrames++;
							break;
					}
				}
				//if (p.Part is Beam b)
				{
					switch (p.Part.Name)
					{
						case string name when name.Contains("HANDRAIL"):
							AddToHandrail(model, p.Part);
							break;
						case string name when name.Contains("PANEL"):
							AddToPanels(p.Part);
							break;
						case string name when name.Contains("KICKFLAT"):
							CountKickFlat(p.Part);
							break;
						case string name when name.Contains("SS-KK-TYPE-14-6"):
							AddExternalKlamp();
							break;
						case string name when name.Contains("SWIVEL"):
							AddSwivelKlamp();
							break;
						case string name when name.Contains("SS-KK-TYPE-15-6"):
							AddElbowJoint();
							break;
						case string name when name.Contains("SS-KK-TYPE-18-6"):
							AddInternalKlamp(p.Part);
							break;
						default:
							break;
					}
					ModelModifiers.ModifyAttribute(p, 3, projData, null, false, true);
				}
			}
			RoundHandrailMeters();
		}

		private static void AddElbowJoint()
		{
			ElbowJoints++;
		}

		private static void AddInternalKlamp(Part p)
		{
			if (!p.Profile.ProfileString.Contains("CHS"))
			{
				SleeveJoints++;
			}
		}

		private static void AddExternalKlamp()
		{
			SleeveJoints++;
		}

		private static void AddSwivelKlamp()
		{
			AngleSwivelBends++;
		}

		private static void CountKickFlat(Part p)
		{
			KickFlatLength += ModelModifiers.GetPartLength(p) / 1000;
		}

		private static void AddToHandrail(Model model, Part p)
		{
			RunningHandrailLength += ModelModifiers.GetPartLength(p) / 1000;
			if (p.Class == "10") { GetSystemLength(p, model, true); }
			else { GetSystemLength(p, model, false); }
		}

		private static void GetSystemLength(Part myPart, Model model, bool isPhaseBreak)
		{
			Beam b = myPart as Beam;
			if (b == null)
			{
				return;
			}
			var boundingBox = CreateBoundingBox(b, 400);
			var parts = model.GetModelObjectSelector().GetObjectsByBoundingBox(boundingBox.MaxPoint, boundingBox.MinPoint);

			Brep standard = GetPostParts(parts, out bool extensionFound);

			int standardHeight = 1230;
			int tolerance = 150;

			if (standard != null && Math.Abs(standard.EndPoint.Z + standardHeight - b.EndPoint.Z) < tolerance) //then the rail has a post and is suitably close to the top of it for it to be the upper rail
			{
				UpdateLengthValues(myPart, isPhaseBreak, extensionFound);
			}
		}

		private static AABB CreateBoundingBox(Beam b, double offset)
		{
			Point startPoint = b.StartPoint;
			Point endPoint = b.EndPoint;
			AABB boundingBox = new AABB(
			new Point(Math.Min(startPoint.X, endPoint.X) - 100, Math.Min(startPoint.Y, endPoint.Y) - 100, Math.Min(startPoint.Z, endPoint.Z) - 3000),
			new Point(Math.Max(startPoint.X, endPoint.X) + 100, Math.Max(startPoint.Y, endPoint.Y) + 100, Math.Max(startPoint.Z, endPoint.Z) + 100));
			return boundingBox;
		}

		private static Brep GetPostParts(ModelObjectEnumerator modelObjects, out bool extensionFound)
		{
			Brep standard = null;
			extensionFound = false;

			foreach (var mObj in modelObjects)
			{
				if (mObj is Brep beam)
				{
					if (beam.Name.Contains("SS-STANDARD"))
					{
						standard = beam;
					}
					if (beam.Name.Contains("SS-EXTENSION"))
					{
						extensionFound = true;
					}
				}
			}

			return standard;
		}

		private static void UpdateLengthValues(Part myPart, bool isPhaseBreak, bool extensionFound)
		{
			double length = ModelModifiers.GetPartLength(myPart) / 1000; //length is in mm, we need it in m hence the /1000
			if (extensionFound) // then our rail is part of a 3 rail system
			{
				if (isPhaseBreak) { LinMeterRun1_8mPhaseBreak += length; }
				else { LinMeterRun1_8mSystem += length; }
			}
			else // its part of a 2 rail
			{
				if (isPhaseBreak) { LinMeterRun1mPhaseBreak += length; }
				else { LinMeterRun1mSystem += length; }
			}
		}

		private static void AddToPanels(Part p)
		{
			if (p is Beam b)
			{
				double panelLength = Distances.Point2Point(Convertor.PointToPoint3D(b.StartPoint), Convertor.PointToPoint3D(b.EndPoint));

				if (b.Name.Contains("BTM"))
				{
					BottomBrickYard460x815 += panelLength < 600 ? 1 : 0;
					BottomBrickYard920x815 += panelLength > 600 && panelLength < 1300 ? 1 : 0;
					BottomBrickYard1780x815 += panelLength > 1300 ? 1 : 0;
				}

				if (b.Name.Contains("TOP"))
				{
					TopBrickYard460x815 += panelLength < 600 ? 1 : 0;
					TopBrickYard920x815 += panelLength > 600 && panelLength < 1300 ? 1 : 0;
					TopBrickYard1780x815 += panelLength > 1300 ? 1 : 0;
				}
			}
		}

		private static void ResetAllNumbers()
		{
			Standards = 0;
			ExtensionPieces = 0;
			RailLengths = 0;
			BottomBrickYard460x815 = 0;
			BottomBrickYard920x815 = 0;
			BottomBrickYard1780x815 = 0;
			TopBrickYard460x815 = 0;
			TopBrickYard920x815 = 0;
			TopBrickYard1780x815 = 0;
			ToeBoardLengths = 0;
			SleeveJoints = 0;
			ElbowJoints = 0;
			AngleSwivelBends = 0;
			EdgeTrimCradleFrames = 0;
			LinMeterRun1mSystem = 0;
			LinMeterRun1_8mSystem = 0;
			LinMeterRun1mPhaseBreak = 0;
			LinMeterRun1_8mPhaseBreak = 0;
			RunningHandrailLength = 0;
			KickFlatLength = 0;
		}
	}
}