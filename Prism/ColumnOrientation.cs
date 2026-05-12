using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Solid;
using static Tekla.Structures.Filtering.Categories.PourObjectFilterExpressions;

namespace Prism
{
	public static class ColumnOrientation
	{
		private static double CoordinateTolerance = 0.001;
		private static double offsetPerpendicularToEdgeofColumnFlange = 50;
		private static double endDistance = 100;

		public static void DetailColumnOrientationHoles(SelectedObjects myObjects, string columnOrientationType, string flangeThickness)
		{
			double tolerance = 0.5;

			if (columnOrientationType != "None")
			{
				List<Beam> columns = new List<Beam>();

				foreach (PrismPart prismPart in myObjects.PrismParts)
				{
					Beam b = prismPart.Part as Beam;
					if (b != null)
					{
						if (IsColumnOkForOrientationHoles(b, tolerance))
						{
							columns.Add(b);
						}
					}
				}

				foreach (Beam column in columns)
				{
					GetMemberProperties(column, out double flangeWidth, out double columnDepth, out double tWeb, out double flangeThick, out double rootRadius);

					ArrayList columnVertices = GetMyColumnVertices(column);

					Point[] vertexPoint = new Point[12];        // get the vertices in an indexed array
					double minimumX = GetMinAndMaxX(columnVertices, vertexPoint, out double maximumX);

					Point[] vertexPointsAtMax_X = VertexPointsAtMaxX(minimumX, vertexPoint);

					int soPointIndex = GetSetOutPointIndex(vertexPointsAtMax_X, vertexPoint, out Point soPoint);    // index of the soPoint in the vertexPoint array

					Vector flangeVector = new Vector();

					if (soPointIndex == 0)          // "start" vertexPoint, so look at 1 & 11
					{
						flangeVector = GetFlangeVector(1, 0, 11, vertexPoint);
					}
					else if (soPointIndex == 11)    // "end" vertexPoint,   so look at 0 & 10
					{
						flangeVector = GetFlangeVector(0, 11, 10, vertexPoint);
					}
					else     // "intermediate" vertexPoint
					{
						flangeVector = GetFlangeVector(soPointIndex - 1, soPointIndex, soPointIndex + 1, vertexPoint);
					}

					if (columnOrientationType == "Holes only")
					{
						SetOutOrientationHole(column, soPoint, flangeVector, flangeWidth, flangeThick, tWeb, rootRadius);
					}
					if (columnOrientationType == "Plate only")
					{
						DrawOrientationPlate(column, soPoint, flangeVector);
					}
					if (columnOrientationType == "Holes and plate")
					{
						if (flangeThick <= Convert.ToInt32(flangeThickness))
						{
							SetOutOrientationHole(column, soPoint, flangeVector, flangeWidth, flangeThick, tWeb, rootRadius);
						}
						if (flangeThick > Convert.ToInt32(flangeThickness))
						{
							DrawOrientationPlate(column, soPoint, flangeVector);
						}
					}
				}
			}
		}

		private static bool IsColumnOkForOrientationHoles(Beam b, double tolerance)
		{
			var profile = b.Profile.ProfileString;

			string[] excludedProfiles = { "PFC", "RSA", "SHS", "CHS", "RHS" };

			bool isStandardColumnProfile = !excludedProfiles.Any(profile.Contains);

			bool isVertical =
				Math.Abs(b.StartPoint.X - b.EndPoint.X) <= tolerance &&
				Math.Abs(b.StartPoint.Y - b.EndPoint.Y) <= tolerance;

			bool isColumn = b.Name == "COLUMN" && isStandardColumnProfile;
			bool isPGVertical = profile.Contains("PG") && isVertical;

			return isColumn || isPGVertical;
		}

		private static void DrawOrientationPlate(Beam column, Point soPoint, Vector flangeVector)
		{
			double deltaX1 = 20 * flangeVector.X;
			double deltaY1 = 20 * flangeVector.Y;
			double deltaX2 = 40 * (flangeVector.X * 2);
			double deltaY2 = 40 * (flangeVector.Y * 2);

			ContourPoint point1 = new ContourPoint(new Point(soPoint.X + deltaX1, soPoint.Y + deltaY1, soPoint.Z + endDistance), null);
			ContourPoint point2 = new ContourPoint(new Point(soPoint.X + deltaX1, soPoint.Y + deltaY1, soPoint.Z + 2 * endDistance), null);
			ContourPoint point3 = new ContourPoint(new Point(soPoint.X + deltaX2, soPoint.Y + deltaY2, soPoint.Z + endDistance), null);
			ContourPoint point4 = new ContourPoint(new Point(soPoint.X + deltaX2, soPoint.Y + deltaY2, soPoint.Z + 2 * endDistance), null);

			ContourPlate cPlate = new ContourPlate();
			Contour c = new Contour();
			c.ContourPoints = new ArrayList { point1, point3, point4, point2 };
			cPlate.Contour = c;
			cPlate.Profile.ProfileString = "PLT8";
			if (point1.Y >= point3.Y && point1.X <= point3.X)
			{
				cPlate.Position.Depth = Position.DepthEnum.FRONT;
			}
			else
			{
				cPlate.Position.Depth = Position.DepthEnum.BEHIND;
			}

			cPlate.Insert();

			WeldPlateToColumn(column, cPlate);
		}

		private static void WeldPlateToColumn(Beam column, ContourPlate cPlate)
		{
			Weld myWeld = new Weld();
			myWeld.MainObject = column;
			myWeld.SecondaryObject = cPlate;
			myWeld.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_BEVEL_BACKING;
			myWeld.ShopWeld = true;
			myWeld.AroundWeld = true;
			myWeld.SizeAbove = 0;
			myWeld.ReferenceText = "P15";
			myWeld.Insert();
		}

		private static void GetMemberProperties(Beam column, out double flangeWidth, out double columnDepth, out double tWeb, out double flangeThick, out double rootRadius)
		{
			flangeWidth = 0;
			columnDepth = 0;
			tWeb = 0;
			flangeThick = 0;
			rootRadius = 0;
			column.GetReportProperty("FLANGE_WIDTH_U", ref flangeWidth);
			column.GetReportProperty("HEIGHT", ref columnDepth);
			column.GetReportProperty("WEB_THICKNESS", ref tWeb);
			column.GetReportProperty("FLANGE_THICKNESS_U", ref flangeThick);

			if ((column.Profile.ProfileString.Contains("UB") || column.Profile.ProfileString.Contains("UC")))
			{
				column.GetReportProperty("PROFILE.ROUNDING_RADIUS_1", ref rootRadius);
			}
			else //its probably a plate girder so assume 10mm 
			{
				rootRadius = 10.0;
			}
		}

		private static ArrayList GetMyColumnVertices(Beam column)
		{
			Solid columnSolid = column.GetSolid();
			Point minimumPoint = columnSolid.MinimumPoint as Point;
			// a holding TSG3d.Point() for aVertex
			Point theMinimumVertexPoint = new Point();
			ArrayList columnVertices = new ArrayList();
			// Get an enumerator for the FACES
			FaceEnumerator columnFaceEnumerator = columnSolid.GetFaceEnumerator();  // gets 12+2 faces
			bool test = false;
			while (columnFaceEnumerator.MoveNext())
			{
				// Get the individual FACE
				Face columnFace = columnFaceEnumerator.Current as Face;

				// Get an enumerator to LOOP through the column FACES
				LoopEnumerator columnLoopEnumerator = columnFace.GetLoopEnumerator();

				while (columnLoopEnumerator.MoveNext())
				{
					// Get the LOOP
					Loop columnLoop = columnLoopEnumerator.Current as Loop;

					// Get an enumerator for the face vertices
					VertexEnumerator columnVertexEnumerator = columnLoop.GetVertexEnumerator() as VertexEnumerator;

					while (columnVertexEnumerator.MoveNext())
					{
						// Get a Point for each vertex
						// Add to an ArrayList
						if (columnVertexEnumerator.Current is Point aVertex && test == false)
						{
							columnVertices.Add(aVertex);
							if (columnVertices.Count == 1)
							{
								theMinimumVertexPoint = aVertex;
							}
							else if (aVertex.Z < theMinimumVertexPoint.Z)
							{
								theMinimumVertexPoint = aVertex;
							}
						}
					}

					// Check to see if the columnVertices ArrayList has 12 vertices ie considering an end face
					// and if it has, check to see if the Z-coordinate = minimumPoint.Z
					// if FALSE, carry on; if TRUE, break out
					//
					if ((columnVertices.Count > 10) && (Math.Abs(theMinimumVertexPoint.Z - minimumPoint.Z) <= CoordinateTolerance))
					{
						// Getting here means that the lower end of the column has been found
						// and all the coordinates of the vertices of the I-section are in columnVertices
						test = true;
						break;
					}
					else
					{
						columnVertices = null;
						columnVertices = new ArrayList();
					}
				}
			}
			return columnVertices;
		}

		private static double GetMinAndMaxX(ArrayList columnVertices, Point[] vertexPoint, out double maximumX)
		{
			int vertexCount = -1;           // a vertex counter for the index
			int max_XCount = -1;            // no. of vertices with the same .X
			maximumX = 0.0;                 // max .X of the I-section's vertices
			int max_YIndex = 0;             // index of max .Y in vertexPointsAtMax_X array 
			int soPointIndex = 0;           // index of the soPoint in the vertexPoint array
			double minimumX = 0.0;

			foreach (Point xyzPoint in columnVertices)
			{
				vertexCount++;
				vertexPoint[vertexCount] = xyzPoint;                // add to the vertexPoint array   

				if (vertexCount == 0)
				{
					maximumX = vertexPoint[vertexCount].X;
				}
				else
				{
					if ((vertexPoint[vertexCount].X - maximumX) >= 0)
					{
						maximumX = vertexPoint[vertexCount].X;

					}
				}

				if (vertexCount == 0)
				{
					minimumX = vertexPoint[vertexCount].X;
				}
				else
				{
					if ((vertexPoint[vertexCount].X - minimumX) <= 0)
					{
						minimumX = vertexPoint[vertexCount].X;
					}
				}
			}
			return minimumX;
		}

		private static Point[] VertexPointsAtMaxX(double minimumX, Point[] vertexPoint)
		{
			Point[] vertexPointsAtMax_X = new Point[1];
			int vertexCount = -1;           // a vertex counter for the index
			int max_XCount = -1;            // no. of vertices with the same .X
			///double maximumX = 0.0;         // max .X of the I-section's vertices
			int max_YIndex = 0;             // index of max .Y in vertexPointsAtMax_X array 

			for (int i = 0; i < vertexPoint.Length; i++)
			{
				if (vertexPoint[i].X == minimumX)
				{
					max_XCount++;

					//
					// Resize the array if necessary
					//
					if (max_XCount > 0)
					{
						Array.Resize(ref vertexPointsAtMax_X, max_XCount + 1);
					}

					vertexPointsAtMax_X[max_XCount] = vertexPoint[i];
				}
			}
			return vertexPointsAtMax_X;
		}

		private static int GetSetOutPointIndex(Point[] vertexPointsAtMax_X, Point[] vertexPoint, out Point soPoint)
		{
			soPoint = new Point();
			int max_YIndex = 0;
			int soPointIndex = 0;
			//
			// Now, get the maximum .Y index if the length of the vertexPointsAtMAx_X > 1
			//
			if (vertexPointsAtMax_X.Length == 1)
			{
				max_YIndex = 0;
				soPoint = vertexPointsAtMax_X[max_YIndex];
			}
			else
			{

				for (int i = 0; i < vertexPointsAtMax_X.Length; i++)
				{
					if (i == 0)
					{
						max_YIndex = i;
						soPoint = vertexPointsAtMax_X[i];
					}
					else
					{
						if (vertexPointsAtMax_X[i].Y < soPoint.Y)
						{
							max_YIndex = i;
							soPoint = vertexPointsAtMax_X[i];
						}
					}
				}
			}

			//
			// Now, having found the soPoint, loop through the vertexPoint array and find the index of
			// this array which corresponds with the soPoint
			//
			for (int i = 0; i < vertexPoint.Length; i++)
			{
				if (soPoint == vertexPoint[i])
				{
					soPointIndex = i;
					break;
				}
			}
			return soPointIndex;
		}

		private static void SetOutOrientationHole(Beam column, Point soPoint, Vector flangeVector, double flangeWidth, double flangeThick, double tWeb, double rootRadius)
		{
			// Use the vector to get the setting out point for the orientation hole

			double boltCutLength = flangeThick > 30 ? 200 : 130;

			BoltArray b = new BoltArray()
			{
				Tolerance = 2.0
			};

			// Loop through BoltSize(s)... 20, 16, 12 depending on whether inner
			// edge of hole hits rootRadius (Weld)

			b.BoltSize = GetBoltSize(flangeWidth, tWeb, rootRadius, b.Tolerance);

			// Get the set-out point of the orientation bolt from the correct vertex

			offsetPerpendicularToEdgeofColumnFlange = 1.4 * (b.BoltSize + b.Tolerance);

			b.BoltStandard = "8.8XOX";
			b.PartToBeBolted = column;

			// offsets from correct vertex to B.FirstPOsition and B.SecondPosition, etc

			double deltaX = offsetPerpendicularToEdgeofColumnFlange * flangeVector.X;
			double deltaY = offsetPerpendicularToEdgeofColumnFlange * flangeVector.Y;

			b.FirstPosition = new Point(soPoint.X + deltaX, soPoint.Y + deltaY, soPoint.Z + endDistance);
			b.SecondPosition = new Point(soPoint.X + deltaX, soPoint.Y + deltaY, soPoint.Z + 2 * endDistance);

			b.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
			b.CutLength = boltCutLength;

			b.ExtraLength = 0;
			b.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;

			b.Position.Plane = Position.PlaneEnum.MIDDLE;
			b.Position.PlaneOffset = flangeThick / -2;
			b.Position.Rotation = Position.RotationEnum.BELOW;
			b.Position.Depth = Position.DepthEnum.MIDDLE;

			// Need to rotate the bolt axis perpendicular to the flange

			b.Position.RotationOffset = Math.Atan(flangeVector.Y / flangeVector.X) * 180 / Math.PI;

			b.Bolt = false;
			b.Washer1 = false;
			b.Washer2 = false;
			b.Washer3 = false;
			b.Nut1 = false;
			b.Nut2 = false;

			b.Hole1 = true;
			b.Hole2 = false;
			b.Hole3 = false;
			b.Hole4 = false;
			b.Hole5 = false;

			b.HoleType = BoltGroup.BoltHoleTypeEnum.HOLE_TYPE_SLOTTED;

			b.AddBoltDistX(0);
			b.AddBoltDistY(0);

			// Insert the BoltArray, B.

			if (!b.Insert())
			{
				Console.WriteLine("Bolt Array Insert Failed!");
			}
			else
			{
				b.SetUserProperty("BOLT_COMMENT", "Orientation Hole");
			}
		}

		private static Vector GetFlangeVector(int i_minus_1, int i, int i_plus_1, Point[] vertArray)
		{
			Vector theVector = new Vector();

			//double dx1 = vertArray[i].X - vertArray[i_minus_1].X;
			//double dy1 = vertArray[i].Y - vertArray[i_minus_1].Y;
			double dx1 = vertArray[i_minus_1].X - vertArray[i].X;
			double dy1 = vertArray[i_minus_1].Y - vertArray[i].Y;
			double dz1 = 0;

			//double dx2 = vertArray[i].X - vertArray[i_plus_1].X;
			//double dy2 = vertArray[i].Y - vertArray[i_plus_1].Y;
			double dx2 = vertArray[i_plus_1].X - vertArray[i].X;
			double dy2 = vertArray[i_plus_1].Y - vertArray[i].Y;
			double dz2 = 0;


			double L1 = 0.0;        // based on i_minus_1, in the GLOBAL X-Y plane
			double L2 = 0.0;        // based on i_plus_1, in the GLOBAL X-Y plane

			L1 = Math.Sqrt(Math.Pow(dx1, 2) + Math.Pow(dy1, 2) + Math.Pow(dz1, 2));
			L2 = Math.Sqrt(Math.Pow(dx2, 2) + Math.Pow(dy2, 2) + Math.Pow(dz2, 2));

			if (L1 > L2)
			{
				theVector.X = dx1 / L1;
				theVector.Y = dy1 / L1;
				theVector.Z = dz1 / L1;
			}
			else
			{
				theVector.X = dx2 / L2;
				theVector.Y = dy2 / L2;
				theVector.Z = dz2 / L2;
			}

			return theVector;

		}

		private static double GetBoltSize(double B, double t, double r, double tolerance)
		{
			double boltDiameter = 0.0;

			double[] boltSize = new double[3] { 20, 16, 12 };

			double distanceToRootRadius = 0.0;
			double distanceToInnerEdgeOfHole = 0.0;

			for (int i = 0; i < boltSize.Length; i++)
			{
				distanceToRootRadius = B / 2 - t / 2 - r;
				distanceToInnerEdgeOfHole = 1.4 * (boltSize[i] + tolerance) + 0.5 * (boltSize[i] + tolerance);

				if (distanceToInnerEdgeOfHole <= distanceToRootRadius)
				{
					boltDiameter = boltSize[i];
					break;
				}
			}
			return boltDiameter;
		}
	}
}