using System;
using System.Collections;
using System.Collections.Generic;
using Tekla.Structures.Catalogs;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace Tekla.Structures.Concrete
{
	public class SplitRebarsInPart
	{
		private enum BarPositions
		{
			First,
			Second,
			Third,
			Forth,
			Undefined
		}

		private enum FirstOrLastEnum
		{
			Undefined,
			FirstRebar,
			LastRebar,
			MiddleRebar
		}

		private enum SpliceSection
		{
			EverySingle,
			EverySecond,
			EveryThird,
			EveryForth,
			Undefined
		}

		private const double AdditionalRebarLength = 0.05;

		private const double AngleEpsilon = 0.0001;

		private const double DistanceEpsilon = 0.001;

		private const double RebarLengthEpsilon = 12.5;

		private SingleRebar originalRebar;

		private ArrayList singleRebars;

		private SplitData splitData;

		private double startHookLength;

		private double endHookLength;

		public SplitRebarsInPart(SplitData splitData, ArrayList singleRebars)
		{
			this.splitData = splitData;
			this.singleRebars = singleRebars;
		}

		public SplitRebarsInPart()
		{
		}

		public bool SplitRebarsIfNeeded(SplitData newSplitData, ArrayList newSingleRebars, out ArrayList splitRebars)
		{
			bool result = false;
			splitRebars = new ArrayList();
			if (newSplitData != null)
			{
				splitData = newSplitData;
			}
			if (newSingleRebars != null)
			{
				singleRebars = newSingleRebars;
			}
			if (splitData != null && singleRebars != null)
			{
				result = SplitRebarsIfNeeded(out splitRebars);
			}
			return result;
		}

		public bool SplitRebarsIfNeeded(out ArrayList newSplitRebars)
		{
			bool result = false;
			double remainingOffset = 0.0;
			newSplitRebars = new ArrayList();
			if (singleRebars.Count > 0)
			{
				for (int i = 0; i < singleRebars.Count; i++)
				{
					originalRebar = singleRebars[i] as SingleRebar;
					double num = Distance.PointToPoint((Point)originalRebar.Polygon.Points[1], (Point)originalRebar.Polygon.Points[0]);
					double rebarDiameter = GetRebarDiameter(originalRebar.Size, originalRebar.Grade);
					startHookLength = GetHookRealLength(originalRebar.StartHook, rebarDiameter);
					endHookLength = GetHookRealLength(originalRebar.EndHook, rebarDiameter);
					if (originalRebar != null && num > splitData.MaxLength + GetRebarLengthEpsilon())
					{
						if (splitData.SpliceSection switch
						{
							0 => SplitEveryRebarInSameLocation(i, ref remainingOffset, new List<double>(), out var splitRebarSet) ? 1 : 0, 
							1 => SplitEverySecondRebarInSameLocation(i, ref remainingOffset, new List<double>(), out splitRebarSet) ? 1 : 0, 
							2 => SplitEveryThirdRebarInSameLocation(i, new List<double>(), ref remainingOffset, out splitRebarSet) ? 1 : 0, 
							3 => SplitEveryFourthRebarInSameLocation(i, new List<double>(), ref remainingOffset, out splitRebarSet) ? 1 : 0, 
							_ => SplitEveryRebarInSameLocation(i, ref remainingOffset, new List<double>(), out splitRebarSet) ? 1 : 0, 
						} == 0)
						{
							continue;
						}
						foreach (SingleRebar item in splitRebarSet)
						{
							newSplitRebars.Add(item);
						}
					}
					else
					{
						newSplitRebars.Add(originalRebar);
					}
				}
				result = true;
			}
			return result;
		}

		private void CalculateTotalOffset(double totalRebarLength, SpliceSection section, BarPositions barPosition, double remainingOffset, out double totalOffset, out double remainingLength, out double hookLength)
		{
			hookLength = 0.0;
			GetRebarMaximumLength(FirstOrLastEnum.MiddleRebar, out var maxLength);
			remainingLength = totalRebarLength - Math.Floor(totalRebarLength / maxLength) * maxLength;
			totalOffset = ((barPosition == BarPositions.First) ? CalculateTotalOffsetForFirstRebar(totalRebarLength, maxLength, ref remainingLength, out hookLength) : CalculateTotalOffsetForFollowingRebar(totalRebarLength, maxLength, remainingOffset, section, barPosition, ref remainingLength));
		}

		private double CalculateTotalOffsetForFirstRebar(double totalRebarLength, double maxLength, ref double remainingLength, out double hookLength)
		{
			double num = 0.0;
			hookLength = 0.0;
			int num2 = (int)Math.Floor(totalRebarLength / maxLength);
			switch (splitData.SpliceSymmetry)
			{
			case 0:
				num = splitData.SpliceOffset;
				break;
			case 1:
				num = ((num2 % 2 != 0) ? (maxLength / 2.0 + remainingLength / 2.0) : (remainingLength / 2.0));
				break;
			case 2:
				hookLength = ((startHookLength > endHookLength) ? startHookLength : endHookLength);
				remainingLength += 2.0 * hookLength;
				num = ((num2 != 1) ? 0.0 : (remainingLength / 2.0));
				break;
			}
			return num - Math.Floor(num / maxLength) * maxLength;
		}

		private double CalculateTotalOffsetForFollowingRebar(double totalRebarLength, double maxLength, double remainingOffset, SpliceSection section, BarPositions barPosition, ref double remainingLength)
		{
			double result = 0.0;
			switch (section)
			{
			case SpliceSection.EverySingle:
				Console.WriteLine("Error");
				break;
			case SpliceSection.EverySecond:
			{
				double shortRebarLength = GetShortRebarLength(maxLength, remainingOffset, 2.0);
				if (barPosition == BarPositions.Second)
				{
					int num = (int)Math.Floor(totalRebarLength / maxLength);
					switch (splitData.SpliceSymmetry)
					{
					case 0:
						result = shortRebarLength;
						break;
					case 1:
						result = ((num % 2 != 1) ? (maxLength / 2.0 + remainingLength / 2.0) : (remainingLength / 2.0));
						break;
					case 2:
						result = ((num % 2 != 0) ? (maxLength / 2.0 + remainingLength / 2.0) : (remainingLength / 2.0));
						break;
					}
				}
				else
				{
					Console.WriteLine("Error");
				}
				break;
			}
			case SpliceSection.EveryThird:
			{
				double shortRebarLength = GetShortRebarLength(maxLength, remainingOffset, 3.0);
				switch (barPosition)
				{
				case BarPositions.Second:
					result = (shortRebarLength + maxLength) / 2.0;
					break;
				case BarPositions.Third:
					result = shortRebarLength;
					break;
				default:
					Console.WriteLine("Error");
					break;
				}
				break;
			}
			case SpliceSection.EveryForth:
			{
				double shortRebarLength = GetShortRebarLength(maxLength, remainingOffset, 4.0);
				switch (barPosition)
				{
				case BarPositions.Second:
					result = 2.0 * (maxLength - shortRebarLength) / 3.0 + shortRebarLength;
					break;
				case BarPositions.Third:
					result = (maxLength - shortRebarLength) / 3.0 + shortRebarLength;
					break;
				case BarPositions.Forth:
					result = shortRebarLength;
					break;
				default:
					Console.WriteLine("Error");
					break;
				}
				break;
			}
			}
			return result;
		}

		private bool CreateDifferentMiddleRebarInCenterSymmetry(double remainingLength, SingleRebar currentOriginalRebar, ref Vector splitPattern, ref double incrementalLength, ref ArrayList splitRebarSet)
		{
			ArrayList arrayList = new ArrayList();
			double newLength = ((splitData.SpliceType != 0) ? remainingLength : (remainingLength - splitData.LappingLength));
			splitPattern.Normalize(incrementalLength);
			Vector vector = new Vector(splitPattern);
			splitPattern.Normalize(newLength);
			Vector vector2 = new Vector(splitPattern);
			arrayList.Add((Point)currentOriginalRebar.Polygon.Points[0] + vector);
			arrayList.Add((Point)currentOriginalRebar.Polygon.Points[0] + vector + vector2);
			double finalRebarLength;
			bool result = CreateSplittedRebars(FirstOrLastEnum.MiddleRebar, arrayList, splitRebarSet, out finalRebarLength);
			incrementalLength += finalRebarLength;
			return result;
		}

		private bool CreateEndRebar(SingleRebar currentOriginalRebar, double maxLength, Vector splitPattern, ArrayList splitRebarSet, ref double incrementalLength, out double lastSegment)
		{
			ArrayList arrayList = new ArrayList();
			splitPattern.Normalize(incrementalLength);
			Vector vector = new Vector(splitPattern);
			lastSegment = Distance.PointToPoint((Point)currentOriginalRebar.Polygon.Points[0] + vector, (Point)currentOriginalRebar.Polygon.Points[1]);
			arrayList.Add((Point)currentOriginalRebar.Polygon.Points[0] + vector);
			arrayList.Add(currentOriginalRebar.Polygon.Points[1]);
			double finalRebarLength;
			bool result = CreateSplittedRebars(FirstOrLastEnum.LastRebar, arrayList, splitRebarSet, out finalRebarLength);
			incrementalLength += maxLength + splitData.LappingLength / 2.0;
			return result;
		}

		private bool CreateMiddleRebar(SingleRebar currentOriginalRebar, Vector splitPattern, ArrayList splitRebarSet, ref double incrementalLength)
		{
			ArrayList arrayList = new ArrayList();
			splitPattern.Normalize(incrementalLength);
			Vector vector = new Vector(splitPattern);
			GetRebarMaximumLength(FirstOrLastEnum.MiddleRebar, out var maxLength);
			splitPattern.Normalize(maxLength);
			Vector vector2 = new Vector(splitPattern);
			arrayList.Add((Point)currentOriginalRebar.Polygon.Points[0] + vector);
			arrayList.Add((Point)currentOriginalRebar.Polygon.Points[0] + vector + vector2);
			double finalRebarLength;
			bool result = CreateSplittedRebars(FirstOrLastEnum.MiddleRebar, arrayList, splitRebarSet, out finalRebarLength);
			incrementalLength += finalRebarLength;
			return result;
		}

		private bool CreateSplittedRebars(FirstOrLastEnum position, ArrayList rebarPoints, ArrayList splitRebarSet, out double finalRebarLength)
		{
			bool result = false;
			finalRebarLength = 0.0;
			CreateSingleRebar createSingleRebar = new CreateSingleRebar(originalRebar);
			SingleRebar singleRebar = createSingleRebar.GetSingleRebar(rebarPoints);
			if (singleRebar != null)
			{
				double value = 0.0;
				singleRebar.Insert();
				singleRebar.GetReportProperty("LENGTH", ref value);
				singleRebar.Delete();
				double num = Distance.PointToPoint(singleRebar.Polygon.Points[1] as Point, singleRebar.Polygon.Points[0] as Point);
				if (num > value)
				{
					Vector vector = new Vector((Point)singleRebar.Polygon.Points[1] - (Point)singleRebar.Polygon.Points[0]);
					vector.Normalize(num + 0.05);
					rebarPoints[1] = (Point)rebarPoints[0] + vector;
					singleRebar = createSingleRebar.GetSingleRebar(rebarPoints);
				}
				if (singleRebar != null)
				{
					finalRebarLength = Distance.PointToPoint(singleRebar.Polygon.Points[1] as Point, singleRebar.Polygon.Points[0] as Point);
					switch (position)
					{
					case FirstOrLastEnum.FirstRebar:
						singleRebar.EndHook.Shape = RebarHookData.RebarHookShapeEnum.NO_HOOK;
						break;
					case FirstOrLastEnum.MiddleRebar:
						singleRebar.StartHook.Shape = RebarHookData.RebarHookShapeEnum.NO_HOOK;
						singleRebar.EndHook.Shape = RebarHookData.RebarHookShapeEnum.NO_HOOK;
						break;
					case FirstOrLastEnum.LastRebar:
						singleRebar.StartHook.Shape = RebarHookData.RebarHookShapeEnum.NO_HOOK;
						break;
					}
					splitRebarSet.Add(singleRebar);
					result = true;
				}
			}
			return result;
		}

		private bool CreateStartRebar(SingleRebar currentOriginalRebar, Vector splitPattern, ArrayList splitRebarSet, double totalOffset, double hookLength, ref double incrementalLength)
		{
			ArrayList arrayList = new ArrayList();
			if (totalOffset < 0.001)
			{
				double num;
				if (splitData.SpliceSymmetry == 2)
				{
					num = splitData.MaxLength;
					if (splitData.SpliceType == 0)
					{
						num -= splitData.LappingLength / 2.0;
					}
					num -= hookLength;
				}
				else
				{
					GetRebarMaximumLength(FirstOrLastEnum.FirstRebar, out num);
				}
				splitPattern.Normalize(num);
			}
			else
			{
				splitPattern.Normalize(totalOffset);
			}
			Vector vector = new Vector(splitPattern);
			arrayList.Add(currentOriginalRebar.Polygon.Points[0]);
			arrayList.Add((Point)currentOriginalRebar.Polygon.Points[0] + vector);
			double finalRebarLength;
			bool result = CreateSplittedRebars(FirstOrLastEnum.FirstRebar, arrayList, splitRebarSet, out finalRebarLength);
			incrementalLength += finalRebarLength;
			return result;
		}

		private double GetRebarLengthEpsilon()
		{
			double num = 12.5;
			if (splitData.SpliceType == 0)
			{
				num += splitData.LappingLength / 2.0;
			}
			return num;
		}

		private bool GetRebarMaximumLength(FirstOrLastEnum position, out double maxLength)
		{
			maxLength = splitData.MaxLength;
			switch (position)
			{
			case FirstOrLastEnum.FirstRebar:
				if (splitData.SpliceType == 0)
				{
					maxLength -= splitData.LappingLength / 2.0;
				}
				maxLength -= startHookLength;
				break;
			case FirstOrLastEnum.MiddleRebar:
				if (splitData.SpliceType == 0)
				{
					maxLength -= splitData.LappingLength;
				}
				break;
			case FirstOrLastEnum.LastRebar:
				if (splitData.SpliceType == 0)
				{
					maxLength -= splitData.LappingLength / 2.0;
				}
				maxLength -= endHookLength;
				break;
			}
			return true;
		}

		private double GetShortRebarLength(double maxLength, double remainingOffset, double barCount)
		{
			double num;
			if (remainingOffset > splitData.MinSplitDistance + splitData.LappingLength / 2.0 && remainingOffset < maxLength - (splitData.MinSplitDistance + splitData.LappingLength / 2.0))
			{
				num = remainingOffset;
			}
			else
			{
				num = remainingOffset + maxLength / barCount;
				if (num > maxLength)
				{
					num -= maxLength;
				}
			}
			return num;
		}

		private bool SplitEveryFourthRebarInSameLocation(int rebarIndex, List<double> segmentLengths, ref double remainingOffset, out ArrayList splitRebarSet)
		{
			bool result = false;
			SingleRebar singleRebar = singleRebars[rebarIndex] as SingleRebar;
			splitRebarSet = new ArrayList();
			GetRebarMaximumLength(FirstOrLastEnum.MiddleRebar, out var _);
			if (singleRebar != null)
			{
				result = ((segmentLengths.Count != 0) ? ((rebarIndex % 4) switch
				{
					0 => SplitRebar(singleRebar, SpliceSection.EveryForth, BarPositions.First, segmentLengths, ref remainingOffset, out splitRebarSet), 
					1 => SplitRebar(singleRebar, SpliceSection.EveryForth, BarPositions.Second, segmentLengths, ref remainingOffset, out splitRebarSet), 
					2 => SplitRebar(singleRebar, SpliceSection.EveryForth, BarPositions.Third, segmentLengths, ref remainingOffset, out splitRebarSet), 
					3 => SplitRebar(singleRebar, SpliceSection.EveryForth, BarPositions.Forth, segmentLengths, ref remainingOffset, out splitRebarSet), 
					_ => SplitRebar(singleRebar, SpliceSection.EveryForth, BarPositions.First, segmentLengths, ref remainingOffset, out splitRebarSet), 
				}) : ((rebarIndex % 4) switch
				{
					0 => SplitRebar(singleRebar, SpliceSection.EveryForth, BarPositions.First, ref remainingOffset, out splitRebarSet), 
					1 => SplitRebar(singleRebar, SpliceSection.EveryForth, BarPositions.Second, ref remainingOffset, out splitRebarSet), 
					2 => SplitRebar(singleRebar, SpliceSection.EveryForth, BarPositions.Third, ref remainingOffset, out splitRebarSet), 
					3 => SplitRebar(singleRebar, SpliceSection.EveryForth, BarPositions.Forth, ref remainingOffset, out splitRebarSet), 
					_ => SplitRebar(singleRebar, SpliceSection.EveryForth, BarPositions.First, ref remainingOffset, out splitRebarSet), 
				}));
			}
			return result;
		}

		private bool SplitEveryRebarInSameLocation(int rebarIndex, ref double remainingOffset, List<double> segmentLengths, out ArrayList splitRebarSet)
		{
			bool result = false;
			SingleRebar singleRebar = singleRebars[rebarIndex] as SingleRebar;
			splitRebarSet = new ArrayList();
			if (singleRebar != null)
			{
				result = ((segmentLengths.Count != 0) ? SplitRebar(singleRebar, SpliceSection.EverySingle, BarPositions.First, segmentLengths, ref remainingOffset, out splitRebarSet) : SplitRebar(singleRebar, SpliceSection.EverySingle, BarPositions.First, ref remainingOffset, out splitRebarSet));
			}
			return result;
		}

		private bool SplitEverySecondRebarInSameLocation(int rebarIndex, ref double remainingOffset, List<double> segmentLengths, out ArrayList splitRebarSet)
		{
			bool result = false;
			SingleRebar singleRebar = singleRebars[rebarIndex] as SingleRebar;
			splitRebarSet = new ArrayList();
			GetRebarMaximumLength(FirstOrLastEnum.MiddleRebar, out var _);
			if (singleRebar != null)
			{
				result = ((segmentLengths.Count == 0) ? ((rebarIndex % 2 == 0) ? SplitRebar(singleRebar, SpliceSection.EverySecond, BarPositions.First, ref remainingOffset, out splitRebarSet) : SplitRebar(singleRebar, SpliceSection.EverySecond, BarPositions.Second, ref remainingOffset, out splitRebarSet)) : ((rebarIndex % 2 == 0) ? SplitRebar(singleRebar, SpliceSection.EverySecond, BarPositions.First, segmentLengths, ref remainingOffset, out splitRebarSet) : SplitRebar(singleRebar, SpliceSection.EverySecond, BarPositions.Second, segmentLengths, ref remainingOffset, out splitRebarSet)));
			}
			return result;
		}

		private bool SplitEveryThirdRebarInSameLocation(int rebarIndex, List<double> segmentLengths, ref double remainingOffset, out ArrayList splitRebarSet)
		{
			bool result = false;
			SingleRebar singleRebar = singleRebars[rebarIndex] as SingleRebar;
			splitRebarSet = new ArrayList();
			GetRebarMaximumLength(FirstOrLastEnum.MiddleRebar, out var _);
			if (singleRebar != null)
			{
				result = ((segmentLengths.Count != 0) ? ((rebarIndex % 3) switch
				{
					0 => SplitRebar(singleRebar, SpliceSection.EveryThird, BarPositions.First, segmentLengths, ref remainingOffset, out splitRebarSet), 
					1 => SplitRebar(singleRebar, SpliceSection.EveryThird, BarPositions.Second, segmentLengths, ref remainingOffset, out splitRebarSet), 
					2 => SplitRebar(singleRebar, SpliceSection.EveryThird, BarPositions.Third, segmentLengths, ref remainingOffset, out splitRebarSet), 
					_ => SplitRebar(singleRebar, SpliceSection.EveryThird, BarPositions.First, segmentLengths, ref remainingOffset, out splitRebarSet), 
				}) : ((rebarIndex % 3) switch
				{
					0 => SplitRebar(singleRebar, SpliceSection.EveryThird, BarPositions.First, ref remainingOffset, out splitRebarSet), 
					1 => SplitRebar(singleRebar, SpliceSection.EveryThird, BarPositions.Second, ref remainingOffset, out splitRebarSet), 
					2 => SplitRebar(singleRebar, SpliceSection.EveryThird, BarPositions.Third, ref remainingOffset, out splitRebarSet), 
					_ => SplitRebar(singleRebar, SpliceSection.EveryThird, BarPositions.First, ref remainingOffset, out splitRebarSet), 
				}));
			}
			return result;
		}

		private bool SplitRebar(SingleRebar originalRebar, SpliceSection section, BarPositions barPosition, ref double remainingOffset, out ArrayList splitRebarSet)
		{
			bool flag = true;
			double incrementalLength = 0.0;
			splitRebarSet = new ArrayList();
			double num = Distance.PointToPoint((Point)originalRebar.Polygon.Points[1], (Point)originalRebar.Polygon.Points[0]);
			Vector splitPattern = new Vector((Point)originalRebar.Polygon.Points[1] - (Point)originalRebar.Polygon.Points[0]);
			GetRebarMaximumLength(FirstOrLastEnum.MiddleRebar, out var maxLength);
			CalculateTotalOffset(num, section, barPosition, remainingOffset, out var totalOffset, out var remainingLength, out var hookLength);
			int num2 = 0;
			while (incrementalLength + GetRebarLengthEpsilon() < num)
			{
				if (num2 == 0)
				{
					flag &= CreateStartRebar(originalRebar, splitPattern, splitRebarSet, totalOffset, hookLength, ref incrementalLength);
				}
				else if (incrementalLength + maxLength + GetRebarLengthEpsilon() < num)
				{
					int num3 = (int)Math.Floor(num / maxLength);
					flag = ((splitData.SpliceSymmetry != 2 || barPosition != 0 || num3 <= 1 || !(incrementalLength < num / 2.0) || !(incrementalLength + maxLength > num / 2.0)) ? (flag & CreateMiddleRebar(originalRebar, splitPattern, splitRebarSet, ref incrementalLength)) : (flag & CreateDifferentMiddleRebarInCenterSymmetry(remainingLength, originalRebar, ref splitPattern, ref incrementalLength, ref splitRebarSet)));
				}
				else
				{
					flag &= CreateEndRebar(originalRebar, maxLength, splitPattern, splitRebarSet, ref incrementalLength, out var lastSegment);
					if (barPosition == BarPositions.First)
					{
						remainingOffset = lastSegment;
					}
				}
				num2++;
			}
			return flag;
		}

		private bool SplitRebar(SingleRebar originalRebar, SpliceSection section, BarPositions barPosition, List<double> rebarSegmentLengths, ref double remainingOffset, out ArrayList splitRebarSet)
		{
			bool flag = true;
			double incrementalLength = 0.0;
			double num = 0.0;
			double num2 = 0.0;
			splitRebarSet = new ArrayList();
			ArrayList arrayList = new ArrayList();
			foreach (double rebarSegmentLength in rebarSegmentLengths)
			{
				num += rebarSegmentLength;
			}
			CreateSingleRebar createSingleRebar = new CreateSingleRebar(originalRebar);
			for (int i = 0; i < originalRebar.Polygon.Points.Count - 1; i++)
			{
				Vector splitPattern = new Vector((Point)originalRebar.Polygon.Points[i + 1] - (Point)originalRebar.Polygon.Points[i]);
				GetRebarMaximumLength(FirstOrLastEnum.MiddleRebar, out var maxLength);
				CalculateTotalOffset(num, section, barPosition, remainingOffset, out var totalOffset, out var remainingLength, out var hookLength);
				ArrayList splitRebarSet2 = new ArrayList();
				ArrayList arrayList2 = new ArrayList();
				arrayList2.Add(originalRebar.Polygon.Points[i]);
				arrayList2.Add(originalRebar.Polygon.Points[i + 1]);
				SingleRebar singleRebar = createSingleRebar.GetSingleRebar(arrayList2);
				if (num2 + rebarSegmentLengths[i] > maxLength)
				{
					int num3 = 0;
					while (incrementalLength + GetRebarLengthEpsilon() < num)
					{
						if (num3 == 0 && i == 0)
						{
							flag &= CreateStartRebar(singleRebar, splitPattern, splitRebarSet2, totalOffset, hookLength, ref incrementalLength);
						}
						else if (incrementalLength + maxLength + GetRebarLengthEpsilon() < num)
						{
							int num4 = (int)Math.Floor(num / maxLength);
							flag = ((splitData.SpliceSymmetry != 2 || barPosition != 0 || num4 <= 1 || !(incrementalLength < num / 2.0) || !(incrementalLength + maxLength > num / 2.0)) ? (flag & CreateMiddleRebar(singleRebar, splitPattern, splitRebarSet2, ref incrementalLength)) : (flag & CreateDifferentMiddleRebarInCenterSymmetry(remainingLength, singleRebar, ref splitPattern, ref incrementalLength, ref splitRebarSet2)));
						}
						else
						{
							double incrementalLength2;
							if (incrementalLength < maxLength + 0.001)
							{
								incrementalLength2 = rebarSegmentLengths[i] - (maxLength - incrementalLength);
							}
							else
							{
								int num5 = (int)(incrementalLength / maxLength);
								incrementalLength2 = rebarSegmentLengths[i] - ((double)(num5 + 1) * maxLength - incrementalLength);
							}
							flag &= CreateEndRebar(singleRebar, maxLength, splitPattern, splitRebarSet2, ref incrementalLength2, out var lastSegment);
							incrementalLength = incrementalLength2;
							if (barPosition == BarPositions.First)
							{
								remainingOffset = lastSegment;
							}
						}
						num3++;
					}
				}
				else
				{
					incrementalLength += rebarSegmentLengths[i];
					num2 += rebarSegmentLengths[i];
					AddRebarPoint(originalRebar.Polygon.Points[i] as Point, arrayList);
					AddRebarPoint(originalRebar.Polygon.Points[i + 1] as Point, arrayList);
					DrawPoint(originalRebar.Polygon.Points[i] as Point, i.ToString());
					DrawPoint(originalRebar.Polygon.Points[i + 1] as Point, (i + 1).ToString());
				}
				if (splitRebarSet2.Count != 0 && arrayList.Count > 0)
				{
					DrawPoint(((SingleRebar)splitRebarSet2[0]).Polygon.Points[0] as Point, "SP" + i);
					DrawPoint(((SingleRebar)splitRebarSet2[0]).Polygon.Points[1] as Point, "SP" + (i + 1));
					arrayList.Add(((SingleRebar)splitRebarSet2[0]).Polygon.Points[0]);
					SingleRebar singleRebar2 = createSingleRebar.GetSingleRebar(arrayList);
					splitRebarSet2.Insert(0, singleRebar2);
					arrayList.Clear();
					num2 = 0.0;
				}
				splitRebarSet.AddRange(splitRebarSet2);
			}
			return flag;
		}

		private void DrawPoint(Point inputPoint, string comment)
		{
			GraphicsDrawer graphicsDrawer = new GraphicsDrawer();
			graphicsDrawer.DrawText(inputPoint, comment, new Color(1.0, 0.0, 0.0));
		}

		private void AddRebarPoint(Point rebarPoint, ArrayList list)
		{
			if (rebarPoint != null && !list.Contains(rebarPoint))
			{
				list.Add(rebarPoint);
			}
		}

		private double GetHookRealLength(RebarHookData rebarHookData, double actualRebarSize)
		{
			double radius = rebarHookData.Radius;
			double length = rebarHookData.Length;
			double angle = rebarHookData.Angle;
			double num = Math.Abs(angle) / 180.0 * Math.PI;
			double num2 = radius + actualRebarSize / 2.0;
			double num3 = num * num2;
			double result = length + num3 - (num2 + actualRebarSize / 2.0);
			if (length < 0.001)
			{
				result = 0.0;
			}
			return result;
		}

		private double GetRebarDiameter(string size, string grade)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected O, but got Unknown
			RebarItem val = new RebarItem();
			val.Select(grade, size);
			return val.get_ActualDiameter();
		}
	}
}
