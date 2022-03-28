#define TRACE
using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using Tekla.Structures.Dialog;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace Tekla.Structures.Concrete
{
	public class ClassificatorCalculator
	{
		public const string UDA = "Hierarchy";

		public const string UDA2 = "Depth";

		public const string Back = "Back-";

		public const string Bot = "Bot-";

		public const string Front = "Front-";

		public const string Top = "Top-";

		public readonly string PrefixBack;

		public readonly string PrefixBot;

		public readonly string PrefixFront;

		public readonly string PrefixTop;

		private const int Deeper = 2;

		private const double Epsilon = 0.0001;

		private const int Equal = 1;

		private const int Upper = 0;

		private readonly Localization localization;

		public ClassificatorCalculator(Localization localize)
		{
			localization = localize;
			PrefixTop = "Top-";
			PrefixBot = "Bot-";
			PrefixBack = "Back-";
			PrefixFront = "Front-";
		}

		public ClassificatorCalculator(string top, string bot, string back, string front, Localization localize)
		{
			localization = localize;
			PrefixTop = top;
			PrefixBot = bot;
			PrefixBack = back;
			PrefixFront = front;
		}

		public void ClassifyRebar(ModelObject singleObject, ref ArrayList classifiedRebars)
		{
			Reinforcement reinforcement = singleObject as Reinforcement;
			if (reinforcement != null && !ArrayContainsRebar(reinforcement, classifiedRebars))
			{
				string value = SetRebarsLevel(reinforcement);
				reinforcement.SetUserProperty("Hierarchy", value);
				classifiedRebars.Add(reinforcement);
			}
		}

		public void ClassifyRebars(bool allRebars, ref ArrayList rebars, ref ProgressBar progress)
		{
			Tekla.Structures.Model.Model model = new Tekla.Structures.Model.Model();
			ModelObjectEnumerator modelObjectEnumerator = (allRebars ? model.GetModelObjectSelector().GetAllObjects() : new Tekla.Structures.Model.UI.ModelObjectSelector().GetSelectedObjects());
			progress.Maximum = modelObjectEnumerator.GetSize();
			int num = 0;
			while (modelObjectEnumerator.MoveNext())
			{
				try
				{
					rebars = HandleNextObject(allRebars, progress, modelObjectEnumerator.Current, rebars);
					progress.Value++;
				}
				catch (ApplicationException)
				{
					num++;
				}
			}
			if (num > 0)
			{
				ShowMessage("albl_Corrupt_Object_Selection");
			}
			SortRebarsLevels(ref rebars, ref progress);
		}

		public string SetRebarsLevel(Reinforcement rebar)
		{
			string result = string.Empty;
			ArrayList rebarGeometries = rebar.GetRebarGeometries(withHooks: false);
			Part part = rebar.Father as Part;
			if (part != null)
			{
				Tekla.Structures.Model.Solid solid = part.GetSolid();
				double shift = Math.Abs(solid.MaximumPoint.Z);
				double minDepth = Math.Abs(solid.MinimumPoint.Z);
				if (!PartIsSlab(rebar.Father))
				{
					CoordinateSystem coordinateSystem = part.GetCoordinateSystem();
					Matrix matrix = MatrixFactory.Rotate(Math.PI / 2.0, coordinateSystem.AxisX);
					shift = RotateSlabPoint(solid.MaximumPoint, matrix, coordinateSystem.Origin);
					minDepth = RotateSlabPoint(solid.MinimumPoint, matrix, coordinateSystem.Origin);
					RotateRebarsGeometries(ref rebarGeometries, matrix, coordinateSystem.Origin);
					if (shift > 0.0 && minDepth < 0.0)
					{
						ShiftRebars(ref rebarGeometries, ref shift, ref minDepth);
					}
				}
				else
				{
					ShiftRebars(ref rebarGeometries, ref shift, ref minDepth);
				}
				double num = ((shift > 0.0001) ? shift : minDepth);
				double num2 = Math.Abs(GetRebarsPredominantLevel(rebarGeometries));
				result = (PartIsSlab(rebar.Father) ? ((!(num2 < num / 2.0)) ? (PrefixBot + Math.Floor(num2)) : (PrefixTop + Math.Floor(num2))) : ((!(num2 < num / 2.0)) ? (PrefixBack + Math.Floor(num2)) : (PrefixFront + Math.Floor(num2))));
			}
			return result;
		}

		public void SortRebarsLevels(ref ArrayList classifiedRebars, ref ProgressBar progressBar)
		{
			foreach (Reinforcement classifiedRebar in classifiedRebars)
			{
				int num = 1;
				string value = string.Empty;
				string prevLevel = string.Empty;
				classifiedRebar.GetUserProperty("Hierarchy", ref value);
				foreach (Reinforcement classifiedRebar2 in classifiedRebars)
				{
					string value2 = string.Empty;
					classifiedRebar2.GetUserProperty("Hierarchy", ref value2);
					if (classifiedRebar.Identifier.ID != classifiedRebar2.Identifier.ID && classifiedRebar.Father.Identifier.ID == classifiedRebar2.Father.Identifier.ID && RaiseRebarsIndex(value, value2) && !AreLevelsEqual(value2, prevLevel))
					{
						num++;
						prevLevel = value2;
					}
				}
				classifiedRebar.SetUserProperty("Hierarchy", AssembleRebarsLevel(value, num));
				if (progressBar.Maximum > progressBar.Value)
				{
					progressBar.Value++;
				}
			}
			ReviseClassifiedRebars(classifiedRebars);
			if (progressBar.Maximum > progressBar.Value)
			{
				progressBar.Value++;
			}
		}

		internal bool PartIsSlab(ModelObject singlePart)
		{
			bool result = false;
			ContourPlate contourPlate = singlePart as ContourPlate;
			if (contourPlate != null)
			{
				result = true;
			}
			return result;
		}

		private static void AddValueToLevelList(ref ArrayList levelPoints, double currentZ, double lengthBetweenPoints)
		{
			bool flag = false;
			double[] value = new double[2] { currentZ, lengthBetweenPoints };
			for (int i = 0; i < levelPoints.Count; i++)
			{
				double[] array = levelPoints[i] as double[];
				if (array != null && Math.Abs(array[0] - currentZ) < 0.0001)
				{
					array[1] += lengthBetweenPoints;
					flag = true;
				}
			}
			if (!flag)
			{
				levelPoints.Add(value);
			}
		}

		private static double GetLengthBetweenPoints(Point pointA, Point pointB)
		{
			Point point = new Point(pointB.X - pointA.X, pointB.Y - pointA.Y, pointB.Z - pointA.Z);
			return Math.Sqrt(Math.Pow(point.X, 2.0) + Math.Pow(point.Y, 2.0) + Math.Pow(point.Z, 2.0));
		}

		private static double GetRebarsPredominantLevel(ArrayList rebarGeometries)
		{
			ArrayList levelPoints = new ArrayList();
			foreach (RebarGeometry rebarGeometry in rebarGeometries)
			{
				ArrayList points = rebarGeometry.Shape.Points;
				for (int i = 0; i < points.Count - 1; i++)
				{
					Point point = points[i] as Point;
					Point point2 = points[i + 1] as Point;
					if (point != null && point2 != null && Math.Abs(point.Z) - Math.Abs(point2.Z) < 0.0001)
					{
						AddValueToLevelList(ref levelPoints, point.Z, GetLengthBetweenPoints(point, point2));
					}
				}
			}
			return RebarsPredominantLevel(levelPoints);
		}

		private static double RebarsPredominantLevel(ArrayList levelPoints)
		{
			double[] array = new double[2];
			foreach (double[] levelPoint in levelPoints)
			{
				if (Math.Abs(array[1]) < Math.Abs(levelPoint[1]))
				{
					array[0] = levelPoint[0];
					array[1] = levelPoint[1];
				}
			}
			return array[0];
		}

		private bool AreLevelsEqual(string rebarsLevel, string prevLevel)
		{
			bool result = false;
			if (rebarsLevel.Contains(PrefixTop) && prevLevel.Contains(PrefixTop))
			{
				if (CompareLevels(rebarsLevel, prevLevel, PrefixTop) == 1)
				{
					result = true;
				}
			}
			else if (rebarsLevel.Contains(PrefixBot) && prevLevel.Contains(PrefixBot))
			{
				if (CompareLevels(rebarsLevel, prevLevel, PrefixBot) == 1)
				{
					result = true;
				}
			}
			else if (rebarsLevel.Contains(PrefixFront) && prevLevel.Contains(PrefixFront))
			{
				if (CompareLevels(rebarsLevel, prevLevel, PrefixFront) == 1)
				{
					result = true;
				}
			}
			else if (rebarsLevel.Contains(PrefixBack) && prevLevel.Contains(PrefixBack) && CompareLevels(rebarsLevel, prevLevel, PrefixBack) == 1)
			{
				result = true;
			}
			return result;
		}

		private bool ArrayContainsRebar(Reinforcement rebar, ArrayList classifiedRebars)
		{
			bool result = false;
			foreach (Reinforcement classifiedRebar in classifiedRebars)
			{
				if (rebar.Identifier.ID == classifiedRebar.Identifier.ID)
				{
					result = true;
				}
			}
			return result;
		}

		private string AssembleRebarsLevel(string rebarsLevel, int rebarsIndex)
		{
			if (rebarsLevel.Contains(PrefixTop))
			{
				return PrefixTop + rebarsIndex + "*" + rebarsLevel.Substring(PrefixTop.Length);
			}
			if (rebarsLevel.Contains(PrefixBot))
			{
				return PrefixBot + rebarsIndex + "*" + rebarsLevel.Substring(PrefixBot.Length);
			}
			if (rebarsLevel.Contains(PrefixFront))
			{
				return PrefixFront + rebarsIndex + "*" + rebarsLevel.Substring(PrefixFront.Length);
			}
			return PrefixBack + rebarsIndex + "*" + rebarsLevel.Substring(PrefixBack.Length);
		}

		private int CompareLevels(string rebarsLevel, string nextRebarsLevel, string prefix)
		{
			int result = 0;
			int num = ExtractLevel(rebarsLevel, prefix);
			int num2 = ExtractLevel(nextRebarsLevel, prefix);
			if (num == num2)
			{
				result = 1;
			}
			if (num > num2)
			{
				result = 2;
			}
			return result;
		}

		private int ExtractLevel(string rebarsLevel, string prefix)
		{
			return int.Parse(rebarsLevel.Contains("*") ? rebarsLevel.Substring(rebarsLevel.LastIndexOf("*") + 1) : rebarsLevel.Substring(prefix.Length));
		}

		private ArrayList HandleNextObject(bool allRebars, ProgressBar progress, ModelObject nextObj, ArrayList rebars)
		{
			Part part = nextObj as Part;
			if (part != null && (PartIsSlab(nextObj) || PartIsWall(nextObj)))
			{
				ModelObjectEnumerator reinforcements = part.GetReinforcements();
				progress.Maximum += reinforcements.GetSize() * 2 + 1;
				while (reinforcements.MoveNext())
				{
					ClassifyRebar(reinforcements.Current, ref rebars);
					progress.Value++;
				}
			}
			else
			{
				Reinforcement reinforcement = nextObj as Reinforcement;
				if (!allRebars && reinforcement != null)
				{
					ClassifyRebar(nextObj, ref rebars);
				}
			}
			return rebars;
		}

		private bool IsItWall(double width, double height, double length, int count)
		{
			if (height / width > 5.0 && length / width > 5.0)
			{
				return true;
			}
			if (count > 2)
			{
				return false;
			}
			return IsItWall(height, length, width, ++count);
		}

		private bool PartIsWall(ModelObject modelObject)
		{
			bool result = false;
			Part part = modelObject as Part;
			if (part != null)
			{
				double value = 0.0;
				double value2 = 0.0;
				double value3 = 0.0;
				part.GetReportProperty("WIDTH", ref value);
				part.GetReportProperty("HEIGHT", ref value2);
				part.GetReportProperty("LENGTH", ref value3);
				result = IsItWall(value, value2, value3, 0);
			}
			return result;
		}

		private bool RaiseRebarsIndex(string rebarsLevel, string nextRebarsLevel)
		{
			bool result = false;
			if (rebarsLevel.Contains(PrefixTop) && nextRebarsLevel.Contains(PrefixTop))
			{
				if (CompareLevels(rebarsLevel, nextRebarsLevel, PrefixTop) == 2)
				{
					result = true;
				}
			}
			else if (rebarsLevel.Contains(PrefixBot) && nextRebarsLevel.Contains(PrefixBot))
			{
				if (CompareLevels(rebarsLevel, nextRebarsLevel, PrefixBot) == 0)
				{
					result = true;
				}
			}
			else if (rebarsLevel.Contains(PrefixFront) && nextRebarsLevel.Contains(PrefixFront))
			{
				if (CompareLevels(rebarsLevel, nextRebarsLevel, PrefixFront) == 2)
				{
					result = true;
				}
			}
			else if (rebarsLevel.Contains(PrefixBack) && nextRebarsLevel.Contains(PrefixBack) && CompareLevels(rebarsLevel, nextRebarsLevel, PrefixBack) == 0)
			{
				result = true;
			}
			return result;
		}

		private void ReviseClassifiedRebars(ArrayList classifiedRebars)
		{
			try
			{
				foreach (Reinforcement classifiedRebar in classifiedRebars)
				{
					string value = string.Empty;
					classifiedRebar.GetUserProperty("Hierarchy", ref value);
					classifiedRebar.SetUserProperty("Depth", value);
					classifiedRebar.SetUserProperty("Hierarchy", value.Remove(value.LastIndexOf("*")));
				}
			}
			catch (ArgumentOutOfRangeException ex)
			{
				Trace.WriteLine("Exception in ReviseClassifiedRebars:\n " + ex.Message);
			}
		}

		private void RotateRebarsGeometries(ref ArrayList rebarGeometries, Matrix rotateMatrix, Point origin)
		{
			foreach (RebarGeometry rebarGeometry in rebarGeometries)
			{
				ArrayList points = rebarGeometry.Shape.Points;
				for (int i = 0; i < points.Count; i++)
				{
					Point p = points[i] as Point;
					p -= origin;
					points[i] = rotateMatrix.Transform(p);
				}
			}
		}

		private double RotateSlabPoint(Point slabsPoint, Matrix rotMat, Point origin)
		{
			Point p = slabsPoint - origin;
			p = rotMat.Transform(p);
			return p.Z;
		}

		private void ShiftRebars(ref ArrayList rebarGeometries, ref double shift, ref double minDepth)
		{
			if (shift > 0.0001)
			{
				Vector origin = new Vector(0.0, 0.0, shift);
				Matrix rotateMatrix = new Matrix();
				RotateRebarsGeometries(ref rebarGeometries, rotateMatrix, origin);
				shift -= minDepth;
				minDepth = 0.0;
			}
		}

		private void ShowMessage(string message)
		{
			Trace.WriteLine("Exception: " + localization.GetText(message));
			MessageBox.Show(localization.GetText(message), localization.GetText("albl_Rebar_Classificator"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}
	}
}
