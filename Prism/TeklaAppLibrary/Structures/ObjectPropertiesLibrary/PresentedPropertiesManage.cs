#define DEBUG
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Tekla.Structures.Datatype;
using Tekla.Structures.Model;

namespace Tekla.Structures.ObjectPropertiesLibrary
{
	public class PresentedPropertiesManage
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		public struct PropertyTypes
		{
			public const string Date = "date";

			public const string Double = "double";

			public const string Formula = "calc";

			public const string Int = "int";

			public const string Kg = "kg";

			public const string Pieces = "pcs";

			public const string String = "string";

			public const string Ton = "ton";

			public const string Cm = "cm";

			public const string Cm2 = "cm2";

			public const string Cm3 = "cm3";

			public const string Ft = "ft";

			public const string Ft2 = "ft2";

			public const string Ft3 = "ft3";

			public const string Inch = "in";

			public const string Inch2 = "in2";

			public const string Inch3 = "in3";

			public const string Lbs = "lbs";

			public const string M = "m";

			public const string M2 = "m2";

			public const string M3 = "m3";

			public const string Mm = "mm";

			public const string Mm2 = "mm2";

			public const string Mm3 = "mm3";

			public const string TonLong = "ton_long";

			public const string TonShort = "ton_short";

			public const string Yard = "yard";

			public const string Yard2 = "yard2";

			public const string Yard3 = "yard3";
		}

		public static bool ChangeHiddenValuesByOtherFile(ref PresentedPropertiesXml all, ref PresentedPropertiesXml shown)
		{
			foreach (PresentedProperties properties in all.PropertiesList)
			{
				properties.Visible = false;
				foreach (PresentedProperties properties2 in shown.PropertiesList)
				{
					if (properties2.Name == properties.Name)
					{
						properties.Visible = true;
					}
				}
			}
			return all.XmlWriteProperties(all.PropertiesList);
		}

		public static string GetBaseType(string propertyType)
		{
			switch (propertyType.ToLower())
			{
			case "string":
				return "string";
			case "date":
				return "date";
			case "pcs":
			case "int":
				return "int";
			default:
				return "double";
			}
		}

		public static object GetPartPropertyValue(ModelObject thisPart, string visibleName, PresentedPropertiesXml presentedPropertiesInstance)
		{
			object result = null;
			PresentedProperties property = null;
			if (presentedPropertiesInstance.GetPropertyByName(visibleName, ref property))
			{
				result = GetPartPropertyValue(thisPart, property, presentedPropertiesInstance);
			}
			return result;
		}

		public static object GetPartPropertyValue(ModelObject thisPart, PresentedProperties property, PresentedPropertiesXml presentedPropertiesInstance)
		{
			bool isReportProperty = !string.IsNullOrEmpty(property.ReportPropertyName);
			string propertvalueYBasedOnType = GetPropertvalueYBasedOnType(presentedPropertiesInstance, thisPart, property, isReportProperty);
			try
			{
				return GetBaseType(property.PropertyType) switch
				{
					"int" => (!string.IsNullOrEmpty(propertvalueYBasedOnType)) ? Convert.ToInt32(propertvalueYBasedOnType) : 0, 
					"string" => propertvalueYBasedOnType, 
					"date" => string.IsNullOrEmpty(propertvalueYBasedOnType) ? null : ((object)Convert.ToDateTime(propertvalueYBasedOnType)), 
					_ => string.IsNullOrEmpty(propertvalueYBasedOnType) ? 0.0 : Convert.ToDouble(propertvalueYBasedOnType), 
				};
			}
			catch (Exception value)
			{
				Debug.WriteLine(value);
			}
			return null;
		}

		public static bool MakeFileFromOtherFilesHiddenProperties(ref PresentedPropertiesXml shown, ref PresentedPropertiesXml all)
		{
			SearchableSortableBindingList<PresentedProperties> searchableSortableBindingList = new SearchableSortableBindingList<PresentedProperties>();
			foreach (PresentedProperties item in all.PropertiesList.Where((PresentedProperties allProperty) => !allProperty.Hidden))
			{
				searchableSortableBindingList.Add(item);
			}
			SearchableSortableBindingList<PresentedProperties> orderedNewShownProperties = new SearchableSortableBindingList<PresentedProperties>();
			foreach (PresentedProperties properties in shown.PropertiesList)
			{
				for (int i = 0; i < searchableSortableBindingList.Count; i++)
				{
					if (searchableSortableBindingList[i].Equals(properties))
					{
						orderedNewShownProperties.Add(searchableSortableBindingList[i]);
						searchableSortableBindingList.RemoveAt(i);
						break;
					}
				}
			}
			foreach (PresentedProperties item2 in searchableSortableBindingList.Where((PresentedProperties shownProperty) => !orderedNewShownProperties.Contains(shownProperty)))
			{
				orderedNewShownProperties.Add(item2);
			}
			shown.PropertiesList = orderedNewShownProperties;
			return shown.XmlWriteProperties(shown.PropertiesList);
		}

		private static bool GetAttributeByUDAFormula(PresentedPropertiesXml presentedPropertiesInstance, ModelObject modelObj, string udaEquation, out double resultValue)
		{
			char[] array = new char[6] { '+', '-', '*', '/', '(', ')' };
			char[] separator = " ".ToCharArray();
			int startIndex = 0;
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			CultureInfo provider = new CultureInfo("en-us");
			resultValue = 0.0;
			string text = udaEquation;
			for (int i = 0; i < array.Length; i++)
			{
				text = text.Replace(array[i].ToString(CultureInfo.InvariantCulture), " ");
			}
			string[] array2 = text.Split(separator);
			string[] array3 = new string[array2.Length];
			for (int j = 0; j < array2.Length; j++)
			{
				if (array2[j].Length > 0)
				{
					double result;
					bool flag = double.TryParse(array2[j], NumberStyles.Any, currentCulture, out result);
					if (!flag)
					{
						flag = double.TryParse(array2[j], NumberStyles.Any, provider, out result);
					}
					if (!flag)
					{
						array3[j] = array2[j];
						string value = "_UDA_" + j.ToString(CultureInfo.InvariantCulture);
						startIndex = udaEquation.IndexOf(array2[j], startIndex, StringComparison.Ordinal);
						startIndex += array2[j].Length;
						udaEquation = udaEquation.Insert(startIndex, value);
					}
				}
			}
			for (int k = 0; k < array3.Length; k++)
			{
				if (array3[k] == null || array3[k].Length == 0)
				{
					continue;
				}
				PresentedProperties property = null;
				if (presentedPropertiesInstance.GetPropertyByNameOrModelPropertyName(array3[k], ref property))
				{
					string propertvalueYBasedOnType = GetPropertvalueYBasedOnType(presentedPropertiesInstance, modelObj, property, isReportProperty: true, roundDoubles: false);
					try
					{
						double result = Convert.ToDouble(propertvalueYBasedOnType);
						string value = array3[k] + "_UDA_" + k.ToString(CultureInfo.InvariantCulture);
						udaEquation = udaEquation.Replace(value, result.ToString(CultureInfo.InvariantCulture));
					}
					catch (Exception value2)
					{
						Debug.WriteLine(value2);
						return false;
					}
				}
			}
			MathEvaluate mathEvaluate = new MathEvaluate();
			if (mathEvaluate.Parse(udaEquation) && !mathEvaluate.Error)
			{
				mathEvaluate.Infix2Postfix();
				mathEvaluate.EvaluatePostfix();
			}
			if (mathEvaluate.Error)
			{
				if (mathEvaluate.ErrorDescription != "Divide by Zero")
				{
					return false;
				}
			}
			else
			{
				resultValue = mathEvaluate.Result;
			}
			return true;
		}

		private static bool GetProperty(ModelObject propertyOwner, string propertyName, bool isReportProperty, ref string returnValue)
		{
			return isReportProperty ? propertyOwner.GetReportProperty(propertyName, ref returnValue) : propertyOwner.GetUserProperty(propertyName, ref returnValue);
		}

		private static bool GetProperty(ModelObject propertyOwner, string propertyName, bool isReportProperty, ref int returnValue)
		{
			return isReportProperty ? propertyOwner.GetReportProperty(propertyName, ref returnValue) : propertyOwner.GetUserProperty(propertyName, ref returnValue);
		}

		private static bool GetProperty(ModelObject propertyOwner, string propertyName, bool isReportProperty, ref double returnValue)
		{
			string value = string.Empty;
			bool result;
			if (isReportProperty)
			{
				if (propertyName.Contains("EXTERNAL.") || propertyName.Contains("TOP_LEVEL"))
				{
					result = propertyOwner.GetReportProperty(propertyName, ref value);
					if (value.Contains("'") || value.Contains("\""))
					{
						try
						{
							Distance.set_UseFractionalFormat(true);
							Distance val = default(Distance);
							if (Distance.TryParse(value.Trim(), (IFormatProvider)CultureInfo.CurrentCulture, (UnitType)3, ref val))
							{
								returnValue = ((Distance)(ref val)).get_Value();
								result = true;
							}
						}
						catch (Exception value2)
						{
							Debug.WriteLine(value2);
						}
					}
					else if (string.IsNullOrEmpty(value))
					{
						result = propertyOwner.GetReportProperty(propertyName, ref returnValue);
					}
					else
					{
						try
						{
							string s = value;
							if (propertyName.Contains("TOP_LEVEL"))
							{
								s = value.Replace(".", string.Empty);
							}
							if (!double.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out returnValue))
							{
								if (value.Contains("."))
								{
									s = value.Replace(".", ",");
								}
								else if (value.Contains(","))
								{
									s = value.Replace(",", ".");
								}
							}
							if (!double.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out returnValue))
							{
								Debug.WriteLine("GetProperty: string was not convertable.");
							}
						}
						catch (Exception value3)
						{
							Debug.WriteLine(value3);
						}
					}
				}
				else
				{
					result = propertyOwner.GetReportProperty(propertyName, ref returnValue);
				}
			}
			else
			{
				result = propertyOwner.GetUserProperty(propertyName, ref returnValue);
			}
			return result;
		}

		private static string GetPropertvalueYBasedOnType(PresentedPropertiesXml presentedPropertiesInstance, ModelObject part, PresentedProperties thisProperty, bool isReportProperty, bool roundDoubles = true)
		{
			string propertyType = thisProperty.PropertyType;
			int decimals = thisProperty.Decimals;
			string text = thisProperty.ReportPropertyName;
			if (!isReportProperty)
			{
				text = thisProperty.UdaPropertyName;
			}
			string text2 = string.Empty;
			int returnValue = 0;
			double returnValue2 = 0.0;
			string returnValue3 = string.Empty;
			DateTime dateTime = new DateTime(1970, 1, 1);
			string text3 = (roundDoubles ? ("N" + decimals.ToString(CultureInfo.InvariantCulture)) : "G");
			switch (propertyType.ToLower())
			{
			case "pcs":
				text2 = "1";
				break;
			case "calc":
				if (GetAttributeByUDAFormula(presentedPropertiesInstance, part, text, out returnValue2))
				{
					text2 = returnValue2.ToString(text3);
				}
				break;
			case "int":
				GetProperty(part, text, isReportProperty, ref returnValue);
				text2 = returnValue.ToString(CultureInfo.InvariantCulture);
				break;
			case "mm":
			case "mm2":
			case "mm3":
			case "double":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = returnValue2.ToString(text3);
				break;
			case "string":
				GetProperty(part, text, isReportProperty, ref returnValue3);
				text2 = returnValue3;
				break;
			case "date":
				if (!text.StartsWith("EXTERNAL."))
				{
					GetProperty(part, text, isReportProperty, ref returnValue);
					if (returnValue != 0)
					{
						text2 = dateTime.AddSeconds(returnValue).ToString(CultureInfo.InvariantCulture);
					}
				}
				else
				{
					GetProperty(part, text, isReportProperty, ref returnValue3);
					text2 = returnValue3;
				}
				break;
			case "kg":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = returnValue2.ToString(text3);
				break;
			case "ton":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 1000.0).ToString(text3);
				break;
			case "cm":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 10.0).ToString(text3);
				break;
			case "m":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 1000.0).ToString(text3);
				break;
			case "cm2":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 100.0).ToString(text3);
				break;
			case "m2":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 1000000.0).ToString(text3);
				break;
			case "cm3":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 1000.0).ToString(text3);
				break;
			case "m3":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 1000000000.0).ToString(text3);
				break;
			case "in":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 25.4).ToString(text3);
				break;
			case "in2":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 645.16).ToString(text3);
				break;
			case "in3":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 16387.064).ToString(text3);
				break;
			case "ft":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 304.8).ToString(text3);
				break;
			case "ft2":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 92903.04).ToString(text3);
				break;
			case "ft3":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 28316846.6).ToString(text3);
				break;
			case "yard":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 914.4).ToString(text3);
				break;
			case "yard2":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 836127.36).ToString(text3);
				break;
			case "yard3":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 764554857.984).ToString(text3);
				break;
			case "lbs":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 0.45359237).ToString(text3);
				break;
			case "ton_short":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 907.18474).ToString(text3);
				break;
			case "ton_long":
				GetProperty(part, text, isReportProperty, ref returnValue2);
				text2 = (returnValue2 / 1016.0469088).ToString(text3);
				break;
			}
			return text2 ?? string.Empty;
		}
	}
}
