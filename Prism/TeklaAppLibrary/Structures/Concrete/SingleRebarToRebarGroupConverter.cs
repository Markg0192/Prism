using System;
using System.Collections;
using System.Collections.Generic;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace Tekla.Structures.Concrete
{
	public class SingleRebarToRebarGroupConverter
	{
		private enum GroupType
		{
			Undefined,
			Normal,
			Tapered
		}

		private const double AngleEpsilon = 0.01;

		private const double Degress90 = Math.PI / 2.0;

		private const double DistanceEpsilon = 0.1;

		private const double DotEpsilon = 0.5;

		private const double MaximumIterations = 10.0;

		private readonly Vector coordinateZ;

		private readonly RebarGroupConversionData rebarGroupConversionData;

		private readonly ArrayList singleRebars;

		private ArrayList consideredRebarsIndex;

		private ArrayList rebarGroups;

		private ArrayList ungroupedRebars;

		public SingleRebarToRebarGroupConverter(RebarGroupConversionData newRebarGroupConversionData, ArrayList newSingleRebars)
		{
			rebarGroupConversionData = newRebarGroupConversionData;
			singleRebars = newSingleRebars;
			rebarGroups = new ArrayList();
			ungroupedRebars = new ArrayList();
			CoordinateSystem coordinateSystem = rebarGroupConversionData.FatherSlab.GetCoordinateSystem();
			coordinateZ = coordinateSystem.AxisY.Cross(coordinateSystem.AxisX);
		}

		public static Vector GetHookDirection(Reinforcement rebar)
		{
			Vector result = null;
			rebar.Insert();
			ArrayList rebarGeometries = rebar.GetRebarGeometries(withHooks: true);
			ArrayList rebarGeometries2 = rebar.GetRebarGeometries(withHooks: false);
			rebar.Delete();
			if (rebarGeometries.Count > 0)
			{
				RebarGeometry rebarGeometry = rebarGeometries[0] as RebarGeometry;
				RebarGeometry rebarGeometry2 = rebarGeometries2[0] as RebarGeometry;
				ArrayList points = rebarGeometry.Shape.Points;
				ArrayList points2 = rebarGeometry2.Shape.Points;
				if (points.Count > 2)
				{
					Point point = new Point(points[0] as Point);
					Point point2 = new Point(points2[0] as Point);
					if (Distance.PointToPoint(point, point2) > 0.1)
					{
						result = new Vector(point - point2);
					}
					else
					{
						point = points[points.Count - 1] as Point;
						point2 = points2[points2.Count - 1] as Point;
						if (Distance.PointToPoint(point, point2) > 0.1)
						{
							result = new Vector(point - point2);
						}
					}
				}
			}
			return result;
		}

		public static void SwapStartHookWithEndHook(ref SingleRebar newRebar)
		{
			RebarHookData rebarHookData = new RebarHookData
			{
				Shape = newRebar.StartHook.Shape,
				Angle = newRebar.StartHook.Angle,
				Radius = newRebar.StartHook.Radius,
				Length = newRebar.StartHook.Length
			};
			newRebar.StartHook.Shape = newRebar.EndHook.Shape;
			newRebar.StartHook.Angle = newRebar.EndHook.Angle;
			newRebar.StartHook.Radius = newRebar.EndHook.Radius;
			newRebar.StartHook.Length = newRebar.EndHook.Length;
			newRebar.EndHook.Shape = rebarHookData.Shape;
			newRebar.EndHook.Angle = rebarHookData.Angle;
			newRebar.EndHook.Radius = rebarHookData.Radius;
			newRebar.EndHook.Length = rebarHookData.Length;
		}

		public bool ConvertSingleRebarToRebarGroup(bool splitForSplicing, out ArrayList newRebarGroups, out ArrayList newUngroupedRebars)
		{
			bool result = false;
			newRebarGroups = new ArrayList();
			newUngroupedRebars = new ArrayList();
			consideredRebarsIndex = new ArrayList();
			if (CheckIfRebarsAreInSamePlane())
			{
				GetRebarGroups();
				GetUngroupedRebars();
				if (splitForSplicing)
				{
					SplitRebarGroupsForSplicing();
				}
				newRebarGroups = new ArrayList(rebarGroups);
				newUngroupedRebars = new ArrayList(ungroupedRebars);
				result = true;
			}
			else if (CheckIfRebarsCanBeTaperedCurved() && singleRebars.Count > 2)
			{
				GetCircleTaperedGroup();
				newRebarGroups = new ArrayList(rebarGroups);
				newUngroupedRebars = new ArrayList(ungroupedRebars);
				result = true;
			}
			else
			{
				newUngroupedRebars = new ArrayList(singleRebars);
			}
			return result;
		}

		private bool AddParallelRebarsToGroup(ArrayList parallelRebars, GroupType type, ref ArrayList newRebarGroups, ref ArrayList newUngroupedRebars)
		{
			bool result = false;
			SingleRebarToRebarGroupConverter singleRebarToRebarGroupConverter = new SingleRebarToRebarGroupConverter(rebarGroupConversionData, parallelRebars);
			if (singleRebarToRebarGroupConverter.ConvertSingleRebarToRebarGroup(splitForSplicing: false, out var newRebarGroups2, out var newUngroupedRebars2))
			{
				if (newRebarGroups2.Count > 0)
				{
					foreach (RebarGroup item in newRebarGroups2)
					{
						if (!RebarGroupArrayListContainsRebarGroup(item, newRebarGroups))
						{
							newRebarGroups.Add(item);
						}
					}
				}
				if (newUngroupedRebars2.Count > 0)
				{
					foreach (SingleRebar item2 in newUngroupedRebars2)
					{
						if (!RebarArrayListContainsSingleRebar(item2, newUngroupedRebars))
						{
							newUngroupedRebars.Add(item2);
						}
					}
				}
				result = true;
			}
			return result;
		}

		private bool AddRebarsIfDoNotOverlap(SingleRebar newRebar, Vector primaryRebarVector, Vector secondaryRebarVector, ArrayList newParallelRebars)
		{
			bool result = false;
			if (RebarsDoNotOverlap(primaryRebarVector, secondaryRebarVector))
			{
				newParallelRebars.Add(newRebar);
				result = true;
			}
			return result;
		}

		private bool AddRebarsToConsideredRebarsIndex(ArrayList newGroupRebars)
		{
			bool result = false;
			foreach (SingleRebar newGroupRebar in newGroupRebars)
			{
				Point point = new Point((Point)newGroupRebar.Polygon.Points[0]);
				Point point2 = new Point((Point)newGroupRebar.Polygon.Points[1]);
				for (int i = 0; i < singleRebars.Count; i++)
				{
					SingleRebar singleRebar2 = singleRebars[i] as SingleRebar;
					if (singleRebar2 != null && Distance.PointToPoint(point, (Point)singleRebar2.Polygon.Points[0]) < 0.1 && Distance.PointToPoint(point2, (Point)singleRebar2.Polygon.Points[1]) < 0.1)
					{
						consideredRebarsIndex.Add(i);
						result = true;
					}
				}
			}
			return result;
		}

		private bool CheckIfRebarsCanBeTaperedCurved()
		{
			bool result = true;
			if (singleRebars.Count > 1 && rebarGroupConversionData.StirrupType != 0)
			{
				SingleRebar singleRebar = singleRebars[0] as SingleRebar;
				Line line = new Line((Point)singleRebar.Polygon.Points[1], (Point)singleRebar.Polygon.Points[0]);
				GeometricPlane plane = new GeometricPlane((Point)singleRebar.Polygon.Points[0], new Vector((Point)singleRebar.Polygon.Points[1] - (Point)singleRebar.Polygon.Points[0]));
				List<Point> list = new List<Point>();
				foreach (SingleRebar singleRebar2 in singleRebars)
				{
					Line line2 = new Line((Point)singleRebar2.Polygon.Points[1], (Point)singleRebar2.Polygon.Points[0]);
					if (Parallel.LineToLine(line2, line))
					{
						Point point = Intersection.LineToPlane(line2, plane);
						AddPointToList(point, list);
					}
					else
					{
						result = false;
					}
				}
				Point point2 = new Point();
				foreach (Point item in list)
				{
					point2 += item;
				}
				point2.X /= list.Count;
				point2.Y /= list.Count;
				point2.Z /= list.Count;
				double num = Distance.PointToPoint(point2, list[0]);
				foreach (Point item2 in list)
				{
					if (Math.Abs(num - Distance.PointToPoint(point2, item2)) >= 0.1)
					{
						result = false;
					}
				}
			}
			return result;
		}

		private void AddPointToList(Point point, List<Point> pointList)
		{
			bool flag = true;
			foreach (Point point2 in pointList)
			{
				if (Distance.PointToPoint(point, point2) < 0.1)
				{
					flag = false;
				}
			}
			if (flag)
			{
				pointList.Add(point);
			}
		}

		private bool CheckIfRebarsAreInSamePlane()
		{
			bool result = false;
			if (singleRebars.Count > 1 && rebarGroupConversionData.StirrupType == RebarGroup.RebarGroupStirrupTypeEnum.STIRRUP_TYPE_POLYGONAL)
			{
				SingleRebar singleRebar = singleRebars[0] as SingleRebar;
				SingleRebar singleRebar2 = singleRebars[1] as SingleRebar;
				if (singleRebar != null && singleRebar2 != null)
				{
					GeometricPlane plane = new GeometricPlane((Point)singleRebar.Polygon.Points[0], new Vector((Point)singleRebar.Polygon.Points[1] - (Point)singleRebar.Polygon.Points[0]), new Vector((Point)singleRebar2.Polygon.Points[0] - (Point)singleRebar.Polygon.Points[0]));
					foreach (SingleRebar singleRebar3 in singleRebars)
					{
						if (Distance.PointToPlane((Point)singleRebar3.Polygon.Points[0], plane) > 0.1 || Distance.PointToPlane((Point)singleRebar3.Polygon.Points[1], plane) > 0.1)
						{
							throw new Exception("The original reinforcement bars are not located in the same plane.");
						}
					}
					result = true;
				}
			}
			else if (singleRebars.Count == 1)
			{
				result = true;
			}
			return result;
		}

		private bool CreateNormalOrTaperedRebarGroup(ArrayList parallelRebars, GroupType type, out RebarGroup newRebarGroup)
		{
			bool result = false;
			newRebarGroup = new RebarGroup();
			if (GetPolygonForRebarGroup(parallelRebars, out var polygon, out var polygon2))
			{
				newRebarGroup = GetRebarGroup(parallelRebars, type, polygon, polygon2, checkHooks: true);
				result = true;
			}
			return result;
		}

		private bool CreateRebarGroupBySingleRebar(int rebarIndex, ArrayList groupLines, GroupType type)
		{
			bool result = false;
			if (FindRebarsInSameGroup(rebarIndex, groupLines, out var parallelRebars))
			{
				ArrayList arrayList = SplitInDifferentGroupsIfNeeded(parallelRebars);
				if (arrayList != null)
				{
					foreach (ArrayList item in arrayList)
					{
						if (item.Count >= 2 && CreateNormalOrTaperedRebarGroup(item, type, out var newRebarGroup))
						{
							rebarGroups.Add(newRebarGroup);
							AddRebarsToConsideredRebarsIndex(item);
							result = true;
						}
					}
				}
			}
			return result;
		}

		private bool FindRebarGroups(int primaryRebarIndex, ArrayList groupLines, out ArrayList parallelRebars)
		{
			bool result = false;
			parallelRebars = new ArrayList();
			SingleRebar singleRebar = singleRebars[primaryRebarIndex] as SingleRebar;
			if (singleRebar != null)
			{
				parallelRebars.Add(singleRebar);
			}
			for (int i = primaryRebarIndex + 1; i < singleRebars.Count; i++)
			{
				SingleRebar singleRebar2 = singleRebars[i] as SingleRebar;
				if (singleRebar2 != null && !consideredRebarsIndex.Contains(i) && Distance.PointToLine((Point)singleRebar2.Polygon.Points[0], (Line)groupLines[0]) < 0.1 && Distance.PointToLine((Point)singleRebar2.Polygon.Points[1], (Line)groupLines[1]) < 0.1)
				{
					parallelRebars.Add(singleRebar2);
					result = true;
				}
			}
			return result;
		}

		private bool FindRebarsInSameGroup(int primaryRebarIndex, ArrayList groupLines, out ArrayList parallelRebars)
		{
			bool result = false;
			parallelRebars = new ArrayList();
			SingleRebar singleRebar = singleRebars[primaryRebarIndex] as SingleRebar;
			if (singleRebar != null)
			{
				result = FindRebarGroups(primaryRebarIndex, groupLines, out parallelRebars);
			}
			return result;
		}

		private double GetDistanceRebars(SingleRebar primaryRebar, SingleRebar secondaryRebar)
		{
			Line line = new Line((Point)secondaryRebar.Polygon.Points[1], (Point)secondaryRebar.Polygon.Points[0]);
			Point point = Projection.PointToLine((Point)primaryRebar.Polygon.Points[0], line);
			return Distance.PointToPoint((Point)primaryRebar.Polygon.Points[0], point);
		}

		private bool GetGroupLine(int primaryRebarIndex, int secondaryRebarIndex, out ArrayList groupLines)
		{
			bool result = false;
			groupLines = new ArrayList();
			SingleRebar singleRebar = singleRebars[primaryRebarIndex] as SingleRebar;
			SingleRebar singleRebar2 = singleRebars[secondaryRebarIndex] as SingleRebar;
			if (singleRebar != null && singleRebar2 != null)
			{
				groupLines.Add(new Line((Point)singleRebar2.Polygon.Points[0], (Point)singleRebar.Polygon.Points[0]));
				groupLines.Add(new Line((Point)singleRebar2.Polygon.Points[1], (Point)singleRebar.Polygon.Points[1]));
				if (!Parallel.LineToLine((Line)groupLines[0], (Line)groupLines[1], 0.01) || !(0.1 > Distance.PointToLine((Point)singleRebar.Polygon.Points[0], (Line)groupLines[1])))
				{
					result = true;
				}
			}
			return result;
		}

		private ArrayList GetGroupSegments(ArrayList groupGeometries)
		{
			ArrayList arrayList = new ArrayList();
			if (groupGeometries.Count > 1)
			{
				RebarGeometry rebarGeometry = groupGeometries[0] as RebarGeometry;
				RebarGeometry rebarGeometry2 = groupGeometries[groupGeometries.Count - 1] as RebarGeometry;
				if (rebarGeometry != null && rebarGeometry2 != null)
				{
					ArrayList points = rebarGeometry.Shape.Points;
					ArrayList points2 = rebarGeometry2.Shape.Points;
					arrayList.Add(new LineSegment((Point)points[0], (Point)points2[0]));
					arrayList.Add(new LineSegment((Point)points[1], (Point)points2[1]));
				}
			}
			return arrayList;
		}

		private bool GetNormalRebarGroup(int primaryRebarIndex)
		{
			bool flag = false;
			for (int i = primaryRebarIndex + 1; i < singleRebars.Count; i++)
			{
				if (flag)
				{
					break;
				}
				if (consideredRebarsIndex.Contains(i) || !GetGroupLine(primaryRebarIndex, i, out var groupLines))
				{
					continue;
				}
				Line line = groupLines[0] as Line;
				Line line2 = groupLines[1] as Line;
				SingleRebar singleRebar = singleRebars[primaryRebarIndex] as SingleRebar;
				if (line != null && line2 != null && singleRebar != null)
				{
					Vector vector = new Vector((Point)singleRebar.Polygon.Points[1] - (Point)singleRebar.Polygon.Points[0]);
					if (Parallel.LineToLine(line, line2, 0.01) && Math.Abs(vector.Dot(line.Direction)) < 0.5)
					{
						flag = CreateRebarGroupBySingleRebar(primaryRebarIndex, groupLines, GroupType.Normal);
					}
				}
			}
			return flag;
		}

		private bool GetParallelRebarsToCreateNewGroups(RebarGroup primaryRebarGroup, ArrayList primaryGroupGeometries, ArrayList secondaryGroupGeometries, out ArrayList newParallelRebars, out ArrayList remainingParallelRebars)
		{
			bool result = false;
			newParallelRebars = new ArrayList();
			remainingParallelRebars = new ArrayList();
			foreach (RebarGeometry primaryGroupGeometry in primaryGroupGeometries)
			{
				bool flag = false;
				ArrayList points = primaryGroupGeometry.Shape.Points;
				CreateSingleRebar createSingleRebar = new CreateSingleRebar(primaryRebarGroup);
				SingleRebar singleRebar = createSingleRebar.GetSingleRebar(points);
				foreach (RebarGeometry secondaryGroupGeometry in secondaryGroupGeometries)
				{
					if (!flag && singleRebar != null)
					{
						ArrayList points2 = secondaryGroupGeometry.Shape.Points;
						if (Distance.PointToPoint((Point)points[0], (Point)points2[0]) < 0.1 || Distance.PointToPoint((Point)points[1], (Point)points2[1]) < 0.1)
						{
							Vector primaryRebarVector = new Vector((Point)points[1] - (Point)points[0]);
							Vector secondaryRebarVector = new Vector((Point)points2[1] - (Point)points2[0]);
							flag = AddRebarsIfDoNotOverlap(singleRebar, primaryRebarVector, secondaryRebarVector, newParallelRebars);
						}
						else if (Distance.PointToPoint((Point)points[0], (Point)points2[1]) < 0.1 || Distance.PointToPoint((Point)points[1], (Point)points2[0]) < 0.1)
						{
							Vector primaryRebarVector2 = new Vector((Point)points[1] - (Point)points[0]);
							Vector secondaryRebarVector2 = new Vector((Point)points2[0] - (Point)points2[1]);
							flag = AddRebarsIfDoNotOverlap(singleRebar, primaryRebarVector2, secondaryRebarVector2, newParallelRebars);
						}
					}
				}
				if (!flag)
				{
					remainingParallelRebars.Add(singleRebar);
				}
			}
			if (newParallelRebars.Count > 0)
			{
				result = true;
			}
			return result;
		}

		private bool GetPolygonForRebarGroup(ArrayList parallelRebars, out Polygon polygon1, out Polygon polygon2)
		{
			bool result = false;
			polygon1 = new Polygon();
			polygon2 = new Polygon();
			SingleRebar singleRebar = parallelRebars[0] as SingleRebar;
			SingleRebar singleRebar2 = parallelRebars[parallelRebars.Count - 1] as SingleRebar;
			CoordinateSystem coordinateSystem = rebarGroupConversionData.FatherSlab.GetCoordinateSystem();
			if (singleRebar != null && singleRebar2 != null)
			{
				Line line = new Line(singleRebar.Polygon.Points[0] as Point, singleRebar.Polygon.Points[1] as Point);
				Line line2 = new Line(singleRebar2.Polygon.Points[0] as Point, singleRebar2.Polygon.Points[1] as Point);
				if (Distance.PointToLine(coordinateSystem.Origin, line) > Distance.PointToLine(coordinateSystem.Origin, line2))
				{
					SingleRebar singleRebar3 = singleRebar;
					singleRebar = singleRebar2;
					singleRebar2 = singleRebar3;
				}
				polygon1.Points = singleRebar2.Polygon.Points;
				polygon2.Points = singleRebar.Polygon.Points;
				result = true;
			}
			return result;
		}

		private RebarGroup GetRebarGroup(ArrayList parallelRebars, GroupType type, Polygon polygon1, Polygon polygon2, bool checkHooks)
		{
			RebarGroup rebarGroup = new RebarGroup();
			SingleRebar singleRebar = parallelRebars[0] as SingleRebar;
			SingleRebar singleRebar2 = parallelRebars[1] as SingleRebar;
			if (singleRebar != null && singleRebar2 != null)
			{
				Line line = new Line((Point)singleRebar2.Polygon.Points[1], (Point)singleRebar2.Polygon.Points[0]);
				Point point = Projection.PointToLine((Point)singleRebar.Polygon.Points[0], line);
				double num = Distance.PointToPoint((Point)singleRebar.Polygon.Points[0], point);
				rebarGroup.Polygons.Add(polygon1);
				if (type == GroupType.Normal)
				{
					rebarGroup.StartPoint = (Point)polygon1.Points[0];
					rebarGroup.EndPoint = (Point)polygon2.Points[0];
				}
				else
				{
					rebarGroup.Polygons.Add(polygon2);
				}
				rebarGroup.SpacingType = BaseRebarGroup.RebarGroupSpacingTypeEnum.SPACING_TYPE_TARGET_SPACE;
				rebarGroup.Spacings.Add(num);
				rebarGroup.ExcludeType = BaseRebarGroup.ExcludeTypeEnum.EXCLUDE_TYPE_NONE;
				rebarGroup.Father = singleRebar.Father;
				rebarGroup.NumberingSeries.Prefix = singleRebar.NumberingSeries.Prefix;
				rebarGroup.NumberingSeries.StartNumber = singleRebar.NumberingSeries.StartNumber;
				rebarGroup.Name = singleRebar.Name;
				rebarGroup.Size = singleRebar.Size;
				rebarGroup.Grade = singleRebar.Grade;
				rebarGroup.RadiusValues = singleRebar.RadiusValues;
				rebarGroup.Class = singleRebar.Class;
				rebarGroup.StartHook.Shape = singleRebar.StartHook.Shape;
				rebarGroup.StartHook.Angle = singleRebar.StartHook.Angle;
				rebarGroup.StartHook.Radius = singleRebar.StartHook.Radius;
				rebarGroup.StartHook.Length = singleRebar.StartHook.Length;
				rebarGroup.EndHook.Shape = singleRebar.EndHook.Shape;
				rebarGroup.EndHook.Angle = singleRebar.EndHook.Angle;
				rebarGroup.EndHook.Radius = singleRebar.EndHook.Radius;
				rebarGroup.EndHook.Length = singleRebar.EndHook.Length;
				rebarGroup.OnPlaneOffsets = singleRebar.OnPlaneOffsets;
				rebarGroup.FromPlaneOffset = singleRebar.FromPlaneOffset;
				rebarGroup.StartPointOffsetValue = singleRebar.StartPointOffsetValue;
				rebarGroup.StartPointOffsetType = singleRebar.StartPointOffsetType;
				rebarGroup.EndPointOffsetValue = singleRebar.EndPointOffsetValue;
				rebarGroup.EndPointOffsetType = singleRebar.EndPointOffsetType;
				if (checkHooks && RebarHooksShouldBeTurned(rebarGroup))
				{
					if (rebarGroup.StartHook.Shape != 0)
					{
						rebarGroup.StartHook.Shape = RebarHookData.RebarHookShapeEnum.CUSTOM_HOOK;
						rebarGroup.StartHook.Angle = -1.0 * singleRebar.StartHook.Angle;
					}
					if (rebarGroup.EndHook.Shape != 0)
					{
						rebarGroup.EndHook.Shape = RebarHookData.RebarHookShapeEnum.CUSTOM_HOOK;
						rebarGroup.EndHook.Angle = -1.0 * singleRebar.EndHook.Angle;
					}
				}
			}
			return rebarGroup;
		}

		private void GetRebarGroups()
		{
			if (singleRebars.Count <= 1)
			{
				return;
			}
			for (int i = 0; i < singleRebars.Count - 1; i++)
			{
				if (!consideredRebarsIndex.Contains(i))
				{
					GetNormalRebarGroup(i);
				}
			}
			for (int j = 0; j < singleRebars.Count - 1; j++)
			{
				if (!consideredRebarsIndex.Contains(j))
				{
					GetTaperedRebarGroup(j);
				}
			}
		}

		private void GetCircleTaperedGroup()
		{
			for (int i = 0; i < singleRebars.Count; i++)
			{
				if (singleRebars.Count <= 2)
				{
					break;
				}
				SingleRebar singleRebar = singleRebars[0] as SingleRebar;
				GeometricPlane plane = new GeometricPlane((Point)singleRebar.Polygon.Points[0], new Vector((Point)singleRebar.Polygon.Points[1] - (Point)singleRebar.Polygon.Points[0]));
				List<Polygon> list = new List<Polygon>();
				ArrayList arrayList = new ArrayList();
				for (int j = 0; j < singleRebars.Count; j++)
				{
					SingleRebar singleRebar2 = singleRebars[j] as SingleRebar;
					LineSegment lineSegment = new LineSegment((Point)singleRebar2.Polygon.Points[1], (Point)singleRebar2.Polygon.Points[0]);
					if (Intersection.LineSegmentToPlane(lineSegment, plane) != null)
					{
						list.Add(singleRebar2.Polygon);
						arrayList.Add(singleRebar2);
						singleRebars.RemoveAt(j);
						j--;
					}
				}
				if (list.Count > 2)
				{
					RebarGroup rebarGroup = GetRebarGroup(arrayList, GroupType.Tapered, list[0], list[1], checkHooks: false);
					rebarGroup.Polygons.Clear();
					rebarGroup.Polygons.Add(list[1]);
					rebarGroup.Polygons.Add(list[0]);
					rebarGroup.Polygons.Add(list[2]);
					rebarGroup.StirrupType = RebarGroup.RebarGroupStirrupTypeEnum.STIRRUP_TYPE_TAPERED_CURVED;
					rebarGroup.SpacingType = BaseRebarGroup.RebarGroupSpacingTypeEnum.SPACING_TYPE_EXACT_NUMBER;
					rebarGroup.Spacings.Clear();
					rebarGroup.Spacings.Add(list.Count);
					rebarGroups.Add(rebarGroup);
				}
			}
			ungroupedRebars.AddRange(singleRebars);
		}

		private bool GetTaperedRebarGroup(int primaryRebarIndex)
		{
			bool flag = false;
			for (int i = primaryRebarIndex + 1; i < singleRebars.Count; i++)
			{
				if (flag)
				{
					break;
				}
				if (!consideredRebarsIndex.Contains(i) && GetGroupLine(primaryRebarIndex, i, out var groupLines))
				{
					flag = CreateRebarGroupBySingleRebar(primaryRebarIndex, groupLines, GroupType.Tapered);
				}
			}
			return flag;
		}

		private void GetUngroupedRebars()
		{
			for (int i = 0; i < singleRebars.Count; i++)
			{
				if (!consideredRebarsIndex.Contains(i))
				{
					SingleRebar newRebar = singleRebars[i] as SingleRebar;
					if (newRebar != null && RebarHooksShouldBeTurned(newRebar))
					{
						ArrayList rebarPoints = new ArrayList
						{
							newRebar.Polygon.Points[1],
							newRebar.Polygon.Points[0]
						};
						CreateSingleRebar createSingleRebar = new CreateSingleRebar(newRebar);
						newRebar = createSingleRebar.GetSingleRebar(rebarPoints);
						SwapStartHookWithEndHook(ref newRebar);
					}
					ungroupedRebars.Add(newRebar);
				}
			}
		}

		private bool GroupShouldBeSplitted(SingleRebar primaryRebar, SingleRebar secondaryRebar, ref double previusDistanceRebars)
		{
			bool result = false;
			double distanceRebars = GetDistanceRebars(primaryRebar, secondaryRebar);
			if (previusDistanceRebars > 0.1 && Math.Abs(distanceRebars - previusDistanceRebars) > 0.1)
			{
				result = true;
			}
			previusDistanceRebars = distanceRebars;
			return result;
		}

		private bool IsFirstRebarDistanceEqual(SingleRebar firstRebar, SingleRebar secondRebar, SingleRebar thirdRebar)
		{
			bool result = false;
			double distanceRebars = GetDistanceRebars(firstRebar, secondRebar);
			double distanceRebars2 = GetDistanceRebars(secondRebar, thirdRebar);
			if (Math.Abs(distanceRebars - distanceRebars2) < 0.1)
			{
				result = true;
			}
			return result;
		}

		private bool LineSegmentsOverlapButAreNotIdentical(LineSegment primarySegment, LineSegment secondarySegment)
		{
			return Parallel.LineSegmentToLineSegment(primarySegment, secondarySegment, 0.01) && (Distance.PointToLineSegment(secondarySegment.Point1, primarySegment) < 0.1 || Distance.PointToLineSegment(secondarySegment.Point2, primarySegment) < 0.1) && (!(Distance.PointToPoint(primarySegment.Point1, secondarySegment.Point1) < 0.1) || !(Distance.PointToPoint(primarySegment.Point2, secondarySegment.Point2) < 0.1));
		}

		private bool PrimaryRebarGroupShouldBeSplit(ArrayList primaryGroupGeometries, ArrayList secondaryGroupGeometries)
		{
			bool flag = false;
			ArrayList groupSegments = GetGroupSegments(primaryGroupGeometries);
			ArrayList groupSegments2 = GetGroupSegments(secondaryGroupGeometries);
			foreach (LineSegment item in groupSegments)
			{
				foreach (LineSegment item2 in groupSegments2)
				{
					if (item.Length() > item2.Length())
					{
						flag |= LineSegmentsOverlapButAreNotIdentical(item, item2) && RebarGroupsDoNotOverlap((RebarGeometry)primaryGroupGeometries[0], (RebarGeometry)secondaryGroupGeometries[0]);
					}
				}
			}
			return flag;
		}

		private bool PrimaryRebarGroupShouldBeSplit(RebarGroup primaryRebarGroup, ArrayList primaryGroupGeometries, SingleRebar secondaryRebar, out RebarGeometry splitRebarGeometry, out ArrayList remainingParallelRebars)
		{
			bool result = false;
			splitRebarGeometry = null;
			remainingParallelRebars = new ArrayList();
			for (int i = 0; i < primaryGroupGeometries.Count; i++)
			{
				RebarGeometry rebarGeometry = primaryGroupGeometries[i] as RebarGeometry;
				if (rebarGeometry == null)
				{
					continue;
				}
				ArrayList points = rebarGeometry.Shape.Points;
				if (Distance.PointToPoint((Point)points[0], (Point)secondaryRebar.Polygon.Points[0]) < 0.1 || Distance.PointToPoint((Point)points[0], (Point)secondaryRebar.Polygon.Points[1]) < 0.1 || Distance.PointToPoint((Point)points[1], (Point)secondaryRebar.Polygon.Points[0]) < 0.1 || Distance.PointToPoint((Point)points[1], (Point)secondaryRebar.Polygon.Points[1]) < 0.1)
				{
					splitRebarGeometry = rebarGeometry;
					result = true;
					continue;
				}
				CreateSingleRebar createSingleRebar = new CreateSingleRebar(primaryRebarGroup);
				SingleRebar singleRebar = createSingleRebar.GetSingleRebar(points);
				if (singleRebar != null)
				{
					remainingParallelRebars.Add(singleRebar);
				}
			}
			return result;
		}

		private bool RebarArrayListContainsSingleRebar(SingleRebar rebar, ArrayList rebarsArray)
		{
			bool result = false;
			foreach (SingleRebar item in rebarsArray)
			{
				if ((Distance.PointToPoint((Point)rebar.Polygon.Points[0], (Point)item.Polygon.Points[0]) < 0.1 && Distance.PointToPoint((Point)rebar.Polygon.Points[1], (Point)item.Polygon.Points[1]) < 0.1) || (Distance.PointToPoint((Point)rebar.Polygon.Points[1], (Point)item.Polygon.Points[0]) < 0.1 && Distance.PointToPoint((Point)rebar.Polygon.Points[0], (Point)item.Polygon.Points[1]) < 0.1))
				{
					result = true;
				}
			}
			return result;
		}

		private bool RebarGroupArrayListContainsRebarGroup(RebarGroup rebarGroup, ArrayList rebarGroupsArray)
		{
			bool result = false;
			foreach (RebarGroup item in rebarGroupsArray)
			{
				if (rebarGroup.Polygons.Count != item.Polygons.Count)
				{
					continue;
				}
				Polygon polygon = rebarGroup.Polygons[0] as Polygon;
				Polygon polygon2 = item.Polygons[0] as Polygon;
				if (polygon == null || polygon2 == null || ((!(Distance.PointToPoint((Point)polygon.Points[0], (Point)polygon2.Points[0]) < 0.1) || !(Distance.PointToPoint((Point)polygon.Points[1], (Point)polygon2.Points[1]) < 0.1)) && (!(Distance.PointToPoint((Point)polygon.Points[1], (Point)polygon2.Points[0]) < 0.1) || !(Distance.PointToPoint((Point)polygon.Points[0], (Point)polygon2.Points[1]) < 0.1))))
				{
					continue;
				}
				if (rebarGroup.Polygons.Count == 1)
				{
					if ((Distance.PointToPoint(rebarGroup.StartPoint, item.StartPoint) < 0.1 && Distance.PointToPoint(rebarGroup.EndPoint, item.EndPoint) < 0.1) || (Distance.PointToPoint(rebarGroup.EndPoint, item.StartPoint) < 0.1 && Distance.PointToPoint(rebarGroup.StartPoint, item.EndPoint) < 0.1))
					{
						result = true;
					}
				}
				else if (rebarGroup.Polygons.Count == 2)
				{
					Polygon polygon3 = rebarGroup.Polygons[1] as Polygon;
					Polygon polygon4 = item.Polygons[1] as Polygon;
					if (polygon3 != null && polygon4 != null && ((Distance.PointToPoint((Point)polygon3.Points[0], (Point)polygon4.Points[0]) < 0.1 && Distance.PointToPoint((Point)polygon3.Points[1], (Point)polygon4.Points[1]) < 0.1) || (Distance.PointToPoint((Point)polygon3.Points[1], (Point)polygon4.Points[0]) < 0.1 && Distance.PointToPoint((Point)polygon3.Points[0], (Point)polygon4.Points[1]) < 0.1)))
					{
						result = true;
					}
				}
			}
			return result;
		}

		private bool RebarGroupsDoNotOverlap(RebarGeometry primaryRebarGeometry, RebarGeometry secondaryRebarGeometry)
		{
			bool result = false;
			ArrayList points = primaryRebarGeometry.Shape.Points;
			ArrayList points2 = secondaryRebarGeometry.Shape.Points;
			double num = Distance.PointToPoint((Point)points[0], (Point)points2[0]);
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < points.Count; i++)
			{
				for (int j = 0; j < points2.Count; j++)
				{
					if (Distance.PointToPoint((Point)points[i], (Point)points2[j]) < num)
					{
						num = Distance.PointToPoint((Point)points[i], (Point)points2[j]);
						num2 = i;
						num3 = j;
					}
				}
			}
			int index = ((num2 == 0) ? 1 : 0);
			int index2 = ((num3 == 0) ? 1 : 0);
			Vector vector = new Vector((Point)points[index] - (Point)points[num2]);
			Vector vector2 = new Vector((Point)points2[index2] - (Point)points2[num3]);
			if (vector.Dot(vector2) < 0.5)
			{
				result = true;
			}
			return result;
		}

		private bool RebarHooksShouldBeTurned(Reinforcement currentRebar)
		{
			bool result = false;
			Vector hookDirection = GetHookDirection(currentRebar);
			if (rebarGroupConversionData.DepthLocation != int.MinValue && hookDirection != null && ((rebarGroupConversionData.DepthLocation == 0) ^ (hookDirection.GetAngleBetween(coordinateZ) < Math.PI / 2.0)))
			{
				result = true;
			}
			return result;
		}

		private bool RebarsDoNotOverlap(Vector primaryVector, Vector secondaryVector)
		{
			return primaryVector.Dot(secondaryVector) < 0.0;
		}

		private void SplitGroupsWithTwoRebarsOrMore(ArrayList parallelRebars, ArrayList newRebarGroups)
		{
			bool flag = false;
			int num = 0;
			double previusDistanceRebars = 0.0;
			for (int i = 0; i < parallelRebars.Count - 1; i++)
			{
				SingleRebar singleRebar = parallelRebars[i] as SingleRebar;
				SingleRebar singleRebar2 = parallelRebars[i + 1] as SingleRebar;
				if (singleRebar == null || singleRebar2 == null)
				{
					continue;
				}
				ArrayList arrayList = (ArrayList)newRebarGroups[num];
				arrayList.Add(singleRebar);
				if (flag)
				{
					if (!IsFirstRebarDistanceEqual(arrayList[0] as SingleRebar, singleRebar, singleRebar2))
					{
						arrayList.RemoveAt(0);
					}
					flag = false;
				}
				if (GroupShouldBeSplitted(singleRebar, singleRebar2, ref previusDistanceRebars))
				{
					if (arrayList.Count != 0)
					{
						if (arrayList.Count == 1)
						{
							arrayList.RemoveAt(0);
						}
						else if (arrayList.Count == 2)
						{
							arrayList.RemoveAt(0);
							flag = true;
						}
						else
						{
							num++;
							newRebarGroups.Add(new ArrayList());
						}
					}
					previusDistanceRebars = 0.0;
				}
				if (i == parallelRebars.Count - 2)
				{
					ArrayList arrayList2 = (ArrayList)newRebarGroups[num];
					arrayList2.Add(singleRebar2);
				}
			}
		}

		private ArrayList SplitInDifferentGroupsIfNeeded(ArrayList parallelRebars)
		{
			ArrayList arrayList = new ArrayList
			{
				new ArrayList()
			};
			if (parallelRebars.Count > 2)
			{
				SplitGroupsWithTwoRebarsOrMore(parallelRebars, arrayList);
			}
			else if (parallelRebars.Count == 2)
			{
				SingleRebar value = parallelRebars[0] as SingleRebar;
				SingleRebar value2 = parallelRebars[1] as SingleRebar;
				ArrayList arrayList2 = (ArrayList)arrayList[0];
				arrayList2.Add(value);
				arrayList2.Add(value2);
			}
			else
			{
				arrayList = null;
			}
			return arrayList;
		}

		private bool SplitPrimaryRebarGroup(RebarGroup primaryRebarGroup, ArrayList primaryGroupGeometries, ArrayList secondaryGroupGeometries, ref ArrayList splitRebarGroups, ref ArrayList splitUngroupedRebars)
		{
			bool result = false;
			if (GetParallelRebarsToCreateNewGroups(primaryRebarGroup, primaryGroupGeometries, secondaryGroupGeometries, out var newParallelRebars, out var remainingParallelRebars))
			{
				GroupType type = GroupType.Undefined;
				if (primaryRebarGroup.Polygons.Count == 1)
				{
					type = GroupType.Normal;
				}
				else if (primaryRebarGroup.Polygons.Count == 2)
				{
					type = GroupType.Tapered;
				}
				result = AddParallelRebarsToGroup(newParallelRebars, type, ref splitRebarGroups, ref splitUngroupedRebars);
				AddParallelRebarsToGroup(remainingParallelRebars, type, ref splitRebarGroups, ref splitUngroupedRebars);
			}
			return result;
		}

		private bool SplitPrimaryRebarGroupByRebarGroups(ArrayList newRebarGroups, RebarGroup primaryRebarGroup, int primaryGroupIndex, ArrayList primaryGroupGeometries, ref ArrayList splitRebarGroups, ref ArrayList splitUngroupedRebars)
		{
			bool flag = false;
			for (int i = 0; i < newRebarGroups.Count; i++)
			{
				if (flag)
				{
					break;
				}
				RebarGroup rebarGroup = newRebarGroups[i] as RebarGroup;
				if (rebarGroup != null)
				{
					rebarGroup.Insert();
					ArrayList rebarGeometries = rebarGroup.GetRebarGeometries(withHooks: false);
					rebarGroup.Delete();
					if (primaryGroupIndex != i && PrimaryRebarGroupShouldBeSplit(primaryGroupGeometries, rebarGeometries))
					{
						flag = SplitPrimaryRebarGroup(primaryRebarGroup, primaryGroupGeometries, rebarGeometries, ref splitRebarGroups, ref splitUngroupedRebars);
					}
				}
			}
			return flag;
		}

		private bool SplitPrimaryRebarGroupBySingleRebar(ArrayList newUngroupedRebars, RebarGroup primaryRebarGroup, ArrayList primaryGroupGeometries, ref ArrayList splitRebarGroups, ref ArrayList splitUngroupedRebars)
		{
			bool result = false;
			bool flag = false;
			for (int i = 0; i < newUngroupedRebars.Count; i++)
			{
				if (flag)
				{
					break;
				}
				SingleRebar singleRebar = newUngroupedRebars[i] as SingleRebar;
				if (singleRebar == null || !PrimaryRebarGroupShouldBeSplit(primaryRebarGroup, primaryGroupGeometries, singleRebar, out var _, out var remainingParallelRebars))
				{
					continue;
				}
				singleRebar.Insert();
				ArrayList rebarGeometries = singleRebar.GetRebarGeometries(withHooks: false);
				singleRebar.Delete();
				result = SplitPrimaryRebarGroup(primaryRebarGroup, primaryGroupGeometries, rebarGeometries, ref splitRebarGroups, ref splitUngroupedRebars);
				if ((primaryRebarGroup.Polygons.Count != 2 || remainingParallelRebars.Count != 2) && remainingParallelRebars.Count != 1)
				{
					continue;
				}
				foreach (SingleRebar item in remainingParallelRebars)
				{
					if (!RebarArrayListContainsSingleRebar(item, splitUngroupedRebars))
					{
						splitUngroupedRebars.Add(item);
					}
				}
				flag = true;
			}
			return result;
		}

		private bool SplitRebarGroups(ref ArrayList newRebarGroups, ref ArrayList newUngroupedRebars)
		{
			bool result = false;
			ArrayList splitRebarGroups = new ArrayList();
			ArrayList splitUngroupedRebars = new ArrayList();
			for (int i = 0; i < newRebarGroups.Count; i++)
			{
				RebarGroup rebarGroup = newRebarGroups[i] as RebarGroup;
				if (rebarGroup != null)
				{
					rebarGroup.Insert();
					ArrayList rebarGeometries = rebarGroup.GetRebarGeometries(withHooks: false);
					rebarGroup.Delete();
					bool flag = SplitPrimaryRebarGroupByRebarGroups(newRebarGroups, rebarGroup, i, rebarGeometries, ref splitRebarGroups, ref splitUngroupedRebars);
					if (!flag)
					{
						flag = SplitPrimaryRebarGroupBySingleRebar(newUngroupedRebars, rebarGroup, rebarGeometries, ref splitRebarGroups, ref splitUngroupedRebars);
					}
					if (flag)
					{
						result = true;
					}
					else
					{
						splitRebarGroups.Add(rebarGroup);
					}
				}
			}
			newRebarGroups = splitRebarGroups;
			foreach (SingleRebar item in splitUngroupedRebars)
			{
				if (item != null && !RebarArrayListContainsSingleRebar(item, newUngroupedRebars))
				{
					if (RebarHooksShouldBeTurned(item))
					{
						ArrayList rebarPoints = new ArrayList
						{
							item.Polygon.Points[1],
							item.Polygon.Points[0]
						};
						CreateSingleRebar createSingleRebar = new CreateSingleRebar(item);
						SingleRebar newRebar = createSingleRebar.GetSingleRebar(rebarPoints);
						SwapStartHookWithEndHook(ref newRebar);
						newUngroupedRebars.Add(newRebar);
					}
					else
					{
						newUngroupedRebars.Add(item);
					}
				}
			}
			return result;
		}

		private bool SplitRebarGroupsForSplicing()
		{
			bool result = false;
			ArrayList newRebarGroups = new ArrayList(rebarGroups);
			ArrayList newUngroupedRebars = new ArrayList(ungroupedRebars);
			if (rebarGroups.Count > 1)
			{
				bool flag = true;
				for (int i = 0; (double)i < 10.0 && flag; i++)
				{
					flag = SplitRebarGroups(ref newRebarGroups, ref newUngroupedRebars);
				}
				rebarGroups = newRebarGroups;
				ungroupedRebars = newUngroupedRebars;
				result = true;
			}
			return result;
		}
	}
}
