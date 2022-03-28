using System.Collections;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace Tekla.Structures.Concrete
{
	public class SpliceRebarsInPart
	{
		private const double AngleEpsilon = 0.01;

		private const double DistanceEpsilon = 0.1;

		private SpliceData spliceData;

		public SpliceRebarsInPart()
		{
		}

		public SpliceRebarsInPart(SpliceData newSpliceData)
		{
			spliceData = newSpliceData;
		}

		public bool SpliceRebarsIfNeeded(SpliceData newSpliceData, ArrayList rebarGroups, ArrayList ungroupedRebars, out ArrayList splices)
		{
			bool result = false;
			splices = new ArrayList();
			if (newSpliceData != null)
			{
				spliceData = newSpliceData;
			}
			if (spliceData != null)
			{
				result = SpliceRebarsIfNeeded(rebarGroups, ungroupedRebars, out splices);
			}
			return result;
		}

		public bool SpliceRebarsIfNeeded(ArrayList rebarGroups, ArrayList ungroupedRebars, out ArrayList splices)
		{
			bool result = false;
			splices = new ArrayList();
			SpliceRebarGroups(rebarGroups, ref splices);
			SpliceUngroupedRebars(ungroupedRebars, ref splices);
			if (splices.Count > 0)
			{
				result = true;
			}
			return result;
		}

		private void CreateSplices(Reinforcement primary, Reinforcement secondary, double size, ref ArrayList splices)
		{
			RebarSplice rebarSplice = new RebarSplice
			{
				RebarGroup1 = primary,
				RebarGroup2 = secondary
			};
			switch (spliceData.SpliceType)
			{
			case 0:
				rebarSplice.Type = RebarSplice.RebarSpliceTypeEnum.SPLICE_TYPE_LAP_BOTH;
				if (!double.IsNaN(spliceData.LappingLength))
				{
					rebarSplice.LapLength = spliceData.LappingLength;
				}
				else
				{
					rebarSplice.LapLength = spliceData.LappingLengthFactor * size;
				}
				rebarSplice.BarPositions = spliceData.BarPositions;
				rebarSplice.Clearance = 0.0;
				break;
			case 1:
				rebarSplice.Type = RebarSplice.RebarSpliceTypeEnum.SPLICE_TYPE_MUFF;
				break;
			case 2:
				rebarSplice.Type = RebarSplice.RebarSpliceTypeEnum.SPLICE_TYPE_WELD;
				break;
			default:
				rebarSplice.Type = RebarSplice.RebarSpliceTypeEnum.SPLICE_TYPE_LAP_BOTH;
				if (!double.IsNaN(spliceData.LappingLength))
				{
					rebarSplice.LapLength = spliceData.LappingLength;
				}
				else
				{
					rebarSplice.LapLength = spliceData.LappingLengthFactor * size;
				}
				rebarSplice.BarPositions = spliceData.BarPositions;
				rebarSplice.Clearance = 0.0;
				break;
			}
			rebarSplice.Offset = 0.0;
			splices.Add(rebarSplice);
		}

		private bool GetGroupLines(RebarGroup rebarGroup, out ArrayList groupSegments)
		{
			bool result = false;
			groupSegments = new ArrayList();
			ArrayList rebarGeometries = rebarGroup.GetRebarGeometries(withHooks: false);
			if (rebarGeometries.Count > 1)
			{
				RebarGeometry rebarGeometry = rebarGeometries[0] as RebarGeometry;
				RebarGeometry rebarGeometry2 = rebarGeometries[rebarGeometries.Count - 1] as RebarGeometry;
				if (rebarGeometry != null && rebarGeometry2 != null)
				{
					ArrayList points = rebarGeometry.Shape.Points;
					ArrayList points2 = rebarGeometry2.Shape.Points;
					groupSegments.Add(new LineSegment((Point)points[0], (Point)points2[0]));
					groupSegments.Add(new LineSegment((Point)points[1], (Point)points2[1]));
					result = true;
				}
			}
			return result;
		}

		private double GetNominalDiameter(Reinforcement rebar)
		{
			double result = 0.0;
			ArrayList arrayList = new ArrayList();
			RebarGroup rebarGroup = rebar as RebarGroup;
			if (rebarGroup != null)
			{
				arrayList = rebarGroup.GetRebarGeometries(withHooks: false);
			}
			SingleRebar singleRebar = rebar as SingleRebar;
			if (singleRebar != null)
			{
				arrayList = singleRebar.GetRebarGeometries(withHooks: false);
			}
			if (arrayList.Count > 0)
			{
				RebarGeometry rebarGeometry = arrayList[0] as RebarGeometry;
				if (rebarGeometry != null)
				{
					result = rebarGeometry.Diameter;
				}
			}
			return result;
		}

		private Vector GetRebarVectorForGroup(int segmentIndex, ArrayList groupSegments)
		{
			Vector result = new Vector();
			LineSegment lineSegment = groupSegments[0] as LineSegment;
			LineSegment lineSegment2 = groupSegments[1] as LineSegment;
			if (lineSegment != null && lineSegment2 != null)
			{
				result = ((segmentIndex == 0) ? new Vector(lineSegment2.Point1 - lineSegment.Point1) : new Vector(lineSegment.Point1 - lineSegment2.Point1));
			}
			return result;
		}

		private Vector GetRebarVectorForSingleRebar(int pointIndex, SingleRebar rebar)
		{
			return (pointIndex == 0) ? new Vector((Point)rebar.Polygon.Points[1] - (Point)rebar.Polygon.Points[0]) : new Vector((Point)rebar.Polygon.Points[0] - (Point)rebar.Polygon.Points[1]);
		}

		private bool RebarGroupsCanBeSpliced(RebarGroup primaryRebarGroup, RebarGroup secondaryRebarGroup)
		{
			bool flag = false;
			GetGroupLines(primaryRebarGroup, out var groupSegments);
			GetGroupLines(secondaryRebarGroup, out var groupSegments2);
			for (int i = 0; i < groupSegments.Count; i++)
			{
				if (flag)
				{
					break;
				}
				LineSegment lineSegment = groupSegments[i] as LineSegment;
				for (int j = 0; j < groupSegments2.Count; j++)
				{
					if (flag)
					{
						break;
					}
					LineSegment lineSegment2 = groupSegments2[j] as LineSegment;
					if (lineSegment != null && lineSegment2 != null && Parallel.LineSegmentToLineSegment(lineSegment, lineSegment2, 0.01) && Distance.PointToPoint(lineSegment.Point1, lineSegment2.Point1) < 0.1 && Distance.PointToPoint(lineSegment.Point2, lineSegment2.Point2) < 0.1)
					{
						Vector rebarVectorForGroup = GetRebarVectorForGroup(i, groupSegments);
						Vector rebarVectorForGroup2 = GetRebarVectorForGroup(j, groupSegments2);
						flag = RebarsDoNotOverlap(rebarVectorForGroup, rebarVectorForGroup2);
					}
				}
			}
			return flag;
		}

		private bool RebarsCanBeSpliced(SingleRebar primaryRebar, SingleRebar secondaryRebar)
		{
			bool flag = false;
			for (int i = 0; i < primaryRebar.Polygon.Points.Count; i++)
			{
				if (flag)
				{
					break;
				}
				for (int j = 0; j < secondaryRebar.Polygon.Points.Count; j++)
				{
					if (flag)
					{
						break;
					}
					if (Distance.PointToPoint(primaryRebar.Polygon.Points[i] as Point, secondaryRebar.Polygon.Points[j] as Point) < 0.1)
					{
						Vector rebarVectorForSingleRebar = GetRebarVectorForSingleRebar(i, primaryRebar);
						Vector rebarVectorForSingleRebar2 = GetRebarVectorForSingleRebar(j, secondaryRebar);
						flag = RebarsDoNotOverlap(rebarVectorForSingleRebar, rebarVectorForSingleRebar2);
					}
				}
			}
			return flag;
		}

		private bool RebarsDoNotOverlap(Vector primaryVector, Vector secondaryVector)
		{
			return primaryVector.Dot(secondaryVector) < 0.0;
		}

		private void SpliceRebarGroups(ArrayList rebarGroups, ref ArrayList splices)
		{
			if (rebarGroups.Count <= 1)
			{
				return;
			}
			for (int i = 0; i < rebarGroups.Count; i++)
			{
				RebarGroup rebarGroup = rebarGroups[i] as RebarGroup;
				for (int j = i + 1; j < rebarGroups.Count; j++)
				{
					RebarGroup rebarGroup2 = rebarGroups[j] as RebarGroup;
					if (RebarGroupsCanBeSpliced(rebarGroup, rebarGroup2) && rebarGroup != null)
					{
						CreateSplices(rebarGroup, rebarGroup2, GetNominalDiameter(rebarGroup), ref splices);
					}
				}
			}
		}

		private void SpliceUngroupedRebars(ArrayList ungroupedRebars, ref ArrayList splices)
		{
			if (ungroupedRebars.Count <= 1)
			{
				return;
			}
			for (int i = 0; i < ungroupedRebars.Count; i++)
			{
				SingleRebar singleRebar = ungroupedRebars[i] as SingleRebar;
				for (int j = i + 1; j < ungroupedRebars.Count; j++)
				{
					SingleRebar singleRebar2 = ungroupedRebars[j] as SingleRebar;
					if (RebarsCanBeSpliced(singleRebar, singleRebar2) && singleRebar != null)
					{
						CreateSplices(singleRebar, singleRebar2, GetNominalDiameter(singleRebar), ref splices);
					}
				}
			}
		}
	}
}
