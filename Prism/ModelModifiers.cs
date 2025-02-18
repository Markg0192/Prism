using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Drawing.Automation;
using System.IO;
using System.Collections.Generic;
using Tekla.Structures;
using System.Linq;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Tekla.Structures.Model.UI;
using static Prism.Enums;
using Part = Tekla.Structures.Model.Part;
using ModelObject = Tekla.Structures.Model.ModelObject;
using System;
using System.Threading;
using System.Windows.Forms;
using Prism.CustomDialogs;
using System.Text.RegularExpressions;
using System.Data;
using static Tekla.Structures.Filtering.Categories.TaskFilterExpressions;

namespace Prism
{
	/// <summary>
	/// The Model modifiers class is where all changes to the model take place.
	/// This is usually adding stamps to member and drawing user fields.
	/// </summary>
	public static class ModelModifiers
	{
		/*public static void VariationCheck(string phaseNumber, PrismProjectData projData, string variationNumber)
		{
			bool isVariation = phaseNumber.Contains("V") || phaseNumber.Contains("v") || variationNumber != "";

			if (isVariation)
			{
				if (PrismWarnings.IsVariation())
				{
					if (phaseNumber.Contains("V") || phaseNumber.Contains("v"))
					{
						projData.VariationNumber = phaseNumber;
					}
					else
					{
						projData.VariationNumber = variationNumber;
					}
					projData.IsVariation = true;

				}
			}
			else { projData.IsVariation = false; }
		}*/

		public static void VariationCheck(PrismProjectData projData, string variationNumber, string variationType)
		{
			// Check if there is any indication of a variation in phaseNumber or variationNumber
			bool isVariation = !string.IsNullOrEmpty(variationNumber);

			// If it is a variation and the user accepts it as a variation via the Prism warning
			if (isVariation && PrismWarnings.IsVariation())
			{
				// Remove any '.' characters from variationType
				variationType = variationType.Replace(".", string.Empty);
				projData.VariationNumber = $"{variationType}{variationNumber}";
				projData.IsVariation = true;
			}
			else
			{
				projData.IsVariation = false;
			}
		}

		public static void ResetWorkPlane(Model model)
		{
			model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());
			Point Origin = new Point(0, 0, 0);
			Vector x = new Vector(1, 0, 0);
			Vector y = new Vector(0, 1, 0);
			TransformationPlane XZ_Plane = new TransformationPlane(Origin, x, y);
			model.GetWorkPlaneHandler().SetCurrentTransformationPlane(XZ_Plane);
			model.CommitChanges();
		}

		private static void SetVariationAttribute(string variationNumber, Part p)
		{
			string firstVNo = "";
			string secondVNo = "";

			// Read current properties once
			p.GetUserProperty(ModelUDA.FirstVariationNumber(), ref firstVNo);
			p.GetUserProperty(ModelUDA.SecondVariationNumber(), ref secondVNo);

			// 1) If first is empty, fill it and exit
			if (string.IsNullOrEmpty(firstVNo))
			{
				p.SetUserProperty(ModelUDA.FirstVariationNumber(), variationNumber);
				return;
			}

			// 2) If first is NOT empty, check second:
			//    If second is empty AND first != new variation => fill second
			if (string.IsNullOrEmpty(secondVNo))
			{
				// If first equals new, do nothing, so just check if they're different
				if (ParseString(firstVNo) != ParseString(variationNumber))
				{
					p.SetUserProperty(ModelUDA.SecondVariationNumber(), variationNumber);
				}
				return;
			}

			// 3) Both slots are filled.
			//    If first == new or second == new => do nothing (avoid duplication).
			if (ParseString(firstVNo) == ParseString(variationNumber) || ParseString(secondVNo) == ParseString(variationNumber))
			{
				return;
			}

			// 4) Otherwise, both are filled, neither match new => replace first
			p.SetUserProperty(ModelUDA.FirstVariationNumber(), variationNumber);
		}

		public static string ParseString(string input)
		{
			// 1. Remove any 'V', 'O', '.', '-' (case-insensitive)
			string cleaned = Regex.Replace(input, "[VO\\.-]", "", RegexOptions.IgnoreCase);

			// 2. Remove leading zeros
			cleaned = cleaned.TrimStart('0');

			return cleaned;
		}

		public static bool ModifyAttributes(this List<PrismPart> selectedObjects, int stageNumber, PrismProjectData projectData,
	 ToolStrip toolStrip, ToolStripStatusLabel statusLabel, bool isSpecialFittingOrder = false, bool isSeversafe = false)
		{
			int currentCount = 0;
			int totalCount = selectedObjects.Count;

			TableData td = stageNumber == 3 ? UniClass_Codes.ReadTableData(Constants.ModelProjectInforLocation(projectData.ProjNumberAndGuid)) : null;

			foreach (PrismPart part in selectedObjects)
			{
				if (totalCount > 0) UpdateStatusLabel(ref currentCount, toolStrip, statusLabel, totalCount, "Updating Prism Attributes");

				if (!ModifyAttribute(part, stageNumber, projectData, td, isSpecialFittingOrder, isSeversafe))
				{
					return false;
				}
			}
			return true;
		}

		public static bool ModifyAttribute(PrismPart prismPart, int stageNumber, PrismProjectData projectData, TableData uniClassTable, bool isSpecialFittingOrder = false, bool isSeversafe = false)
		{
			if (isSpecialFittingOrder) prismPart.Part.SetUserProperty(ModelUDA.Pre_Ordered(), 1);

			prismPart.Part.SetUserProperty(ModelUDA.CurrentStageName(stageNumber), projectData.Full);
			prismPart.Part.SetUserProperty(ModelUDA.CurrentStageDate(stageNumber), projectData.Date);

			if (stageNumber == 3 && !isSeversafe)
			{
				TableRow row = UniClassCodes.GetUniClassDetailForPart(projectData.ProjNumberAndGuid, prismPart.Part, uniClassTable);
				if (row != null)
				{
					prismPart.Part.SetUserProperty("SEV-UDA-130", row.Code);
					prismPart.Part.SetUserProperty("SEV-UDA-131", row.Title);
				}
			}

			if (IsDetail3(stageNumber))
			{
				CheckForAndFixNegativeDftWfts(prismPart);
			}

			if (IsFabStage(stageNumber))
			{
				prismPart.Part.SetUserProperty(ModelUDA.PartMarkAtFab(), prismPart.PartMark);
			}

			if (IsFabStage(stageNumber) && prismPart.NumbersOutOfDate)
			{
				return PrismWarnings.NumbersNoLongerUpToDate();
			}

			return true;
		}

		private static bool IsFabStage(int stageNumber)
		{
			return stageNumber == 7;
		}

		private static bool IsDetail3(int stageNumber)
		{
			return stageNumber == 6;
		}

		private static void CheckForAndFixNegativeDftWfts(PrismPart pPart)
		{
			if (pPart.SherwinDft.Contains("-"))
			{
				pPart.Part.SetUserProperty(ModelUDA.FireDFT(), 0);
			}
			if (pPart.SherwinWft.Contains("-"))
			{
				pPart.Part.SetUserProperty(ModelUDA.FireWFT(), 0);
			}
			if (pPart.HempelDft.Contains("-"))
			{
				pPart.Part.SetUserProperty(ModelUDA.HempelFireDFT(), 0);
			}
			if (pPart.HempelWft.Contains("-"))
			{
				pPart.Part.SetUserProperty(ModelUDA.HempelFireWFT(), 0);
			}
		}

		public static void StampBoltUDA(List<BoltGroup> allBolts, string name, string date, string phaseNumber, string issueNumber)
		{
			//run macro
			foreach (BoltGroup bolts in allBolts)
			{
				if (bolts != null)
				{
					bolts.SetUserProperty(ModelUDA.BoltOrderedBy(), name);
					bolts.SetUserProperty(ModelUDA.BoltOrderedDate(), date);
					bolts.SetUserProperty(ModelUDA.BoltOrderPhaseNo(), phaseNumber);
					bolts.SetUserProperty(ModelUDA.BoltOrderIssueNo(), issueNumber);
				}
			}
		}

		private static string TimesBoltOrdered(BoltGroup bolts)
		{
			string timesOrdered = "";
			bolts.GetUserProperty(ModelUDA.BoltOrderPhaseNo(), ref timesOrdered);

			if (timesOrdered != "" && timesOrdered.Contains("Times ordered"))
			{
				var nu = timesOrdered.Split('=');
				int newOrderCount = Convert.ToInt32(nu[1]) + 1;
				return $"Times ordered ={newOrderCount}";
			}

			return "Times ordered =1";
		}

		public static string BoltPhaseAndIssue(string phaseNumber, string issueNumber)
		{
			return $"Ordered Phase-{phaseNumber} Issue{issueNumber}";
		}

		public static void StampPartFabUDA(List<PrismPart> selectedModelParts, string phaseNumber, string issueNumber)
		{
			foreach (PrismPart part in selectedModelParts)
			{
				part.Part.SetUserProperty(ModelUDA.FabStampUDA(), ModelUDA.FabStamp(phaseNumber, issueNumber));
				part.Part.Modify();
			}
		}

		public static void AddStartNumbers(this List<PrismPart> parts, string startNumber)
		{
			foreach (PrismPart pPart in parts)
			{
				pPart.Part.PartNumber.StartNumber = Convert.ToInt32(startNumber);
				pPart.Part.AssemblyNumber.StartNumber = Convert.ToInt32(startNumber);
				pPart.Part.Modify();
			}
		}

		/// <summary>
		/// Adds Prelim marks to the parts within the selected objects.
		/// The method retrieves the current last used Prelim number from the model data files and an optional Prelim prefix from advanced settings.
		/// It then iterates through each part, assigning a new Prelim mark (consisting of the prefix and a sequential number) to parts that lack one.
		/// If the current phase/order is a variation, a variation attribute is also applied.
		/// Finally, the method updates the stored last used Prelim number with the new value.
		/// </summary>
		public static bool AddPrelimMarks(this SelectedObjects selectedObjects, PrismProjectData pData, ToolStrip ts, ToolStripStatusLabel tssl)
		{
			// Retrieve the last used prelim value from the project info.
			if (Logging.GetLastUsedPrelim(pData.ProjNumberAndGuid) is int currentLastNumber)
			{
				// Get the prelim prefix advanced setting.
				string prelimPrefix = Logging.GetAdvancedSetting(pData.ProjNumberAndGuid, AdvancedSettingType.PrelimPrefix);

				// If no valid number was retrieved, default to 1.
				if (currentLastNumber == 0)
				{
					currentLastNumber = 1;
				}

				List<PrismPart> partsToRecievePrelim = selectedObjects.PrismParts.Where(x => string.IsNullOrWhiteSpace(x.Prelim)).ToList();

				// Save the updated last used prelim value back to storage.
				if (!Logging.SetLastUsedPrelim(pData.ProjNumberAndGuid, currentLastNumber + partsToRecievePrelim.Count)) return false;

				int total = partsToRecievePrelim.Count;
				int count = 1;
				// Iterate over all parts that need a prelim mark.
				foreach (PrismPart p in partsToRecievePrelim)
				{
					UpdateStatusLabel(ref count, ts, tssl, total, "Applying Prelim Marks");
					// Compose the prelim mark by joining the prefix and the current number.
					string prelimMark = prelimPrefix + currentLastNumber.ToString();

					// Set the user property for the prelim mark.
					p.Part.SetUserProperty(ModelUDA.PrelimMark(), prelimMark);

					// If this project variation requires additional attributes, set them.
					if (pData.IsVariation)
					{
						SetVariationAttribute(pData.VariationNumber, p.Part);
					}

					currentLastNumber++;
				}

				SelectedObjects refreshedSelectedPartsForChecking = new SelectedObjects(pData.ProjPath, StageTypes.Prelim3, "", "", selectedObjects.Model, false);

				int numberBeforeDuplicateChecking = currentLastNumber;
				CheckForAndFixDuplicatePrelims(refreshedSelectedPartsForChecking, prelimPrefix, ref currentLastNumber, ts, tssl);

				//if currentLastNumber has changed since going into CheckForAndFixDuplicatePrelims, we need to write this to the model data
				if (numberBeforeDuplicateChecking != currentLastNumber)
				{
					if (!Logging.SetLastUsedPrelim(pData.ProjNumberAndGuid, currentLastNumber)) return false;
				}
				return true;
			}

			PrismWarnings.GetLastUsedPrelimFailed();
			return false;
		}

		private static void UpdateStatusLabel(ref int processedCount, ToolStrip toolStrip, ToolStripStatusLabel statusLabel, int totalCount, string countType)
		{
			// Safely increment the processed count.
			int currentCount = Interlocked.Increment(ref processedCount);
			double progressPercentage = (double)currentCount / totalCount * 100;

			// Calculate the ideal update interval as 5% of the total.
			int idealInterval = (int)Math.Ceiling(totalCount * 0.05);
			// Clamp the update interval between 5 and 250.
			int updateInterval = Math.Max(5, Math.Min(idealInterval, 150));

			// Throttle UI updates to maintain responsiveness:
			if (currentCount % updateInterval == 0 || currentCount == totalCount)
			{
				toolStrip.Invoke(new Action(() =>
				{
					statusLabel.Text = $"{countType} {currentCount} of {totalCount}({progressPercentage:N1}%";
				}));
			}
		}

		/// <summary>
		/// Checks the Prelims of the fresly got prism parts (which now should all have prelims), and checks for duplicating marks.
		/// If a duplicate is found, assigns a new unique Prelim number (using the given prefix and starting from currentLastNumber).
		/// The currentLastNumber is incremented as new numbers are assigned.
		/// </summary>
		/// <param name="selectedObjects">The freshly loaded set of parts to validate.</param>
		/// <param name="prelimPrefix">The prefix to use when composing a new Prelim mark.</param>
		/// <param name="currentLastNumber">The current last used number; passed by reference so that it is updated as new numbers are assigned.</param>
		public static void CheckForAndFixDuplicatePrelims(SelectedObjects selectedObjects, string prelimPrefix, ref int currentLastNumber, ToolStrip ts, ToolStripStatusLabel tssl)
		{
			// Use a HashSet to track the Prelim values already encountered.
			HashSet<string> seenPrelims = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			List<PrismPart> partsWithAPrelim = selectedObjects.PrismParts.Where(x => !string.IsNullOrWhiteSpace(x.Prelim)).ToList();

			int total = partsWithAPrelim.Count;
			int count = 1;
			// Loop through all parts.
			foreach (PrismPart part in partsWithAPrelim)
			{
				UpdateStatusLabel(ref count, ts, tssl, total, "Validating Prelim Mark");

				// If this Prelim has already been encountered, it's a duplicate.
				if (seenPrelims.Contains(part.Prelim))
				{
					// Compose a new unique Prelim mark.
					string newPrelim = prelimPrefix + currentLastNumber.ToString();

					// Update the part's property with the new value.
					part.Part.SetUserProperty(ModelUDA.PrelimMark(), newPrelim);

					// Add the new Prelim to the set.
					seenPrelims.Add(newPrelim);

					// Increment the current last number for the next assignment.
					currentLastNumber++;
				}
				else
				{
					// If not a duplicate, add the value to the seen set.
					seenPrelims.Add(part.Prelim);
				}
			}
		}

		public static List<PrismPart> SelectSpecialTaggedInSelection(SelectedObjects selectedObjects)
		{
			List<PrismPart> specialTaggedParts = new List<PrismPart>();
			foreach (PrismPart part in selectedObjects.PrismParts)
			{
				string specialTag = "";  //this is the number read from teklas UDA when no execution class is applied, we are defaulting to it not having one here
				part.Part.GetUserProperty(ModelUDA.SpecialFittingTag(), ref specialTag);

				if (specialTag != "")
				{
					specialTaggedParts.Add(part);
				}
			}
			specialTaggedParts.SelectParts();
			return specialTaggedParts;
		}

		public static void ClearPrelimMarking(ProjectInfo pInfo, string resetNumber)
		{
			pInfo.SetUserProperty(ModelUDA.LastUsedPrelim(), Convert.ToInt32(resetNumber));
		}

		public static double GetPartLength(Part myPart)
		{
			double length = 0.0;
			myPart.GetReportProperty(ModelUDA.Length(), ref length);
			return length;
			/* ArrayList points = myPart.GetCenterLine(true);
             Point start = points[0] as Point;
             Point end = points[1] as Point;
             double Length = Distance.PointToPoint(end, start);
             return Length;*/
		}

		public static double GetPartWidth(Part myPart)
		{
			double width = 0.0;
			myPart.GetReportProperty("FLANGE_LENGTH_B", ref width);
			return width;
		}

		public static void LockPart(this Part part)
		{
			part.SetUserProperty(ModelUDA.ObjectLock(), 1);
		}

		public static List<Part> MoveAndRenameOmittedMembers(List<ModelObject> partsToBeMoved, double distanceToMoveInZ, bool keepOriginal)
		{
			List<Part> movedParts = new List<Part>();

			foreach (Part p in partsToBeMoved)
			{
				if (keepOriginal)
				{
					Vector newVector = new Vector(0, 0, distanceToMoveInZ);
					Part copiedMember = Operation.CopyObject(p, newVector) as Part;

					copiedMember.Name = "OMIT";
					copiedMember.AssemblyNumber.Prefix = "OMIT";
					copiedMember.PartNumber.Prefix = "OMIT";
					copiedMember.Class = "6";
					copiedMember.GetPhase(out Phase currentPhase);
					Phase myPhase = new Phase((Convert.ToInt32(currentPhase.PhaseNumber) + 1000000), $"Phase {currentPhase.PhaseNumber} OMIT", "", 0);
					myPhase.Insert();
					copiedMember.SetPhase(myPhase);
					copiedMember.Modify();

					for (int i = 0; i < 10; i++)
					{
						copiedMember.SetUserProperty(ModelUDA.CurrentStageName(i), p.StageString(ModelUDA.CurrentStageName(i))); //Set prism values and prelim on the new copied fabsec
						copiedMember.SetUserProperty(ModelUDA.CurrentStageDate(i), p.StageString(ModelUDA.CurrentStageDate(i))); //All these values are unique in the model settings
						p.SetUserProperty(ModelUDA.CurrentStageName(i), "");
						p.SetUserProperty(ModelUDA.CurrentStageDate(i), "");
					}

					copiedMember.SetUserProperty(ModelUDA.FabsecUniqueNumber(), p.StageString(ModelUDA.FabsecUniqueNumber()));
					copiedMember.SetUserProperty(ModelUDA.PrelimMark(), p.GetPrelimMark());
					copiedMember.SetUserProperty(ModelUDA.PartMarkAtFab(), p.StageString(ModelUDA.PartMarkAtFab()));
					copiedMember.SetUserProperty(ModelUDA.FabStampUDA(), p.StageString(ModelUDA.FabStampUDA()));

					p.SetUserProperty(ModelUDA.FabsecUniqueNumber(), "");
					p.SetUserProperty(ModelUDA.PrelimMark(), "");
					p.SetUserProperty(ModelUDA.PartMarkAtFab(), "");
					p.SetUserProperty(ModelUDA.FabStampUDA(), "");

					p.Modify();

					movedParts.Add(copiedMember);
				}
				else
				{
					p.Name = "OMIT";
					p.AssemblyNumber.Prefix = "OMIT";
					p.PartNumber.Prefix = "OMIT";
					p.Class = "6";
					p.GetPhase(out Phase currentPhase);
					Phase myPhase = new Phase((Convert.ToInt32(currentPhase.PhaseNumber) + 1000000), $"Phase {currentPhase.PhaseNumber} OMIT", "", 0);
					myPhase.Insert();
					p.SetPhase(myPhase);
					p.Modify();
					Vector myVector = new Vector(0, 0, distanceToMoveInZ);
					Operation.MoveObject(p, myVector);
					p.Select();


				}

			}
			return movedParts;
		}

		public static void CopyAndOmitPart(double distanceToMoveInZ, Part p, List<PrismPart> movedParts)
		{
			Vector newVector = new Vector(0, 0, distanceToMoveInZ);
			Part copiedMember = Operation.CopyObject(p, newVector) as Part;

			copiedMember.Name = "OMIT";
			copiedMember.AssemblyNumber.Prefix = "OMIT";
			copiedMember.PartNumber.Prefix = "OMIT";
			copiedMember.Class = "6";
			copiedMember.GetPhase(out Phase currentPhase);
			Phase myPhase = new Phase((Convert.ToInt32(currentPhase.PhaseNumber) + 1000000), $"Phase {currentPhase.PhaseNumber} OMIT", "", 0);
			myPhase.Insert();
			copiedMember.SetPhase(myPhase);
			copiedMember.Modify();

			for (int i = 0; i < 10; i++)
			{
				copiedMember.SetUserProperty(ModelUDA.CurrentStageName(i), p.StageString(ModelUDA.CurrentStageName(i))); //Set prism values and prelim on the new copied fabsec
				copiedMember.SetUserProperty(ModelUDA.CurrentStageDate(i), p.StageString(ModelUDA.CurrentStageDate(i))); //All these values are unique in the model settings
				p.SetUserProperty(ModelUDA.CurrentStageName(i), "");
				p.SetUserProperty(ModelUDA.CurrentStageDate(i), "");
			}

			copiedMember.SetUserProperty(ModelUDA.FabsecUniqueNumber(), p.StageString(ModelUDA.FabsecUniqueNumber()));
			copiedMember.SetUserProperty(ModelUDA.PrelimMark(), p.GetPrelimMark());
			copiedMember.SetUserProperty(ModelUDA.PartMarkAtFab(), p.StageString(ModelUDA.PartMarkAtFab()));
			copiedMember.SetUserProperty(ModelUDA.FabStampUDA(), p.StageString(ModelUDA.FabStampUDA()));

			p.SetUserProperty(ModelUDA.FabsecUniqueNumber(), "");
			p.SetUserProperty(ModelUDA.PrelimMark(), "");
			p.SetUserProperty(ModelUDA.PartMarkAtFab(), "");
			p.SetUserProperty(ModelUDA.FabStampUDA(), "");

			p.Modify();

			movedParts.Add(new PrismPart(copiedMember));
		}

		private static void MoveAndOmitPart(Part p, double distanceToMoveInZ, List<PrismPart> movedParts)
		{
			p.Name = "OMIT";
			p.AssemblyNumber.Prefix = "OMIT";
			p.PartNumber.Prefix = "OMIT";
			p.Class = "6";
			p.GetPhase(out Phase currentPhase);
			Phase myPhase = new Phase((Convert.ToInt32(currentPhase.PhaseNumber) + 1000000), $"Phase {currentPhase.PhaseNumber} OMIT", "", 0);
			myPhase.Insert();
			p.SetPhase(myPhase);
			p.Modify();
			Vector myVector = new Vector(0, 0, distanceToMoveInZ);
			Operation.MoveObject(p, myVector);
			p.Select();
			movedParts.Add(new PrismPart(p));
		}

		public static List<PrismPart> MoveAndRenameOmittedMembers2(List<PrismPart> partsToBeMoved, double distanceToMoveInZ, bool keepOriginal, Model model)
		{
			List<PrismPart> movedParts = new List<PrismPart>();
			string isCarcassCreated = "";

			foreach (PrismPart p in partsToBeMoved)
			{
				if (p.IsFabsec)
				{
					p.Part.GetUserProperty(ModelUDA.FabsecCarcassOrdered(), ref isCarcassCreated);
				}

				HandleMember(p, distanceToMoveInZ, keepOriginal, model, movedParts, isCarcassCreated != "");
			}

			return movedParts;
		}

		private static void HandleMember(PrismPart fabsec, double distanceToMoveInZ, bool keepOriginal, Model model, List<PrismPart> movedParts, bool isFabsecWithCarcassCreated)
		{
			if (isFabsecWithCarcassCreated)
			{
				PrismPart carcass = FabsecProcessing.GetCarcassFromSelected(model, fabsec);

				MoveAndOmitPart(carcass.Part, distanceToMoveInZ * 2, movedParts);
				if (keepOriginal)
				{
					ClearFabsecAttributes(fabsec.Part);
				}
				else { fabsec.Part.Delete(); }
			}
			else
			{
				if (keepOriginal)
				{
					CopyAndOmitPart(distanceToMoveInZ, fabsec.Part, movedParts);
					ClearFabsecAttributes(fabsec.Part);
				}
				else { MoveAndOmitPart(fabsec.Part, distanceToMoveInZ, movedParts); }
			}
		}

		private static void ClearFabsecAttributes(Part fabsec)
		{
			for (int i = 0; i < 10; i++)
			{
				fabsec.SetUserProperty(ModelUDA.CurrentStageName(i), "");
				fabsec.SetUserProperty(ModelUDA.CurrentStageDate(i), "");
			}

			fabsec.SetUserProperty(ModelUDA.FabsecUniqueNumber(), "");
			fabsec.SetUserProperty(ModelUDA.PrelimMark(), "");
			fabsec.SetUserProperty(ModelUDA.PartMarkAtFab(), "");
			fabsec.SetUserProperty(ModelUDA.FabStampUDA(), "");
			fabsec.Modify();
		}

		private static string StageString(this Part part, string stageType)
		{
			string stageString = "";
			part.GetUserProperty(stageType, ref stageString);
			return stageString;
		}

		public static bool PerformNumbering()
		{
			// new MacroBuilder().Callback("acmd_partnumbers_selected", string.Empty, "main_frame").Run(); 
			// TeklaStructures.Connect();
			//  TeklaStructures.CommonTasks.PerformNumbering(false);
			PrismMacroBuilder.NumberSelected();
			return PrismWarnings.AreYouHappyWithNumbering();
		}

		public static void CreateDrawings(this List<PrismPart> selectedObjects)
		{
			FileInfo file = new FileInfo(FirmFolderLoc.DrawingWizard());
			AutoDrawingRule rule = new AutoDrawingRule(file.FullName);
			AutoDrawingsStatusEnum status;
			List<Identifier> idList = new List<Identifier>();
			foreach (PrismPart part in selectedObjects)
			{
				idList.Add(part.Part.Identifier);
			}
			DrawingCreator.CreateDrawings(rule, idList, out status);
		}

		public static void SelectParts(this List<PrismPart> partsToBeSelected)
		{
			ArrayList selectList = new ArrayList();
			foreach (PrismPart part in partsToBeSelected)
			{
				selectList.Add(part.Part);
			}
			Tekla.Structures.Model.UI.ModelObjectSelector ms = new Tekla.Structures.Model.UI.ModelObjectSelector();
			ms.Select(selectList);
			foreach (Part part in selectList)
			{
				if(part is PolyBeam polybeam && polybeam.Contour.ContourPoints.Count <= 2)
				{
					continue;
				}
				part.Modify();
			}
		}

		public static void SelectParts(this List<ModelObject> partsToBeSelected)
		{
			ArrayList selectList = new ArrayList();
			foreach (Part part in partsToBeSelected)
			{
				selectList.Add(part);
			}
			Tekla.Structures.Model.UI.ModelObjectSelector ms = new Tekla.Structures.Model.UI.ModelObjectSelector();
			ms.Select(selectList);
			foreach (Part part in selectList)
			{
				part.Modify();
			}
		}

		public static void SelectBolts(this List<BoltGroup> partsToBeSelected)
		{
			ArrayList selectList = new ArrayList();
			foreach (BoltGroup part in partsToBeSelected)
			{
				selectList.Add(part);
			}
			Tekla.Structures.Model.UI.ModelObjectSelector ms = new Tekla.Structures.Model.UI.ModelObjectSelector();
			ms.Select(selectList);
			foreach (BoltGroup part in selectList)
			{
				// part.Modify();
			}
		}

		public static void SelectAssembly(this Assembly assToBeSelected)
		{
			ArrayList selectList = new ArrayList { assToBeSelected };

			Tekla.Structures.Model.UI.ModelObjectSelector ms = new Tekla.Structures.Model.UI.ModelObjectSelector();
			ms.Select(selectList);

			assToBeSelected.Modify();

		}

		public static string GetFabsecEngRef(this Part p)
		{
			string engRef = "";
			p.GetUserProperty(ModelUDA.FabsecEngRef(), ref engRef);
			return engRef;
		}

		public static string GetPrelimMark(this Part p)
		{
			string prelim = "";
			p.GetUserProperty(ModelUDA.PrelimMark(), ref prelim);
			return prelim;
		}

		public static void HideOrRestoreTekla(int hideOrRestore)
		{
			//if hideOrRestore = 7 then minimise tekla
			//if hideOrRestore = 9 then restore tekla
			Process[] processes = Process.GetProcesses();
			foreach (Process process in processes)
			{
				if (process.MainWindowTitle.ToUpper().Contains("TEKLA"))
				{
					ShowWindow(process.MainWindowHandle, hideOrRestore);
				}
			}
		}

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		public static void RemoveLog(string folderPath)
		{
			List<string> fileTypes = new List<string>();
			if (Directory.Exists(folderPath))
			{
				foreach (string subFile in Directory.GetFiles(folderPath))
				{
					fileTypes.Add(subFile.Substring(subFile.Length - 3));
				}
				if (fileTypes.Where(x => x.Contains("bswx")).Count() == fileTypes.Where(x => x.Contains("Log")).Count())
				{
					DeleteLog(folderPath);
				}
				else
				{
					// Thread.Sleep(1000);
					RemoveLog(folderPath);
				}
			}
		}

		public static void RemoveIDDessin(string folderPath)
		{
			List<string> fileTypes = new List<string>();
			if (Directory.Exists(folderPath))
			{
				foreach (string subFile in Directory.GetFiles(folderPath))
				{
					fileTypes.Add(subFile.Substring(subFile.Length - 3));
					if (subFile.Contains("ID_dessins_KP1"))
					{
						File.Delete(subFile);
					}
				}
			}
		}

		public static void RemoveFolders(string folderPath)
		{
			if (Directory.Exists(folderPath))
			{
				File.Delete(folderPath);
			}
		}

		private static void DeleteLog(string folderPath)
		{
			foreach (string subFile in Directory.GetFiles(folderPath))
			{
				if (subFile.Substring(subFile.Length - 3) == "txt")
				{
					File.Delete(subFile);
				}
			}
		}

		public static void RedrawViews()
		{
			var selectedView = ViewHandler.GetAllViews();

			while (selectedView.MoveNext())
			{
				ViewHandler.RedrawView(selectedView.Current);
			}
		}

		public static void SetPartsRed(List<ModelObject> myParts, bool reset = true)
		{
			Color red = new Color(1, 0, 0);
			SetColouring(myParts, red, reset);
		}

		public static void SetPartsYellow(List<ModelObject> myParts, bool reset = true)
		{
			Color yellow = new Color(1, 1, 0);
			SetColouring(myParts, yellow, reset);
		}

		public static void SetPartsGreen(List<ModelObject> myParts, bool reset = true)
		{
			Color green = new Color(0, 1, 0);
			SetColouring(myParts, green, reset);
		}

		public static void SetPartsBlue(List<ModelObject> myParts, bool reset = true)
		{
			Color blue = new Color(0, 0, 1);
			SetColouring(myParts, blue, reset);
		}

		private static void SetColouring(List<ModelObject> myParts, Color color, bool reset)
		{
			if (reset)
			{
				ModelObjectVisualization.SetTemporaryStateForAll(new Color(0.5, 0.5, 0.5, 0.2));
			}
			ModelObjectVisualization.SetTemporaryState(myParts, color);
		}

		public static int ChangeSpecialTag(string newTagString, string phaseNum, string issueNum, Model model, out List<ModelObject> objects)
		{
			SelectedObjects selectedObjects = new SelectedObjects("", StageTypes.Prelim3, phaseNum, issueNum, model, false);
			ModifySpecialTag(newTagString, selectedObjects.PrismParts);
			objects = new List<ModelObject>();
			foreach (PrismPart p in selectedObjects.PrismParts)
			{
				objects.Add(p.Part);
			}
			return 0;
		}

		public static void ModifySpecialTag(string modifyTo, List<PrismPart> selectedModelParts)
		{
			foreach (PrismPart part in selectedModelParts)
			{
				part.Part.SetUserProperty(ModelUDA.SpecialFittingTag(), modifyTo);
				part.Part.Modify();
			}
		}

		public static void ModifySpecialTag(string modifyTo, List<ModelObject> selectedModelParts)
		{
			foreach (Part part in selectedModelParts)
			{
				part.SetUserProperty(ModelUDA.SpecialFittingTag(), modifyTo);
				part.Modify();
			}
		}

		public static void ModifyUDA(Part part, string Uda, string changeTo)
		{
			part.SetUserProperty(Uda, changeTo);
			//part.Modify();
		}
	}
}