using System;
using System.Collections;
using System.Globalization;
using System.IO;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace Tekla.Structures.Concrete
{
	public class PatternInfo
	{
		private const string BeamTag = "PATTERN_TYPE";

		private const string BottomClassTag = "BOT_CLASS";

		private const string BottomNameTag = "BOT_NAME";

		private const string BottomXyTag = "BOT_XY";

		private const string PatternBeginTag = "PATTERN_BEGIN";

		private const string PatternEndTag = "PATTERN_END";

		private const string SymmteryTag = "SYMMETRY";

		private const string TopClassTag = "TOP_CLASS";

		private const string TopNameTag = "TOP_NAME";

		private const string TopXyTag = "TOP_XY";

		private static ArrayList patternData;

		private static int bottomClass = 3;

		private static string bottomName = "BOTTOM";

		private static int topClass = 3;

		private static string topName = "TOP";

		private ArrayList bottomPos;

		private ArrayList topPos;

		public static string BottomName => bottomName;

		public static int TopClass => topClass;

		public static string TopName => topName;

		public static int BottomClass => bottomClass;

		public string BeamType { get; set; }

		public double Height { get; set; }

		public bool IsSymmetrical { get; set; }

		public string Name { get; set; }

		public double Width { get; set; }

		public int MaxNumberBottom => IsSymmetrical ? (2 * bottomPos.Count) : bottomPos.Count;

		public int MaxNumberTop => IsSymmetrical ? (2 * topPos.Count) : topPos.Count;

		public PatternInfo()
		{
			BeamType = string.Empty;
			Height = 0.0;
			IsSymmetrical = true;
			Name = string.Empty;
			Width = (Height = 0.0);
			bottomPos = new ArrayList();
			topPos = new ArrayList();
		}

		public static ArrayList GetAllBeamTypes()
		{
			ArrayList arrayList = new ArrayList();
			if (patternData == null)
			{
				LoadPatternData();
			}
			if (patternData != null)
			{
				foreach (PatternInfo patternDatum in patternData)
				{
					if (!arrayList.Contains(patternDatum.BeamType))
					{
						arrayList.Add(patternDatum.BeamType);
					}
				}
			}
			return arrayList;
		}

		public static PatternInfo GetPatternInfo(string name)
		{
			PatternInfo patternInfo = null;
			if (patternData == null)
			{
				LoadPatternData();
			}
			if (patternData != null)
			{
				foreach (PatternInfo patternDatum in patternData)
				{
					if (patternDatum.Name == name)
					{
						patternInfo = patternDatum;
						break;
					}
				}
				if (patternInfo == null)
				{
					patternInfo = (PatternInfo)patternData[0];
				}
			}
			return patternInfo;
		}

		public static ArrayList GetPatternsByBeamType(string type)
		{
			ArrayList arrayList = new ArrayList();
			if (patternData == null)
			{
				LoadPatternData();
			}
			if (patternData != null)
			{
				foreach (PatternInfo patternDatum in patternData)
				{
					if (patternDatum.BeamType == type)
					{
						arrayList.Add(patternDatum);
					}
				}
			}
			return arrayList;
		}

		public static string GetSysFilePathName(Tekla.Structures.Model.Model m, string name)
		{
			string result = string.Empty;
			string[] array = null;
			int num = 0;
			while (true)
			{
				string Value = string.Empty;
				if (num == 0)
				{
					Value = m.GetInfo().ModelPath;
				}
				else if (num == 1)
				{
					TeklaStructuresSettings.GetAdvancedOption("XS_PROJECT", ref Value);
				}
				else if (num == 2)
				{
					TeklaStructuresSettings.GetAdvancedOption("XS_FIRM", ref Value);
				}
				else if (num == 3)
				{
					TeklaStructuresSettings.GetAdvancedOption("XS_SYSTEM", ref Value);
					array = Value.Split(';');
					if (array.GetLength(0) > 0)
					{
						Value = array[0];
					}
				}
				else
				{
					if (array.GetLength(0) <= num - 3)
					{
						break;
					}
					Value = array[num - 3];
				}
				if (Value != string.Empty)
				{
					string text = Value + "\\" + name;
					if (File.Exists(text))
					{
						result = text;
						break;
					}
				}
				num++;
			}
			return result;
		}

		public void AddBottomPos(double x, double y)
		{
			Point value = new Point(x, y, 0.0);
			bottomPos.Add(value);
			SetWidthHeight(x, y);
		}

		public void AddTopPos(double x, double y)
		{
			Point value = new Point(x, y, 0.0);
			topPos.Add(value);
		}

		public bool GetBottomXy(int index, ref double x, ref double y)
		{
			bool result = false;
			int num = (IsSymmetrical ? (index / 2) : index);
			if (num < bottomPos.Count)
			{
				Point point = (Point)bottomPos[num];
				x = point.X;
				y = point.Y;
				if (IsSymmetrical && 2 * num != index)
				{
					x = 0.0 - x;
				}
				result = true;
			}
			return result;
		}

		public bool GetTopXy(int index, ref double x, ref double y)
		{
			bool result = false;
			int num = (IsSymmetrical ? (index / 2) : index);
			if (num < topPos.Count)
			{
				Point point = (Point)topPos[num];
				x = point.X;
				y = point.Y;
				if (IsSymmetrical && 2 * num != index)
				{
					x = 0.0 - x;
				}
				result = true;
			}
			return result;
		}

		public void ReadPatternBlock(StreamReader sr)
		{
			bottomPos = new ArrayList();
			topPos = new ArrayList();
			string text;
			while ((text = sr.ReadLine()) != null && text != "PATTERN_END")
			{
				string[] array = text.Split(new char[1] { ';' }, 3);
				if (array.GetLength(0) > 1)
				{
					if (array[0] == "SYMMETRY" && array.GetLength(0) > 1)
					{
						IsSymmetrical = array[1] != "0";
					}
					else if (array[0] == "BOT_XY" && array.GetLength(0) > 2)
					{
						Point point = new Point(Convert.ToDouble(array[1], CultureInfo.InvariantCulture), Convert.ToDouble(array[2], CultureInfo.InvariantCulture), 0.0);
						SetWidthHeight(point.X, point.Y);
						bottomPos.Add(point);
					}
					else if (array[0] == "TOP_XY" && array.GetLength(0) > 2)
					{
						Point value = new Point(Convert.ToDouble(array[1], CultureInfo.InvariantCulture), Convert.ToDouble(array[2], CultureInfo.InvariantCulture), 0.0);
						topPos.Add(value);
					}
				}
			}
		}

		public bool SetWidthHeight(double pointX, double pointY)
		{
			bool result = false;
			if (bottomPos != null)
			{
				for (int i = 0; i < bottomPos.Count; i++)
				{
					Point point = (Point)bottomPos[i];
					if (point.X < 2.0 * Math.Abs(pointX))
					{
						Width = 2.0 * Math.Abs(pointX);
					}
					if (point.X < 2.0 * Math.Abs(pointY))
					{
						Height = 2.0 * Math.Abs(pointY);
					}
					else
					{
						Height = Width;
					}
					result = true;
				}
			}
			return result;
		}

		private static string GetPatternFileName()
		{
			string result = string.Empty;
			Tekla.Structures.Model.Model model = new Tekla.Structures.Model.Model();
			ModelInfo info = model.GetInfo();
			if (info != null && !info.ModelPath.Equals(string.Empty))
			{
				result = GetSysFilePathName(model, "\\StrandPattern.dat");
			}
			return result;
		}

		private static bool LoadPatternData()
		{
			patternData = new ArrayList();
			try
			{
				string patternFileName = GetPatternFileName();
				if (!patternFileName.Equals(string.Empty))
				{
					StreamReader streamReader = new StreamReader(patternFileName);
					using (streamReader)
					{
						string beamType = string.Empty;
						string text;
						while ((text = streamReader.ReadLine()) != null)
						{
							string[] array = text.Split(new char[1] { ';' }, 3);
							if (array.GetLength(0) <= 1)
							{
								continue;
							}
							if (array[0] == "PATTERN_TYPE")
							{
								beamType = array[1];
							}
							else if (array[0] == "BOT_NAME")
							{
								bottomName = array[1];
							}
							else if (array[0] == "BOT_CLASS")
							{
								try
								{
									bottomClass = Convert.ToInt32(array[1]);
								}
								catch
								{
								}
							}
							else if (array[0] == "TOP_NAME")
							{
								topName = array[1];
							}
							else if (array[0] == "TOP_CLASS")
							{
								try
								{
									topClass = Convert.ToInt32(array[1]);
								}
								catch
								{
								}
							}
							else if (array[0] == "PATTERN_BEGIN")
							{
								PatternInfo patternInfo = new PatternInfo
								{
									Name = array[1],
									BeamType = beamType
								};
								patternInfo.ReadPatternBlock(streamReader);
								patternData.Add(patternInfo);
							}
						}
					}
				}
			}
			catch
			{
				throw new Exception("Exception in LoadPatternData of PatternInfo.");
			}
			return patternData.Count < 0;
		}
	}
}
