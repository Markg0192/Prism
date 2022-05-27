#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Tekla.Structures.Model;

namespace Tekla.Structures.ObjectPropertiesLibrary
{
	public class PresentedPropertiesXml
	{
		public delegate void TickHandler(PresentedPropertiesXml m, EventArgs e);

		public const string CsIniDefaultAttributes = "NAME                            FLOAT       RIGHT    TRUE       8       2         string     string|OBJECT_DESCRIPTION              FLOAT       RIGHT    TRUE       8       2         string     string|OBJECT_TYPE                     FLOAT       RIGHT    TRUE       8       2         string     string|MATERIAL                        FLOAT       RIGHT    TRUE       8       2         string     string|PROFILE                         FLOAT       RIGHT    TRUE       8       2         string     string|MATERIAL_TYPE                   FLOAT       RIGHT    TRUE       8       2         string     string|PART_POS                        FLOAT       RIGHT    TRUE       8       2         string     string|ASSEMBLY_POS                    FLOAT       RIGHT    TRUE       8       2         string     string|MAINPART.PROFILE                FLOAT       RIGHT    TRUE       8       2         string     string|PHASE.NAME                      FLOAT       RIGHT    TRUE       8       2         string     string|TOP_LEVEL                       FLOAT       RIGHT    TRUE       8       2         m          m|AREA                            FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_GROSS                      FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_NET                        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PLAN                       FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_GXY_NET         FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_GXZ_NET         FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_GYZ_NET         FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_GXY_GROSS       FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_GXZ_GROSS       FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_GYZ_GROSS       FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_XY_NET          FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_XZ_NET          FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_YZ_NET          FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_XY_GROSS        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_XZ_GROSS        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_YZ_GROSS        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PGZ                        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_NGZ                        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PGX                        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_NGX                        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PGY                        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_NGY                        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PZ                         FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_NZ                         FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PX                         FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_NX                         FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PY                         FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_NY                         FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_FORM_TOP                   FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_FORM_BOTTOM                FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_FORM_SIDE                  FLOAT       RIGHT    TRUE       8       1         Area       m2|ASSEMBLY_PLWEIGHT               FLOAT       RIGHT    TRUE      10       1         Weight     kg|CAST_UNIT_REBAR_WEIGHT          FLOAT       RIGHT    TRUE       8       1         Weight     kg|HEIGHT                          FLOAT       RIGHT    TRUE       8       1         Length     m|LENGTH                          FLOAT       RIGHT    TRUE       8       1         Length     m|LENGTH_GROSS                    FLOAT       RIGHT    TRUE       8       1         Length     m|LENGTH_MAX                      FLOAT       RIGHT    TRUE       8       1         Length     m|LENGTH_MIN                      FLOAT       RIGHT    TRUE       8       1         Length     m|PROFILE.COVER_AREA              FLOAT       RIGHT    TRUE       7       2         Area       m2|PROFILE.CROSS_SECTION_AREA      FLOAT       RIGHT    TRUE       7       2         Area       m2|PROFILE.FLANGE_THICKNESS        FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.HEIGHT_1                FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.HEIGHT_2                FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.HEIGHT_3                FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.HEIGHT_4                FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.MAJOR_AXIS_LENGTH_1     FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.MAJOR_AXIS_LENGTH_2     FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.MINOR_AXIS_LENGTH_1     FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.MINOR_AXIS_LENGTH_2     FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.WEB_THICKNESS           FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.WIDTH_1                 FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.WIDTH_2                 FLOAT       RIGHT    TRUE       7       2         Length     m|SUPPLEMENT_PART_WEIGHT          FLOAT       LEFT     TRUE       8       1         Weight     kg|WEB_HEIGHT                      FLOAT       RIGHT    TRUE       7       2         Length     m|WEB_LENGTH                      FLOAT       RIGHT    TRUE       7       2         Length     m|WEB_WIDTH                       FLOAT       RIGHT    TRUE       7       2         Length     m|WEIGHT                          FLOAT       RIGHT    TRUE       8       1         Weight     kg|WEIGHT_GROSS                    FLOAT       RIGHT    TRUE       8       1         Weight     kg|WEIGHT_MAX                      FLOAT       RIGHT    TRUE       8       1         Weight     kg|WEIGHT_MIN                      FLOAT       RIGHT    TRUE       8       1         Weight     kg|WEIGHT_NET                      FLOAT       RIGHT    TRUE       8       1         Weight     kg|WIDTH                           FLOAT       RIGHT    TRUE       8       1         Length     m|VOLUME                          FLOAT       RIGHT    TRUE       8       2         Volume     m3|VOLUME_GROSS                    FLOAT       RIGHT    TRUE       8       2         Volume     m3|VOLUME_NET                      FLOAT       RIGHT    TRUE       8       2         Volume     m3|NUMBER                          INTEGER     LEFT     TRUE       8       2         Number     int";

		private const string Comment = "Allowed property types:\n    double    \n    int \n    string\n    date\n    kg    (base type double)\n    ton    (as of thousand of Kg, base type double)\n    mm    (base type double)\n    mm2    (square mm, base type double)\n    mm3    (cubic mm, base type double)\n    cm    (base type double)\n    cm2    (square cm, base type double)\n    cm3    (cubic cm, base type double)\n    m    (base type double)\n    m2    (base type double)\n    m3    (base type double)\n    in    (base type double)\n    in2    (base type double)\n    in3    (base type double)\n    ft    (base type double)\n    ft2    (base type double)\n    ft3    (base type double)\n    yard    (base type double)\n    yard2    (base type double)\n    yard3    (base type double)\n    lbs (base type double)\n    ton_short (base type double)\n    ton_long (base type double)\n    calc (indicates report type is calculated from formula)\n\n    Display type can be any string.\n\n    REPORT_property is either property name (for example \"WEIGHT\") or\n    formula (for example \"WEIGHT / AREA\"). For reference model objects, prefix \"EXTERNAL.\"\n    is needed (for example \"EXTERNAL.BaseQuantities.NetWeight\").\n\n    For formula calculation report properties used have to be defined in this file. Defined property type is then\n    used for calculation. Either defined display name or report property name can be used, first matching property is used.\n";

		private const string XsFirm = "XS_FIRM";

		private const string XsProject = "XS_PROJECT";

		private const string XsSystem = "XS_SYSTEM";

		private Tekla.Structures.Model.Model currentModel;

		private DateTime lastReadTime = DateTime.MinValue;

		private string propertiesFileName = "\\ObjectBrowserProperties.xml";

		private SearchableSortableBindingList<PresentedProperties> propertiesList;

		private int readOnlyOnce;

		private string theFullPath;

		private SearchableSortableBindingList<PresentedProperties> visiblePropertiesList;

		public virtual string LoadSaveDirectory => string.IsNullOrEmpty(theFullPath) ? string.Empty : theFullPath.Substring(0, theFullPath.LastIndexOf("\\", StringComparison.Ordinal));

		public SearchableSortableBindingList<PresentedProperties> PropertiesList
		{
			get
			{
				if (readOnlyOnce == 0)
				{
					SearchableSortableBindingList<PresentedProperties> searchableSortableBindingList = ReadPropertiesList();
					if (searchableSortableBindingList != null)
					{
						propertiesList = searchableSortableBindingList;
					}
					readOnlyOnce = 1;
				}
				return propertiesList;
			}
			set
			{
				propertiesList = value;
				visiblePropertiesList = null;
			}
		}

		public SearchableSortableBindingList<PresentedProperties> VisiblePropertiesList
		{
			get
			{
				if (visiblePropertiesList == null)
				{
					if (readOnlyOnce == 0)
					{
						SearchableSortableBindingList<PresentedProperties> searchableSortableBindingList = ReadPropertiesList();
						if (searchableSortableBindingList != null)
						{
							propertiesList = searchableSortableBindingList;
						}
						readOnlyOnce = 1;
					}
					visiblePropertiesList = new SearchableSortableBindingList<PresentedProperties>();
					foreach (PresentedProperties item in propertiesList.Where((PresentedProperties property) => !property.Hidden))
					{
						visiblePropertiesList.Add(item);
					}
				}
				return visiblePropertiesList;
			}
			set
			{
			}
		}

		public event TickHandler Modified;

		public PresentedPropertiesXml(string fileName, Tekla.Structures.Model.Model openModel)
		{
			Initialize(fileName, openModel);
		}

		protected PresentedPropertiesXml()
		{
		}

		public static SearchableSortableBindingList<PresentedProperties> ReadPropertiesListFromFile(string fileName)
		{
			SearchableSortableBindingList<PresentedProperties> result = null;
			FileInfo fileInfo = new FileInfo(fileName);
			if (fileInfo.Length > 0)
			{
				try
				{
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.Load(fileName);
					if (xmlDocument.DocumentElement != null)
					{
						string name = xmlDocument.DocumentElement.Name;
						XmlSerializer xmlSerializer = new XmlSerializer(typeof(SearchableSortableBindingList<PresentedProperties>), new XmlRootAttribute(name));
						TextReader textReader = new StreamReader(fileName);
						result = (SearchableSortableBindingList<PresentedProperties>)xmlSerializer.Deserialize(textReader);
						textReader.Close();
					}
				}
				catch (IOException value)
				{
					Debug.WriteLine(value);
				}
				catch (XmlException value2)
				{
					Debug.WriteLine(value2);
				}
			}
			return result;
		}

		public static void SortBindingList(ref BindingList<object> list, ListSortDirection direction, PropertyDescriptor property)
		{
			SearchableSortableBindingList<object> searchableSortableBindingList = new SearchableSortableBindingList<object>();
			foreach (object item in list)
			{
				searchableSortableBindingList.Add(item);
			}
			searchableSortableBindingList.Sort(property, direction);
			list.Clear();
			foreach (object item2 in searchableSortableBindingList)
			{
				list.Add(item2);
			}
		}

		public static bool XmlWriteProperties(BindingList<PresentedProperties> listToSerialize, string fileName)
		{
			bool result = false;
			try
			{
				XmlRootAttribute root = new XmlRootAttribute("Properties")
				{
					IsNullable = true
				};
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(BindingList<PresentedProperties>), root);
				XmlWriterSettings settings = new XmlWriterSettings
				{
					NewLineHandling = NewLineHandling.Entitize,
					Indent = true,
					NewLineOnAttributes = true
				};
				XmlWriter xmlWriter = XmlWriter.Create(fileName, settings);
				xmlWriter.WriteComment("Allowed property types:\n    double    \n    int \n    string\n    date\n    kg    (base type double)\n    ton    (as of thousand of Kg, base type double)\n    mm    (base type double)\n    mm2    (square mm, base type double)\n    mm3    (cubic mm, base type double)\n    cm    (base type double)\n    cm2    (square cm, base type double)\n    cm3    (cubic cm, base type double)\n    m    (base type double)\n    m2    (base type double)\n    m3    (base type double)\n    in    (base type double)\n    in2    (base type double)\n    in3    (base type double)\n    ft    (base type double)\n    ft2    (base type double)\n    ft3    (base type double)\n    yard    (base type double)\n    yard2    (base type double)\n    yard3    (base type double)\n    lbs (base type double)\n    ton_short (base type double)\n    ton_long (base type double)\n    calc (indicates report type is calculated from formula)\n\n    Display type can be any string.\n\n    REPORT_property is either property name (for example \"WEIGHT\") or\n    formula (for example \"WEIGHT / AREA\"). For reference model objects, prefix \"EXTERNAL.\"\n    is needed (for example \"EXTERNAL.BaseQuantities.NetWeight\").\n\n    For formula calculation report properties used have to be defined in this file. Defined property type is then\n    used for calculation. Either defined display name or report property name can be used, first matching property is used.\n");
				xmlSerializer.Serialize(xmlWriter, listToSerialize);
				xmlWriter.Close();
				result = true;
			}
			catch (IOException value)
			{
				Debug.WriteLine(value);
			}
			catch (XmlException value2)
			{
				Debug.WriteLine(value2);
			}
			return result;
		}

		public bool AddPropertyWithReportName(string reportPropertyName, string valueString)
		{
			bool result = false;
			PresentedProperties property = null;
			string type = "string";
			if (!string.IsNullOrEmpty(valueString))
			{
				type = TryToGuessType(valueString);
			}
			if (!GetPropertyByName(reportPropertyName, ref property))
			{
				property = new PresentedProperties(reportPropertyName, reportPropertyName, string.Empty, type, 2, 100, hidden: true);
				PropertiesList.Add(property);
				result = true;
			}
			return result;
		}

		public void ForceReadFile()
		{
			visiblePropertiesList = null;
			lastReadTime = DateTime.MinValue;
			readOnlyOnce = 0;
		}

		public int GetPropertiesHashCode(BindingList<PresentedProperties> listToHash)
		{
			string text = listToHash.Aggregate(string.Empty, (string current, PresentedProperties presentedProperties) => current + presentedProperties.ToString());
			return text.GetHashCode();
		}

		public bool GetPropertyByModelPropertyName(string modelPropertyName, ref PresentedProperties property)
		{
			bool result = false;
			if (!string.IsNullOrEmpty(modelPropertyName))
			{
				foreach (PresentedProperties item in PropertiesList.Where((PresentedProperties propertyInList) => propertyInList.ReportPropertyName == modelPropertyName || propertyInList.UdaPropertyName == modelPropertyName))
				{
					PresentedProperties presentedProperties = (property = item);
					result = true;
				}
			}
			return result;
		}

		public bool GetPropertyByName(string propertyName, ref PresentedProperties property)
		{
			bool result = false;
			if (!string.IsNullOrEmpty(propertyName))
			{
				foreach (PresentedProperties item in PropertiesList.Where((PresentedProperties propertyInList) => propertyInList.Name == propertyName))
				{
					PresentedProperties presentedProperties = (property = item);
					result = true;
				}
			}
			return result;
		}

		public bool GetPropertyByNameOrModelPropertyName(string propertyName, ref PresentedProperties property)
		{
			bool result = false;
			if (GetPropertyByName(propertyName, ref property))
			{
				result = true;
			}
			else if (GetPropertyByModelPropertyName(propertyName, ref property))
			{
				result = true;
			}
			return result;
		}

		public bool Initialize(string fileName, Tekla.Structures.Model.Model openModel)
		{
			propertiesFileName = fileName;
			if (!propertiesFileName.StartsWith("\\"))
			{
				propertiesFileName = "\\" + propertiesFileName;
			}
			currentModel = openModel;
			ForceReadFile();
			if (!GetFullPathToExistingFile(propertiesFileName, ref theFullPath))
			{
				CreateDefaultFile();
			}
			return true;
		}

		public void MergeProperties(PresentedPropertiesXml propertiesToMerge)
		{
			SearchableSortableBindingList<PresentedProperties> propertiesExists = new SearchableSortableBindingList<PresentedProperties>();
			foreach (PresentedProperties properties in propertiesToMerge.PropertiesList)
			{
				foreach (PresentedProperties properties2 in PropertiesList)
				{
					if (properties2.Equals(properties))
					{
						properties2.Copy(properties);
						propertiesExists.Add(properties);
					}
				}
			}
			foreach (PresentedProperties item in propertiesToMerge.PropertiesList.Where((PresentedProperties property) => !propertiesExists.Contains(property)))
			{
				PropertiesList.Add(item);
			}
		}

		public void MoveProperty(PresentedProperties propertyToMove, PresentedProperties propertyToMoveBy)
		{
			PresentedProperties item = new PresentedProperties(propertyToMove);
			PropertiesList.Remove(propertyToMove);
			if (propertyToMoveBy != null)
			{
				PropertiesList.Insert(PropertiesList.IndexOf(propertyToMoveBy), item);
			}
			else
			{
				PropertiesList.Add(item);
			}
		}

		public void RemoveProperty(PresentedProperties propertyToRemove)
		{
			if (PropertiesList.Contains(propertyToRemove))
			{
				PropertiesList.Remove(propertyToRemove);
			}
		}

		public bool XmlWriteProperties()
		{
			return XmlWriteProperties(PropertiesList);
		}

		public virtual bool XmlWriteProperties(BindingList<PresentedProperties> listToSerialize)
		{
			if (theFullPath != Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\", StringComparison.Ordinal) + 1) + propertiesFileName)
			{
				theFullPath = currentModel.GetInfo().ModelPath + propertiesFileName;
			}
			return XmlWriteProperties(listToSerialize, theFullPath);
		}

		protected virtual SearchableSortableBindingList<PresentedProperties> ReadPropertiesList()
		{
			SearchableSortableBindingList<PresentedProperties> result = null;
			if (File.Exists(theFullPath))
			{
				DateTime lastWriteTime = new FileInfo(theFullPath).LastWriteTime;
				if (lastReadTime < lastWriteTime)
				{
					lastReadTime = lastWriteTime;
					result = ReadPropertiesListFromFile(theFullPath);
					if (this.Modified != null)
					{
						this.Modified(this, null);
					}
					if (theFullPath != currentModel.GetInfo().ModelPath + propertiesFileName)
					{
						XmlWriteProperties(propertiesList);
					}
				}
			}
			return result;
		}

		private static string GetFullPath(IEnumerable paths, string fileName)
		{
			using (IEnumerator<string> enumerator = (from string path in paths
				where !string.IsNullOrEmpty(path) && File.Exists(path + fileName)
				select path).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string current = enumerator.Current;
					return current + fileName;
				}
			}
			return string.Empty;
		}

		private static int OccurencesOf(string searchIn, string searchThis)
		{
			bool flag = true;
			int num = 0;
			int num2 = -1;
			if (!string.IsNullOrEmpty(searchIn))
			{
				while (flag)
				{
					int num3 = num2;
					if (searchIn.Length > num2 + 1)
					{
						num3 = searchIn.IndexOf(searchThis, num2 + 1, StringComparison.Ordinal);
					}
					if (num3 > num2)
					{
						num2 = num3;
						num++;
					}
					else
					{
						flag = false;
					}
				}
			}
			return num;
		}

		private static string TryToGuessType(string stringValue)
		{
			string text = "string";
			bool flag = true;
			try
			{
				text = "double";
			}
			catch (Exception)
			{
				try
				{
					string text2 = string.Empty;
					if (stringValue.Contains("."))
					{
						text2 = stringValue.Replace(".", ",");
						if (OccurencesOf(text2, ",") > 1)
						{
							flag = false;
						}
					}
					else if (stringValue.Contains(","))
					{
						text2 = stringValue.Replace(",", ".");
					}
					if (!string.IsNullOrEmpty(text2) && flag)
					{
						text = "double";
					}
				}
				catch (Exception)
				{
					text = "string";
				}
			}
			return (text == "string") ? "date" : text;
		}

		private void CreateDefaultFile()
		{
			string[] array = "NAME                            FLOAT       RIGHT    TRUE       8       2         string     string|OBJECT_DESCRIPTION              FLOAT       RIGHT    TRUE       8       2         string     string|OBJECT_TYPE                     FLOAT       RIGHT    TRUE       8       2         string     string|MATERIAL                        FLOAT       RIGHT    TRUE       8       2         string     string|PROFILE                         FLOAT       RIGHT    TRUE       8       2         string     string|MATERIAL_TYPE                   FLOAT       RIGHT    TRUE       8       2         string     string|PART_POS                        FLOAT       RIGHT    TRUE       8       2         string     string|ASSEMBLY_POS                    FLOAT       RIGHT    TRUE       8       2         string     string|MAINPART.PROFILE                FLOAT       RIGHT    TRUE       8       2         string     string|PHASE.NAME                      FLOAT       RIGHT    TRUE       8       2         string     string|TOP_LEVEL                       FLOAT       RIGHT    TRUE       8       2         m          m|AREA                            FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_GROSS                      FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_NET                        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PLAN                       FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_GXY_NET         FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_GXZ_NET         FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_GYZ_NET         FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_GXY_GROSS       FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_GXZ_GROSS       FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_GYZ_GROSS       FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_XY_NET          FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_XZ_NET          FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_YZ_NET          FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_XY_GROSS        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_XZ_GROSS        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PROJECTION_YZ_GROSS        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PGZ                        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_NGZ                        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PGX                        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_NGX                        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PGY                        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_NGY                        FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PZ                         FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_NZ                         FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PX                         FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_NX                         FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_PY                         FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_NY                         FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_FORM_TOP                   FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_FORM_BOTTOM                FLOAT       RIGHT    TRUE       8       1         Area       m2|AREA_FORM_SIDE                  FLOAT       RIGHT    TRUE       8       1         Area       m2|ASSEMBLY_PLWEIGHT               FLOAT       RIGHT    TRUE      10       1         Weight     kg|CAST_UNIT_REBAR_WEIGHT          FLOAT       RIGHT    TRUE       8       1         Weight     kg|HEIGHT                          FLOAT       RIGHT    TRUE       8       1         Length     m|LENGTH                          FLOAT       RIGHT    TRUE       8       1         Length     m|LENGTH_GROSS                    FLOAT       RIGHT    TRUE       8       1         Length     m|LENGTH_MAX                      FLOAT       RIGHT    TRUE       8       1         Length     m|LENGTH_MIN                      FLOAT       RIGHT    TRUE       8       1         Length     m|PROFILE.COVER_AREA              FLOAT       RIGHT    TRUE       7       2         Area       m2|PROFILE.CROSS_SECTION_AREA      FLOAT       RIGHT    TRUE       7       2         Area       m2|PROFILE.FLANGE_THICKNESS        FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.HEIGHT_1                FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.HEIGHT_2                FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.HEIGHT_3                FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.HEIGHT_4                FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.MAJOR_AXIS_LENGTH_1     FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.MAJOR_AXIS_LENGTH_2     FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.MINOR_AXIS_LENGTH_1     FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.MINOR_AXIS_LENGTH_2     FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.WEB_THICKNESS           FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.WIDTH_1                 FLOAT       RIGHT    TRUE       7       2         Length     m|PROFILE.WIDTH_2                 FLOAT       RIGHT    TRUE       7       2         Length     m|SUPPLEMENT_PART_WEIGHT          FLOAT       LEFT     TRUE       8       1         Weight     kg|WEB_HEIGHT                      FLOAT       RIGHT    TRUE       7       2         Length     m|WEB_LENGTH                      FLOAT       RIGHT    TRUE       7       2         Length     m|WEB_WIDTH                       FLOAT       RIGHT    TRUE       7       2         Length     m|WEIGHT                          FLOAT       RIGHT    TRUE       8       1         Weight     kg|WEIGHT_GROSS                    FLOAT       RIGHT    TRUE       8       1         Weight     kg|WEIGHT_MAX                      FLOAT       RIGHT    TRUE       8       1         Weight     kg|WEIGHT_MIN                      FLOAT       RIGHT    TRUE       8       1         Weight     kg|WEIGHT_NET                      FLOAT       RIGHT    TRUE       8       1         Weight     kg|WIDTH                           FLOAT       RIGHT    TRUE       8       1         Length     m|VOLUME                          FLOAT       RIGHT    TRUE       8       2         Volume     m3|VOLUME_GROSS                    FLOAT       RIGHT    TRUE       8       2         Volume     m3|VOLUME_NET                      FLOAT       RIGHT    TRUE       8       2         Volume     m3|NUMBER                          INTEGER     LEFT     TRUE       8       2         Number     int".Split('|');
			propertiesList = new SearchableSortableBindingList<PresentedProperties>();
			string[] array2 = array;
			foreach (string text in array2)
			{
				string[] array3 = text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (array3.Length != 0)
				{
					PresentedProperties item = new PresentedProperties(array3[0], array3[0], string.Empty, array3[7], 2, 100, hidden: false);
					propertiesList.Add(item);
				}
			}
			PresentedProperties item2 = new PresentedProperties("Rebar Group Weight", "WEIGHT * NUMBER", string.Empty, "calc", 2, 100, hidden: false);
			propertiesList.Add(item2);
			XmlWriteProperties(propertiesList);
		}

		private bool GetFullPathToExistingFile(string fileName, ref string fullPath)
		{
			bool result = false;
			string Value = string.Empty;
			string Value2 = string.Empty;
			string Value3 = string.Empty;
			if (!fileName.StartsWith("\\"))
			{
				fileName = "\\" + fileName;
			}
			TeklaStructuresSettings.GetAdvancedOption("XS_PROJECT", ref Value3);
			TeklaStructuresSettings.GetAdvancedOption("XS_FIRM", ref Value2);
			TeklaStructuresSettings.GetAdvancedOption("XS_SYSTEM", ref Value);
			if (File.Exists(Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\", StringComparison.Ordinal)) + fileName))
			{
				fullPath = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\", StringComparison.Ordinal)) + fileName;
			}
			if (File.Exists(currentModel.GetInfo().ModelPath + fileName))
			{
				fullPath = currentModel.GetInfo().ModelPath + fileName;
			}
			if (string.IsNullOrEmpty(fullPath))
			{
				fullPath = GetFullPath(Value3.Split(';'), fileName);
			}
			if (string.IsNullOrEmpty(fullPath))
			{
				fullPath = GetFullPath(Value2.Split(';'), fileName);
			}
			if (string.IsNullOrEmpty(fullPath))
			{
				fullPath = GetFullPath(Value.Split(';'), fileName);
			}
			if (string.IsNullOrEmpty(fullPath))
			{
				fullPath = currentModel.GetInfo().ModelPath + fileName;
			}
			else
			{
				result = true;
			}
			return result;
		}
	}
}
