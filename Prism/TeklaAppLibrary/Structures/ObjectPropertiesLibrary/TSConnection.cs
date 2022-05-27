#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Tekla.Structures.Dialog;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace Tekla.Structures.ObjectPropertiesLibrary
{
	public class TSConnection
	{
		private const string PrivateFileOnTheSide = "ObjectBrowser.ail";

		private const string PrivateLocalizationFile = "messages\\DotAppsStrings\\ObjectBrowser.ail";

		private const string XsDirOwn = "XS_DIR";

		private Events events;

		private string localizationCommonFile;

		private string localizationPrivateFile;

		private Tekla.Structures.Model.Model model;

		private Tekla.Structures.Model.UI.ModelObjectSelector modelSelector;

		private string teklaDir;

		private string currentLanguage;

		private Localization localizationForm;

		public string CurrentLanguage
		{
			get
			{
				if (currentLanguage == null)
				{
					SetLanguage();
				}
				return currentLanguage;
			}
			set
			{
				currentLanguage = value;
			}
		}

		public Localization LocalizationForm
		{
			get
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_001a: Expected O, but got Unknown
				if (localizationForm == null)
				{
					localizationForm = new Localization();
					if (!string.IsNullOrEmpty(LocalizationCommonFile))
					{
						localizationForm.LoadFile(LocalizationCommonFile);
					}
					if (!string.IsNullOrEmpty(LocalizationPrivateFile))
					{
						localizationForm.LoadFile(LocalizationPrivateFile);
					}
					localizationForm.set_Language(CurrentLanguage);
				}
				return localizationForm;
			}
		}

		public Tekla.Structures.Model.Model OpenModel => model;

		public Events TeklaEvents
		{
			get
			{
				if (events == null && model != null && model.GetConnectionStatus())
				{
					events = new Events();
				}
				return events;
			}
		}

		protected Tekla.Structures.Model.UI.ModelObjectSelector ModelSelector
		{
			get
			{
				if (modelSelector == null && OpenModel.GetConnectionStatus())
				{
					modelSelector = new Tekla.Structures.Model.UI.ModelObjectSelector();
				}
				return modelSelector;
			}
		}

		private string LocalizationCommonFile
		{
			get
			{
				if (localizationCommonFile == null)
				{
					if (TeklaDir != null)
					{
						localizationCommonFile = Localization.get_DefaultLocalizationFile();
					}
					if (!File.Exists(localizationCommonFile))
					{
						localizationCommonFile = string.Empty;
					}
				}
				return localizationCommonFile;
			}
		}

		private string LocalizationPrivateFile
		{
			get
			{
				if (localizationPrivateFile == null)
				{
					if (TeklaDir != null)
					{
						localizationPrivateFile = Path.Combine(TeklaDir, "messages\\DotAppsStrings\\ObjectBrowser.ail");
					}
					if (!File.Exists(localizationPrivateFile))
					{
						localizationPrivateFile = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\") + 1) + "ObjectBrowser.ail";
						if (!File.Exists(localizationPrivateFile))
						{
							localizationPrivateFile = string.Empty;
						}
					}
				}
				return localizationPrivateFile;
			}
		}

		private string TeklaDir
		{
			get
			{
				if (teklaDir == null && OpenModel != null)
				{
					TeklaStructuresSettings.GetAdvancedOption("XS_DIR", ref teklaDir);
				}
				return teklaDir;
			}
		}

		public TSConnection()
		{
			Connect();
		}

		public void Connect()
		{
			if (this.model == null)
			{
				Tekla.Structures.Model.Model model = new Tekla.Structures.Model.Model();
				if (!model.GetConnectionStatus())
				{
					throw new ApplicationException("Cannot connect to TeklaStructures process.");
				}
				if (string.IsNullOrEmpty(model.GetInfo().ModelPath))
				{
					throw new ApplicationException("Model is not loaded into TeklaStructures.");
				}
				this.model = model;
				TeklaEvents.TeklaStructuresExit += delegate
				{
					Application.Exit();
				};
				TeklaEvents.ModelLoad += delegate
				{
					Application.Restart();
				};
				TeklaEvents.Register();
				TeklaStructures.Connect();
			}
		}

		public void Dispose()
		{
			if (events != null)
			{
				events.UnRegister();
			}
			TeklaStructures.Disconnect();
		}

		public Dictionary<string, ModelObject> GetAllObjects()
		{
			Dictionary<string, ModelObject> dictionary = new Dictionary<string, ModelObject>();
			Tekla.Structures.Model.ModelObjectSelector modelObjectSelector = model.GetModelObjectSelector();
			ModelObjectEnumerator allObjects = modelObjectSelector.GetAllObjects();
			allObjects.SelectInstances = false;
			try
			{
				while (allObjects.MoveNext())
				{
					string gUIDByIdentifier;
					if (allObjects.Current is ReferenceModel)
					{
						ModelObjectEnumerator children = allObjects.Current.GetChildren();
						children.SelectInstances = false;
						while (children.MoveNext())
						{
							gUIDByIdentifier = OpenModel.GetGUIDByIdentifier(children.Current.Identifier);
							if (!string.IsNullOrEmpty(gUIDByIdentifier))
							{
								dictionary.Add(gUIDByIdentifier, children.Current);
							}
						}
					}
					gUIDByIdentifier = OpenModel.GetGUIDByIdentifier(allObjects.Current.Identifier);
					if (!string.IsNullOrEmpty(gUIDByIdentifier))
					{
						dictionary.Add(gUIDByIdentifier, allObjects.Current);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
			}
			return dictionary;
		}

		[Conditional("DEBUG")]
		public void GetChildrenOfSelected()
		{
			string text = string.Empty;
			IEnumerator<object> enumerator = TeklaStructures.Model.SelectedObjects.GetEnumerator();
			enumerator.MoveNext();
			ModelObject modelObject = (ModelObject)enumerator.Current;
			ModelObjectEnumerator children = modelObject.GetChildren();
			children.SelectInstances = false;
			while (children.MoveNext())
			{
				text = text + " ::: " + children.Current.Identifier.ID;
			}
			MessageBox.Show(text, "Childs", MessageBoxButtons.OK);
		}

		public ArrayList GetSelectedObjects()
		{
			ArrayList arrayList = new ArrayList();
			if (model != null)
			{
				Tekla.Structures.Model.ModelObjectSelector modelObjectSelector = model.GetModelObjectSelector();
				ModelObjectEnumerator selectedObjects = new Tekla.Structures.Model.UI.ModelObjectSelector().GetSelectedObjects();
				selectedObjects.SelectInstances = false;
				try
				{
					while (selectedObjects.MoveNext())
					{
						arrayList.Add(selectedObjects.Current);
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.ToString());
				}
			}
			return arrayList;
		}

		public bool SelectObjectsInModel(ArrayList selectedObjects)
		{
			bool result = false;
			if (model != null)
			{
				try
				{
					ModelSelector.Select(selectedObjects);
					result = true;
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.ToString());
				}
			}
			return result;
		}

		public void SetLanguage()
		{
			string Value = string.Empty;
			TeklaStructuresSettings.GetAdvancedOption("XS_LANGUAGE", ref Value);
			switch (Value)
			{
			case "ENGLISH":
				currentLanguage = "enu";
				break;
			case "DUTCH":
				currentLanguage = "nld";
				break;
			case "FRENCH":
				currentLanguage = "fra";
				break;
			case "GERMAN":
				currentLanguage = "deu";
				break;
			case "ITALIAN":
				currentLanguage = "ita";
				break;
			case "SPANISH":
				currentLanguage = "esp";
				break;
			case "JAPANESE":
				currentLanguage = "jpn";
				break;
			case "CHINESE SIMPLIFIED":
				currentLanguage = "chs";
				break;
			case "CHINESE TRADITIONAL":
				currentLanguage = "cht";
				break;
			case "CZECH":
				currentLanguage = "csy";
				break;
			case "PORTUGUESE BRAZILIAN":
				currentLanguage = "ptb";
				break;
			case "HUNGARIAN":
				currentLanguage = "hun";
				break;
			case "POLISH":
				currentLanguage = "plk";
				break;
			case "RUSSIAN":
				currentLanguage = "rus";
				break;
			default:
				currentLanguage = "enu";
				break;
			}
		}

		public void ZoomToSelectedObjects()
		{
			try
			{
				TeklaStructures.ExecuteScript("akit.Callback(\"acmdZoomToSelected\", \"\", \"main_frame\");");
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
			}
		}
	}
}
