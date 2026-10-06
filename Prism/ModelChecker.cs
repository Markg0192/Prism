using Prism.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using static Prism.Enums;

namespace Prism
{
	/// <summary>
	/// The pre run checks class is used to check the model for any issues in the model that may cause the program to crash.
	/// These are ran before the main program and warns the user of the problem and closes the app before it crashes.
	/// </summary>
	public static class ModelChecker
	{
		public static List<PrismPart> IncorrectNameAndClass = new List<PrismPart>();
		public static List<PrismPart> MissingExecutionClass = new List<PrismPart>();
		public static List<PrismPart> HasNoFinish = new List<PrismPart>();
		//  public static List<PrismPart> StartNumbersDoNotMatch = new List<PrismPart>();
		//  public static List<PrismPart> PhasesDoNotMatch = new List<PrismPart>();
		//   public static List<PrismPart> PhasesDoNotMatchParts = new List<PrismPart>();
		public static List<ModelObject> NotOrderedParts = new List<ModelObject>();
		public static List<ModelObject> OrderedParts = new List<ModelObject>();
		public static List<PrismPart> PartsWithoutIntumescentLoading = new List<PrismPart>();
		public static List<PrismPart> IncorrectOrientation = new List<PrismPart>();

		public static int StartNumbersDontMatch = 0;
		public static int PhasesDontMatch = 0;

		public static void BoltThrough2Ply(SelectedObjects selectedObjects)
		{
			foreach (PrismBoltGroup pbg in selectedObjects.PrismBoltGroups)
			{

				int executionClassData = 10;  //this is the number read from teklas UDA when no execution class is applied, we are defaulting to it not having one here
											  // p.GetUserProperty(ModelUDA.ExcecutionClass(), ref executionClassData);

				if (executionClassData == 10)
				{
					//MissingExecutionClass.Add(p);
				}
			}
		}

		public static bool RunStage4Checks(this SelectedObjects selectedObjects, string userName)
		{
			Factory location = PrismWarnings.FactoryLocation();
			if (location == Factory.Unknown)
			{
				PrismWarnings.IgnoreFittingCheck();
			}

			foreach (PrismPart myMainPart in selectedObjects.GetMainParts())
			{
				GetUnorderedParts(myMainPart.Part);
				GetPartsWithoutAFinish(myMainPart);
				CheckForIntumescentLoading(myMainPart);

				ArrayList mySecondaries = myMainPart.Part.GetAssembly().GetSecondaries();
				foreach (Part mySecondaryPart in mySecondaries)
				{
					GetPartsThatStartNumbersDontMatch(myMainPart, mySecondaryPart, selectedObjects);
					GetPartsWherePhasesDontMatch(myMainPart, mySecondaryPart, selectedObjects);
					CheckFittings.GetIncorrectFittings(mySecondaryPart, location);
				}
			}
			return CheckForAndActionErrors(userName, selectedObjects.PrismBoltGroups, selectedObjects);
		}

		private static List<BoltGroup> GetShearStudBoltGroups(List<BoltGroup> boltGroups)
		{
			return boltGroups
				.Where(boltGroup => boltGroup.BoltStandard == "SHEAR-STUD" && CheckBoltProperty(boltGroup))
				.ToList();
		}

		private static bool CheckBoltProperty(BoltGroup boltGroup)
		{
			string property = "";
			boltGroup.GetReportProperty(ModelUDA.BoltShearStudTag(), ref property);
			return string.IsNullOrEmpty(property); // Return true if the property is not null or empty
		}

		private static bool IsShearStud(List<BoltGroup> boltGroup)
		{
			string property = null;
			return boltGroup.Any(bolt =>
			{
				if (bolt.BoltStandard == "SHEAR-STUD")
				{
					property = "";
					bolt.GetReportProperty(ModelUDA.BoltShearStudTag(), ref property);
					return property == "";
				}
				return false;
			});
		}

		public static void ClearOldLists()
		{
			CheckFittings.IncorrectGrade.Clear();
			CheckFittings.IncorrectLength.Clear();
			CheckFittings.IncorrectThickness.Clear();
			CheckFittings.UnOrderedObjects.Clear();
			CheckFittings.AllIncorrectPlate.Clear();
			HasNoFinish.Clear();
			PartsWithoutIntumescentLoading.Clear();
			OrderedParts.Clear();
			NotOrderedParts.Clear();
			//   StartNumbersDoNotMatch.Clear();
			//   PhasesDoNotMatch.Clear();
			//   PhasesDoNotMatchParts.Clear();
		}

		public static bool MemberOrientationIsCorrect(SelectedObjects selectedObjects, out IgnoreType ignore)
		{
			IncorrectOrientation.Clear();
			MemberOrientation(selectedObjects);
			//ModelChecker.IncorrectOrientation.Clear(); //if this line is active all orientation functionallity is disabled

			ignore = PrismWarnings.DisplayOrderErrors(IncorrectOrientation, Error.Orientation);

			if (ignore == IgnoreType.Stop)
			{
				return false;
			}
			return true;
		}

		public static void HasExecutionClass(SelectedObjects selectedObjects)
		{
			foreach (PrismPart p in selectedObjects.GetMainParts())
			{
				int executionClassData = 10;  //this is the number read from teklas UDA when no execution class is applied, we are defaulting to it not having one here
				p.Part.GetUserProperty(ModelUDA.ExcecutionClass(), ref executionClassData);

				if (executionClassData == 10)
				{
					p.PartErrors.Add(Error.Execution);
					MissingExecutionClass.Add(p);
				}
			}
		}

		public static void HasExecutionClass(PrismPart mainPart)
		{
			int executionClassData = 10;  //this is the number read from teklas UDA when no execution class is applied, we are defaulting to it not having one here
			mainPart.Part.GetUserProperty(ModelUDA.ExcecutionClass(), ref executionClassData);

			if (executionClassData == 10)
			{
				mainPart.PartErrors.Add(Error.Execution);
				MissingExecutionClass.Add(mainPart);
			}
		}

		public static void MemberOrientation(SelectedObjects selectedObjects)
		{
			int tolerance = 5;
			foreach (PrismPart pPart in selectedObjects.GetMainParts())
			{
				Beam b = pPart.Part as Beam;
				if (b != null && (b.Profile.ProfileString.StartsWith("UB") || b.Profile.ProfileString.StartsWith("UKB")
					|| b.Profile.ProfileString.StartsWith("UC") || b.Profile.ProfileString.StartsWith("UKC")

					|| b.Profile.ProfileString.StartsWith("IPE") || b.Profile.ProfileString.StartsWith("IPEA")
					|| b.Profile.ProfileString.StartsWith("IPN") || b.Profile.ProfileString.StartsWith("HAU")
					|| b.Profile.ProfileString.StartsWith("HD") || b.Profile.ProfileString.StartsWith("HEM")
					|| b.Profile.ProfileString.StartsWith("HEA") || b.Profile.ProfileString.StartsWith("HEB")
					))
				{
					if (b.Name == GdomValues.BeamName && Math.Abs(b.StartPoint.Z - b.EndPoint.Z) < tolerance)
					{
						CheckBeamOrientation(b, pPart);
					}

					if (b.Name == GdomValues.ColumnName)
					{
						CheckColumnOrientation(b, pPart);
					}

					if (b.Name.Contains(GdomValues.RafterName))
					{
						CheckRafterOrientation(b, pPart);
					}
					if (b.Name == GdomValues.BraceName)
					{
						// CheckBraceOrientation(b);
					}
				}
			}
		}

		public static void MemberOrientation(PrismPart mainPart)
		{
			int tolerance = 5;

			Beam b = mainPart.Part as Beam;
			if (b != null && (b.Profile.ProfileString.StartsWith("UB") || b.Profile.ProfileString.StartsWith("UKB")
				|| b.Profile.ProfileString.StartsWith("UC") || b.Profile.ProfileString.StartsWith("UKC")

				|| b.Profile.ProfileString.StartsWith("IPE") || b.Profile.ProfileString.StartsWith("IPEA")
				|| b.Profile.ProfileString.StartsWith("IPN") || b.Profile.ProfileString.StartsWith("HAU")
				|| b.Profile.ProfileString.StartsWith("HD") || b.Profile.ProfileString.StartsWith("HEM")
				|| b.Profile.ProfileString.StartsWith("HEA") || b.Profile.ProfileString.StartsWith("HEB")
				))
			{
				if (b.Name == GdomValues.BeamName && Math.Abs(b.StartPoint.Z - b.EndPoint.Z) < tolerance)
				{
					CheckBeamOrientation(b, mainPart);
				}

				if (b.Name == GdomValues.ColumnName)
				{
					CheckColumnOrientation(b, mainPart);
				}

				if (b.Name.Contains(GdomValues.RafterName))
				{
					CheckRafterOrientation(b, mainPart);
				}
				if (b.Name == GdomValues.BraceName)
				{
					// CheckBraceOrientation(b);
				}
			}

		}

		private static void CheckBraceOrientation(Beam b)
		{
			bool check1 = false;
			bool check2 = false;

			bool isVertical = b.StartPoint.X == b.EndPoint.X && b.StartPoint.Y == b.EndPoint.Y ? true : false;

			Point3D p1 = new Point3D(b.StartPoint.X, b.StartPoint.Y, b.StartPoint.Z + 100);
			Point3D p2 = new Point3D(b.StartPoint);
			Point3D p3 = new Point3D(b.EndPoint);

			Geometry.Vector v = new Geometry.Vector(p2, p1);
			Geometry.Vector v2 = new Geometry.Vector(p2, p3);
			double radian = (double)v.AngleBetween(v2);
			double degree = AnglesHelper.Degrees(radian);

			Point startPoint = b.StartPoint;
			Point endPoint = b.EndPoint;
			if (isVertical)
			{
				if (b.StartPoint.Z > b.EndPoint.Z)
				{
					IncorrectOrientation.Add(new PrismPart(b));
				}
			}
			else
			{
				double a = startPoint.X - endPoint.X;
				double o = startPoint.Y - endPoint.Y;
				double angle = Math.Atan2(o, a);
				double myAngle = 180 * angle / Math.PI;

				if (myAngle < 44.6 && myAngle > -135.4 || myAngle == 180)
				{
					check1 = true;
				}

				if (myAngle == 90 || myAngle == -90)
				{
					a = o;
				}

				double o1 = startPoint.Z - endPoint.Z;
				double angle2 = Math.Atan2(o1, a);
				double myAngle2 = 180 * angle2 / Math.PI;

				if (check1)
				{
					if (myAngle2 <= 44.6 && myAngle2 >= -135.4)
					{
						check2 = true;
					}
				}
				else
				{
					if (myAngle2 < 44.6 && myAngle2 >= -135.4)
					{
						check2 = true;
					}
				}

				if (check2)
				{
					IncorrectOrientation.Add(new PrismPart(b));
				}
			}
		}

		private static void CheckRafterOrientation(Beam b, PrismPart pPart)
		{
			//All rafters must be detailed with start point at the apex, so a simple check to make sure start point is higher than end point will do here
			if (b.StartPoint.Z < b.EndPoint.Z)
			{
				pPart.PartErrors.Add(Error.Orientation);
				IncorrectOrientation.Add(new PrismPart(b));
			}
		}

		private static void CheckColumnOrientation(Beam b, PrismPart pPart)
		{
			//Column rotation must be "FRONT" or "BELOW", therefore "BACK" and "TOP" are wrong, start point must also be lower than end point.
			if (b.Position.Rotation == Position.RotationEnum.BACK || b.Position.Rotation == Position.RotationEnum.TOP || b.StartPoint.Z > b.EndPoint.Z)
			{
				pPart.PartErrors.Add(Error.Orientation);
				IncorrectOrientation.Add(new PrismPart(b));
			}
		}

		private static void CheckBeamOrientation(Beam b, PrismPart pPart)
		{
			bool check1 = false;
			bool check2 = false;

			Point startPoint = b.StartPoint;
			Point endPoint = b.EndPoint;

			double a = startPoint.X - endPoint.X;
			double o = startPoint.Y - endPoint.Y;
			double angle = Math.Atan2(o, a);
			double myAngle = 180 * angle / Math.PI;

			if (myAngle > -44.6 && myAngle < 135.4)
			{
				check1 = true;
			}

			if (check1 || check2)
			{
				pPart.PartErrors.Add(Error.Orientation);
				IncorrectOrientation.Add(new PrismPart(b));
			}
		}

		public static void NameAndClassAligned(SelectedObjects selectedObjects)
		{
			var partClass = GdomValues.PartClass();

			foreach (var p in selectedObjects.PrismParts)
			{
				if (p.Part.Name.IndexOf("TEMP", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					if (p.Part.Class != GdomValues.TemporaryObjectClass)
					{
						p.PartErrors.Add(Error.NameAndClass);
						IncorrectNameAndClass.Add(p);
					}
				}
				else
				{
					if (partClass.TryGetValue(p.Part.Name, out var meantToBeClass) && meantToBeClass != null && !meantToBeClass.Contains(p.Part.Class))
					{
						p.PartErrors.Add(Error.NameAndClass);
						IncorrectNameAndClass.Add(p);
					}
				}
			}
		}

		public static void NameAndClassAligned(PrismPart p)
		{
			var partClass = GdomValues.PartClass();

			if (p.Part.Name.IndexOf("TEMP", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				if (p.Part.Class != GdomValues.TemporaryObjectClass)
				{
					p.PartErrors.Add(Error.NameAndClass);
					IncorrectNameAndClass.Add(p);
				}
			}
			else
			{
				if (partClass.TryGetValue(p.Part.Name, out var meantToBeClass) && meantToBeClass != null && !meantToBeClass.Contains(p.Part.Class))
				{
					p.PartErrors.Add(Error.NameAndClass);
					IncorrectNameAndClass.Add(p);
				}
			}
		}

		public static bool ArePreviousStepsComplete(SelectedObjects selectedObjects, int stageNumber)
		{
			List<PrismPart> incompleteParts = selectedObjects.GetNonSeversafeParts()
				.Where(p =>
				{
					string userProperty = "";
					p.Part.GetUserProperty(ModelUDA.PreviousStageName(stageNumber), ref userProperty);
					return userProperty == "";
				})
				.Select(p => p)
				.ToList();

			if (incompleteParts.Any())
			{
				PrismWarnings.PreviousStepIncomplete(incompleteParts);
				return false;
			}

			return true;
		}

		public static bool ArePreviousStepsComplete(SelectedObjects selectedObjects, int stageNumber, bool forWpfInterface)
		{
			List<PrismPart> incompleteParts = selectedObjects.GetNonSeversafeParts()
				.Where(p =>
				{
					string userProperty = "";
					p.Part.GetUserProperty(ModelUDA.PreviousStageName(stageNumber), ref userProperty);
					return userProperty == "";
				})
				.Select(p => p)
				.ToList();

			if (incompleteParts.Any())
			{
				foreach (PrismPart part in incompleteParts)
				{
					part.PartErrors.Add(Error.PreviousStepIncomplete);
				}
				return false;
			}

			return true;
		}

		public static bool AreFabsecCarcassesOrdered(SelectedObjects selectedObjects)
		{
			bool allOrdered = true;

			foreach (PrismPart fabsec in selectedObjects.GetFabsecParts())
			{
				string carcassOrdered = "";
				fabsec.Part.GetUserProperty(ModelUDA.FabsecCarcassOrdered(), ref carcassOrdered);

				if (!string.IsNullOrWhiteSpace(carcassOrdered)) continue;

				if (!fabsec.PartErrors.Contains(Error.FabsecCarcassNotOrdered))
					fabsec.PartErrors.Add(Error.FabsecCarcassNotOrdered);

				allOrdered = false;
			}

			return allOrdered;
		}

		public static bool AreFabPackChecksComplete(SelectedObjects selectedObjects)
		{
			List<PrismPart> incompleteParts = selectedObjects.GetNonSeversafeParts()
				.Where(part =>
				{
					string completedBy = "";
					part.Part.GetUserProperty(ModelUDA.FabCheckCompleteUser(), ref completedBy);
					return string.IsNullOrWhiteSpace(completedBy);
				})
				.ToList();

			if (!incompleteParts.Any()) return true;

			foreach (PrismPart part in incompleteParts)
			{
				if (!part.PartErrors.Contains(Error.PreviousStepIncomplete))
					part.PartErrors.Add(Error.PreviousStepIncomplete);
			}

			return false;
		}

		public static bool ConfirmShearStudOrder(string userName, List<PrismBoltGroup> prismBoltGroups, bool requiredForFabPackChecks)
		{
			List<PrismBoltGroup> unorderedShearStuds = prismBoltGroups
				.Where(pbg => pbg != null && pbg.isShearStud && !ShearStudIsTaggedOrdered(pbg))
				.ToList();

			if (!unorderedShearStuds.Any()) return true;

			if (PrismWarnings.TagShearStudsAsOrdered())
			{
				foreach (PrismBoltGroup prismBoltGroup in unorderedShearStuds)
				{
					BoltGroup boltGroup = prismBoltGroup.BoltGroup;
					if (boltGroup == null) continue;

					boltGroup.SetUserProperty(ModelUDA.BoltOrderedBy(), userName);
					boltGroup.SetUserProperty(ModelUDA.BoltShearStudTag(), "Ordered");
					prismBoltGroup.isOrdered = true;
				}

				return true;
			}

			if (!requiredForFabPackChecks) return true;

			PrismWarnings.ShearStudsBlockFabPackChecks();
			return false;
		}

		private static bool ShearStudIsTaggedOrdered(PrismBoltGroup prismBoltGroup)
		{
			BoltGroup boltGroup = prismBoltGroup.BoltGroup;
			if (boltGroup == null) return false;

			string orderedTag = "";
			boltGroup.GetReportProperty(ModelUDA.BoltShearStudTag(), ref orderedTag);
			return string.Equals(orderedTag, "Ordered", StringComparison.OrdinalIgnoreCase);
		}

		private static bool CheckForAndActionErrors(string userName, List<PrismBoltGroup> prismBoltGroups, SelectedObjects selectedObjects)
		{
			if (!OrderErrors()) { return false; }
			if (!FinishErrors()) { return false; }

			IgnoreType startNumberError = StartNumbersErrors(selectedObjects, out List<PrismPart> startNumberErrorParts);
			if (startNumberError == IgnoreType.AutoFix)
			{
				StartNumbersDontMatch = startNumberErrorParts.Count;
				AutoFix.AssemblyAndStartNumbers(startNumberErrorParts);
			}
			if (startNumberError == IgnoreType.Stop) { return false; }

			IgnoreType phaseMatchError = PhaseMatchErrors(selectedObjects, out List<PrismPart> phaseErrorParts);
			if (phaseMatchError == IgnoreType.AutoFix)
			{
				PhasesDontMatch = phaseErrorParts.Count;
				AutoFix.PartPhasing(phaseErrorParts);
			}
			if (phaseMatchError == IgnoreType.Stop) { return false; }

			if (!IntumesecentLoadingErrors()) { return false; }

			if (!ShearStudsNotOrdered(userName, prismBoltGroups)) { return false; }

			return CheckFittings.DisplayFittingErrors();
		}

		private static bool ShearStudsNotOrdered(string userName, List<PrismBoltGroup> prismBoltGroups)
		{
			if (prismBoltGroups.Any(pbg => !pbg.isOrdered && pbg.isShearStud)) // then there are shears studs that appear to not be ordered.
			{
				bool studsOrdered = PrismWarnings.UnorderedShearStuds();

				// If studs are not ordered and user decides not to ignore, return false
				if (!studsOrdered && !PrismWarnings.IgnoreAndContinue())
				{
					return false;
				}

				// Modify attributes only if studs have been ordered
				if (studsOrdered)
				{
					foreach (PrismBoltGroup boltGroup in prismBoltGroups.Where(pbg => !pbg.isOrdered && pbg.isShearStud))
					{
						string property = "";
						boltGroup.BoltGroup.GetReportProperty(ModelUDA.BoltShearStudTag(), ref property);
						if (property == "")
						{
							boltGroup.BoltGroup.SetUserProperty(ModelUDA.BoltOrderedBy(), userName);
							boltGroup.BoltGroup.SetUserProperty(ModelUDA.BoltShearStudTag(), "Ordered");
						}
					}
				}
			}
			return true;
		}

		public static IgnoreType PhaseMatchErrors(SelectedObjects selectedObjects, out List<PrismPart> partsWithError)
		{
			partsWithError = selectedObjects.GetPartsWithPhaseNotMatchingError();

			int errorCount = partsWithError.Count;

			if (errorCount != 0)
			{
				PrismWarnings.Warning = errorCount.ToString();

				PrismWarnings.PhasesDontMatch(partsWithError);

				ModelModifiers.SetPartsRed(Convertor.PrismPartsToModelObjects(partsWithError));

				return PrismWarnings.NewIgnoreWarning();
			}
			return IgnoreType.Unspecified;
		}

		public static IgnoreType StartNumbersErrors(SelectedObjects selectedObjects, out List<PrismPart> partsWithError)
		{
			partsWithError = selectedObjects.GetPartsWithStartNumberError();
			if (partsWithError.Count != 0)
			{
				PrismWarnings.Warning = partsWithError.Count.ToString();

				PrismWarnings.StartNumbersDontMatch(partsWithError);

				ModelModifiers.SetPartsRed(Convertor.PrismPartsToModelObjects(partsWithError));

				return PrismWarnings.NewIgnoreWarning();
			}
			return IgnoreType.Unspecified;
		}

		public static bool OrderErrors()
		{
			if (NotOrderedParts.Count != 0)
			{
				PrismWarnings.Warning = NotOrderedParts.Count.ToString();

				PrismWarnings.HasNotBeenOrdered();

				ModelModifiers.SetPartsRed(NotOrderedParts);

				return PrismWarnings.IgnoreWarning();
			}
			return true;
		}

		public static bool IntumesecentLoadingErrors()
		{
			if (PartsWithoutIntumescentLoading.Count != 0)
			{
				PrismWarnings.Warning = PartsWithoutIntumescentLoading.Count.ToString();

				PrismWarnings.IntumescentLoadingMissing(PartsWithoutIntumescentLoading);

				ModelModifiers.SetPartsRed(Convertor.PrismPartsToModelObjects(PartsWithoutIntumescentLoading));

				return PrismWarnings.IgnoreIntumescentLoading();
			}
			return true;
		}

		public static bool FinishErrors()
		{
			if (HasNoFinish.Count != 0)
			{
				PrismWarnings.Warning = HasNoFinish.Count.ToString();

				PrismWarnings.HasNoFinish(HasNoFinish);

				ModelModifiers.SetPartsRed(Convertor.PrismPartsToModelObjects(HasNoFinish));

				return PrismWarnings.IgnoreWarning();
			}
			return true;
		}

		public static void GetPartsThatStartNumbersDontMatch(this PrismPart mainPart, Part secondaryPart, SelectedObjects selectedObjects)
		{
			int mainPartNumber = mainPart.Part.AssemblyNumber.StartNumber;
			if (mainPartNumber != secondaryPart.PartNumber.StartNumber)
			{
				PrismPart p = selectedObjects.PrismParts.Find(x => x.Guid == secondaryPart.Identifier.GUID.ToString());
				if (p == null)
				{
					p = new PrismPart(secondaryPart);
					selectedObjects.PrismParts.Add(p);
				}
				p.StartNumberDoesntMatch = true;
				p.StartNumber = mainPartNumber;
				p.PartErrors.Add(Error.SecondaryNumberingMismatch);
			}
		}

		public static void GetPartsWherePhasesDontMatch(this PrismPart mainPart, Part secondaryPart, SelectedObjects selectedObjects)
		{
			secondaryPart.GetPhase(out Phase secondaryPhase);

			if (mainPart.Phase.PhaseNumber != secondaryPhase.PhaseNumber)
			{
				PrismPart p = selectedObjects.PrismParts.Find(x => x.Guid == secondaryPart.Identifier.GUID.ToString());
				if (p == null)
				{
					p = new PrismPart(secondaryPart);
					selectedObjects.PrismParts.Add(new PrismPart(secondaryPart));
				}
				p.PhaseDoesntMatchMain = true;
				p.Phase = mainPart.Phase;
				p.PartErrors.Add(Error.SecondaryPhasingMismatch);
			}
		}

		public static void GetPartsWithoutAFinish(this PrismPart mainPart)
		{
			if (mainPart.Finish.Length == 0)
			{
				HasNoFinish.Add(mainPart);
				mainPart.PartErrors.Add(Error.FinishMissing);
			}
		}

		public static bool HasBeenOrdered(this Part mainPart, bool skipMessages)
		{
			string prelimMark = "";
			mainPart.GetUserProperty(ModelUDA.CurrentStageName(3), ref prelimMark); //Check prism uda material order complete for data
			if (prelimMark.Length == 0)
			{
				if (skipMessages) { return false; }
			}
			return true;
		}

		public static void GetUnorderedParts(this Part mainPart)
		{
			if (!mainPart.Profile.ProfileString.Contains("PLT") && !mainPart.Profile.ProfileString.Contains("FLT"))
			{
				string prelimMark = "";
				mainPart.GetUserProperty(ModelUDA.CurrentStageName(3), ref prelimMark); //Check prism uda material order complete for data    
				if (prelimMark.Length == 0)
				{
					NotOrderedParts.Add(mainPart);

				}
				else
				{
					OrderedParts.Add(mainPart);
				}
			}
		}

		public static void CheckPartsAreOrdered(this PrismPart mainPart)
		{
			if (!mainPart.Part.Profile.ProfileString.Contains("PLT") && !mainPart.Part.Profile.ProfileString.Contains("FLT"))
			{
				string prelimMark = "";
				mainPart.Part.GetUserProperty(ModelUDA.CurrentStageName(3), ref prelimMark); //Check prism uda material order complete for data    
				if (prelimMark.Length == 0)
				{
					mainPart.PartErrors.Add(Error.PartNotOrdered);
				}
			}

		}

		public static void CheckForIntumescentLoading(this PrismPart mainPart)
		{
			if (mainPart.Finish.StartsWith(GdomValues.IntumescentCode))
			{
				if (IsMissingProperties(mainPart.SherwinDft, mainPart.SherwinWft))
				{
					if (IsMissingProperties(mainPart.HempelDft, mainPart.HempelWft))
					{
						PartsWithoutIntumescentLoading.Add(mainPart);
						mainPart.PartErrors.Add(Error.IntumescentLoadingMissing);
					}
				}
			}
		}

		private static bool IsMissingProperties(string dft, string wft)
		{
			return string.IsNullOrEmpty(dft) && string.IsNullOrEmpty(wft);
		}

		public static bool NameAndClassAlign(SelectedObjects myObjects)
		{
			IncorrectNameAndClass.Clear();
			NameAndClassAligned(myObjects);
			IgnoreType ignore = PrismWarnings.DisplayOrderErrors(IncorrectNameAndClass, Error.NameAndClass);

			if (ignore == IgnoreType.AutoFix)
			{
				AutoFix.PartNameAndClass();
			}
			if (ignore == IgnoreType.Stop)
			{
				return false;
			}
			return true;
		}

		public static bool PartsHaveExecutionClass(SelectedObjects myObjects)
		{
			MissingExecutionClass.Clear();
			HasExecutionClass(myObjects);
			IgnoreType ignore = PrismWarnings.DisplayOrderErrors(MissingExecutionClass, Error.Execution);

			if (ignore == IgnoreType.AutoFix)
			{
				AutoFix.ExecutionClass();
			}
			if (ignore == IgnoreType.Stop)
			{
				return false;
			}
			return true;
		}

		public static bool IsCurrentUser(params string[] userNames)
		{
			return userNames.Any(userName => string.Equals(Environment.UserName, userName, StringComparison.OrdinalIgnoreCase));
		}
	}

}