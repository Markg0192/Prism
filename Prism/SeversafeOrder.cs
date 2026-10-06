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
			try
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

				bool zipFileCanBeAttached = reportManager.Folders.ZipFolder(reportManager.Folders.EpoPath);

				EmailWriter.WriteEpoEmail(reportManager.ProjectData, reportPrefix, reportManager.IssueNum, reportManager.PhaseNum, siteDate, reportManager.Folders.EpoPath, zipFileCanBeAttached);
			}
			catch (Exception ex)
			{
				
			}
		}

		public static void CopyRenameAndWrite(PrismProjectData projData, string phaseNumber, string deliveryDate, string sourcePath, string destinationPath, string newFileName, int divisionNo)
		{
			try
			{
				if (File.Exists(sourcePath))
				{
					Directory.CreateDirectory(destinationPath);

					string destinationFilePath = Path.Combine(destinationPath, newFileName);

					File.Copy(sourcePath, destinationFilePath);

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
			Application excelApp = null;
			Workbook workbook = null;

			try
			{
				excelApp = new Application();
				workbook = excelApp.Workbooks.Open(filePath);

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
				if (workbook != null)
				{
					workbook.Close(false);
				}

				if (excelApp != null)
				{
					excelApp.Quit();
				}

				if (workbook != null)
				{
					System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
				}

				if (excelApp != null)
				{
					System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
				}
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
			if (divisionNo == 1)
			{
				return "TextBox 21";
			}
			if (divisionNo == 2)
			{
				return "TextBox 22";
			}
			if (divisionNo == 3)
			{
				return "TextBox 23";
			}
			if (divisionNo == 4)
			{
				return "TextBox 14";
			}
			return "void";
		}

		private static void GetFullKickFlatsRequired()
		{
			double exactKickFlatRequired = KickFlatLength / 3.700;
			int extraRails = exactKickFlatRequired > 5 ? 0 : 2;
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
			List<SeversafePostCacheItem> postCache = BuildSeversafePostCache(selectedObjects);

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

				switch (p.Part.Name)
				{
					case string name when name.Contains("HANDRAIL"):
						AddToHandrail(p.Part, postCache);
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

				ModelModifiers.ModifyAttribute(p, Enums.StageTypes.Prelim3, projData, null, null, false, true);
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

		private static void AddToHandrail(Part p, List<SeversafePostCacheItem> postCache)
		{
			RunningHandrailLength += ModelModifiers.GetPartLength(p) / 1000;

			if (p.Class == "10")
			{
				GetSystemLength(p, postCache, true);
			}
			else
			{
				GetSystemLength(p, postCache, false);
			}
		}

		private static void GetSystemLength(Part myPart, List<SeversafePostCacheItem> postCache, bool isPhaseBreak)
		{
			Beam b = myPart as Beam;
			if (b == null)
			{
				return;
			}

			int standardHeight = 1230;
			int tolerance = 150;

			bool extensionFound = false;
			Brep standard = null;

			double minX = Math.Min(b.StartPoint.X, b.EndPoint.X) - 100;
			double maxX = Math.Max(b.StartPoint.X, b.EndPoint.X) + 100;
			double minY = Math.Min(b.StartPoint.Y, b.EndPoint.Y) - 100;
			double maxY = Math.Max(b.StartPoint.Y, b.EndPoint.Y) + 100;
			double minZ = Math.Min(b.StartPoint.Z, b.EndPoint.Z) - 3000;
			double maxZ = Math.Max(b.StartPoint.Z, b.EndPoint.Z) + 100;

			foreach (SeversafePostCacheItem post in postCache)
			{
				bool insideBoundingBox =
					post.EndPoint.X >= minX &&
					post.EndPoint.X <= maxX &&
					post.EndPoint.Y >= minY &&
					post.EndPoint.Y <= maxY &&
					post.EndPoint.Z >= minZ &&
					post.EndPoint.Z <= maxZ;

				if (!insideBoundingBox)
				{
					continue;
				}

				if (post.IsStandard)
				{
					standard = post.Brep;
				}

				if (post.IsExtension)
				{
					extensionFound = true;
				}
			}

			if (standard != null && Math.Abs(standard.EndPoint.Z + standardHeight - b.EndPoint.Z) < tolerance)
			{
				UpdateLengthValues(myPart, isPhaseBreak, extensionFound);
			}
		}

		private static void UpdateLengthValues(Part myPart, bool isPhaseBreak, bool extensionFound)
		{
			double length = ModelModifiers.GetPartLength(myPart) / 1000;

			if (extensionFound)
			{
				if (isPhaseBreak) { LinMeterRun1_8mPhaseBreak += length; }
				else { LinMeterRun1_8mSystem += length; }
			}
			else
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

		private class SeversafePostCacheItem
		{
			public Brep Brep { get; set; }
			public string Name { get; set; }
			public Point EndPoint { get; set; }
			public bool IsStandard { get; set; }
			public bool IsExtension { get; set; }
		}

		private static List<SeversafePostCacheItem> BuildSeversafePostCache(List<PrismPart> selectedObjects)
		{
			var postCache = new List<SeversafePostCacheItem>();

			foreach (PrismPart prismPart in selectedObjects)
			{
				if (prismPart.Part is Brep brep)
				{
					bool isStandard = brep.Name.Contains("SS-STANDARD");
					bool isExtension = brep.Name.Contains("SS-EXTENSION");

					if (isStandard || isExtension)
					{
						postCache.Add(new SeversafePostCacheItem
						{
							Brep = brep,
							Name = brep.Name,
							EndPoint = brep.EndPoint,
							IsStandard = isStandard,
							IsExtension = isExtension
						});
					}
				}
			}

			return postCache;
		}
	}
}