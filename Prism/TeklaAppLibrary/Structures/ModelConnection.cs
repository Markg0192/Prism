#define DEBUG
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using Tekla.Structures.Dialog;
using Tekla.Structures.InpParser;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Model.UI;

namespace Tekla.Structures
{
	internal class ModelConnection : IModel, IConnection, ITransaction, IRunMacro, IPicker, ISelection, ISelectObject, IEnvironment
	{
		private const int LongOperation = 60000;

		private static readonly char[] PathSeparators = new char[1] { ';' };

		private Tekla.Structures.Model.Model connection;

		private Events events;

		private bool eventsRegistered;

		private Localization localization;

		public IEnumerable<object> AllObjects
		{
			get
			{
				List<object> snapshot = new List<object>();
				try
				{
					if (IsActive)
					{
						SeparateThread.Execute(60000, delegate
						{
							foreach (object allObject in connection.GetModelObjectSelector().GetAllObjects())
							{
								if (allObject != null)
								{
									snapshot.Add(allObject);
								}
							}
						});
					}
				}
				catch (Exception value)
				{
					Debug.WriteLine(value);
					snapshot.Clear();
				}
				return snapshot;
			}
		}

		public IEnumerable<string> CloningTemplateModelFolders
		{
			get
			{
				string[] array = this["XS_CLONING_TEMPLATE_DIRECTORY"].Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
				foreach (string location in array)
				{
					string[] directories = Directory.GetDirectories(location);
					for (int j = 0; j < directories.Length; j++)
					{
						yield return directories[j];
					}
				}
			}
		}

		public IEnumerable<string> CompanyFolders => this["XS_FIRM"].Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);

		public Configuration Configuration
		{
			get
			{
				if (IsActive)
				{
					return SeparateThread.Execute(() => (Configuration)ModuleManager.Configuration);
				}
				return Configuration.Viewer;
			}
		}

		public CultureInfo CultureInfo => Language switch
		{
			"ENGLISH" => new CultureInfo("en"), 
			"DUTCH" => new CultureInfo("nl"), 
			"FRENCH" => new CultureInfo("fr"), 
			"GERMAN" => new CultureInfo("de"), 
			"ITALIAN" => new CultureInfo("it"), 
			"SPANISH" => new CultureInfo("es"), 
			"JAPANESE" => new CultureInfo("ja"), 
			"CHINESE SIMPLIFIED" => new CultureInfo("zh-CHS"), 
			"CHINESE TRADITIONAL" => new CultureInfo("zh-CHT"), 
			"CZECH" => new CultureInfo("cs"), 
			"PORTUGUESE BRAZILIAN" => new CultureInfo("pt-BR"), 
			"HUNGARIAN" => new CultureInfo("hu"), 
			"POLISH" => new CultureInfo("pl"), 
			"RUSSIAN" => new CultureInfo("ru"), 
			_ => new CultureInfo("en"), 
		};

		public IEnumerable<string> DrawingMacros
		{
			get
			{
				string folder = Path.Combine(MacrosFolder, "drawings");
				if (!Directory.Exists(folder))
				{
					yield break;
				}
				string[] files = Directory.GetFiles(folder, "*.cs");
				foreach (string path in files)
				{
					if (path.EndsWith(".cs"))
					{
						yield return Path.GetFileNameWithoutExtension(path);
					}
				}
			}
		}

		public ModelFolder Folder
		{
			get
			{
				if (IsActive)
				{
					ModelInfo modelInfo = SeparateThread.Execute((SeparateThread.Action<ModelInfo>)connection.GetInfo);
					if (modelInfo != null)
					{
						return new ModelFolder(modelInfo.ModelPath, SearchPath);
					}
				}
				return new ModelFolder(string.Empty, string.Empty);
			}
		}

		public bool IsActive => connection != null && connection.GetConnectionStatus();

		public string Language => this["XS_LANGUAGE"];

		public Localization Localization
		{
			get
			{
				//IL_001e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Expected O, but got Unknown
				if (IsActive && localization == null)
				{
					localization = new Localization();
					try
					{
						switch (Language)
						{
						case "ENGLISH":
							localization.set_Language("enu");
							break;
						case "DUTCH":
							localization.set_Language("nld");
							break;
						case "FRENCH":
							localization.set_Language("fra");
							break;
						case "GERMAN":
							localization.set_Language("deu");
							break;
						case "ITALIAN":
							localization.set_Language("ita");
							break;
						case "SPANISH":
							localization.set_Language("esp");
							break;
						case "JAPANESE":
							localization.set_Language("jpn");
							break;
						case "CHINESE SIMPLIFIED":
							localization.set_Language("chs");
							break;
						case "CHINESE TRADITIONAL":
							localization.set_Language("cht");
							break;
						case "CZECH":
							localization.set_Language("csy");
							break;
						case "PORTUGUESE BRAZILIAN":
							localization.set_Language("ptb");
							break;
						case "HUNGARIAN":
							localization.set_Language("hun");
							break;
						case "POLISH":
							localization.set_Language("plk");
							break;
						case "RUSSIAN":
							localization.set_Language("rus");
							break;
						default:
							localization.set_Language("enu");
							break;
						}
						localization.LoadFile(Localization.get_DefaultLocalizationFile());
					}
					catch (Exception value)
					{
						Debug.WriteLine(value);
					}
				}
				return localization;
			}
		}

		public string MacrosFolder => MacrosFolders.FirstOrDefault();

		public IEnumerable<string> MacrosFolders
		{
			get
			{
				string text = this["XS_MACRO_DIRECTORY"];
				string text2 = text.Replace("\\\\", "\\");
				return text2.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
			}
		}

		public IEnumerable<string> ModelMacros
		{
			get
			{
				string folder = Path.Combine(MacrosFolder, "modeling");
				if (!Directory.Exists(folder))
				{
					yield break;
				}
				string[] files = Directory.GetFiles(folder, "*.cs");
				foreach (string path in files)
				{
					if (path.EndsWith(".cs"))
					{
						yield return Path.GetFileNameWithoutExtension(path);
					}
				}
			}
		}

		public string Name
		{
			get
			{
				if (IsActive)
				{
					ModelInfo modelInfo = SeparateThread.Execute((SeparateThread.Action<ModelInfo>)connection.GetInfo);
					if (modelInfo != null)
					{
						return modelInfo.ModelName;
					}
				}
				return string.Empty;
			}
		}

		public IEnumerable<Dictionary<string, string>> OptionTypeUDAIndexAndValue
		{
			get
			{
				Parser parser = new Parser();
				parser.ValidationOn = false;
				try
				{
					parser.Parse(Path.Combine(this["XS_INP"], ModelFolder.ObjectDefinitionFile), overrideExisting: false);
				}
				catch (Exception value)
				{
					Debug.WriteLine(value);
				}
				foreach (string companyFolder in CompanyFolders)
				{
					try
					{
						parser.Parse(Path.Combine(companyFolder, ModelFolder.ObjectDefinitionFile), overrideExisting: true);
					}
					catch (Exception value2)
					{
						Debug.WriteLine(value2);
					}
				}
				foreach (string projectFolder in ProjectFolders)
				{
					try
					{
						parser.Parse(Path.Combine(projectFolder, ModelFolder.ObjectDefinitionFile), overrideExisting: true);
					}
					catch (Exception value3)
					{
						Debug.WriteLine(value3);
					}
				}
				foreach (string systemFolder in SystemFolders)
				{
					try
					{
						parser.Parse(Path.Combine(systemFolder, ModelFolder.ObjectDefinitionFile), overrideExisting: true);
					}
					catch (Exception value4)
					{
						Debug.WriteLine(value4);
					}
				}
				try
				{
					parser.Parse(Path.Combine(Folder.FolderPath, ModelFolder.ObjectDefinitionFile), overrideExisting: true);
				}
				catch (Exception value5)
				{
					Debug.WriteLine(value5);
				}
				return parser.FindUdas(UDATypes.Option).ConvertAll(delegate(UDA item)
				{
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					int num = 0;
					dictionary.Add("Name", item.Name);
					foreach (UDAValue value6 in item.Values)
					{
						if (value6.DefaultSwitch == 2)
						{
							num = -1;
						}
						dictionary.Add(num.ToString(), value6.Value);
						num++;
					}
					return dictionary;
				});
			}
		}

		public IEnumerable<string> ProjectFolders => this["XS_PROJECT"].Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);

		public string SearchPath
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (string projectFolder in ProjectFolders)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(';');
					}
					stringBuilder.Append(projectFolder);
				}
				foreach (string companyFolder in CompanyFolders)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(';');
					}
					stringBuilder.Append(companyFolder);
				}
				foreach (string systemFolder in SystemFolders)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(';');
					}
					stringBuilder.Append(systemFolder);
				}
				return stringBuilder.ToString();
			}
		}

		public IEnumerable<object> SelectedObjects
		{
			get
			{
				List<object> snapshot = new List<object>();
				try
				{
					if (IsActive)
					{
						SeparateThread.Execute(60000, delegate
						{
							foreach (object selectedObject in new Tekla.Structures.Model.UI.ModelObjectSelector().GetSelectedObjects())
							{
								if (selectedObject != null)
								{
									snapshot.Add(selectedObject);
								}
							}
							foreach (object item in Enumerable.From(ViewHandler.GetSelectedViews()))
							{
								if (item != null)
								{
									snapshot.Add(item);
								}
							}
						});
					}
				}
				catch (Exception value)
				{
					Debug.WriteLine(value);
					snapshot.Clear();
				}
				return snapshot;
			}
		}

		public IEnumerable<string> SystemFolders => this["XS_SYSTEM"].Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);

		public bool UseUSImperialUnitsInInput => this["XS_IMPERIAL_INPUT"] == "TRUE";

		public IEnumerable<string> UserDefinedAttributes
		{
			get
			{
				Parser parser = new Parser();
				parser.ValidationOn = false;
				try
				{
					parser.Parse(Path.Combine(this["XS_INP"], ModelFolder.ObjectDefinitionFile), overrideExisting: false);
				}
				catch (Exception value)
				{
					Debug.WriteLine(value);
				}
				foreach (string companyFolder in CompanyFolders)
				{
					try
					{
						parser.Parse(Path.Combine(companyFolder, ModelFolder.ObjectDefinitionFile), overrideExisting: true);
					}
					catch (Exception value2)
					{
						Debug.WriteLine(value2);
					}
				}
				foreach (string projectFolder in ProjectFolders)
				{
					try
					{
						parser.Parse(Path.Combine(projectFolder, ModelFolder.ObjectDefinitionFile), overrideExisting: true);
					}
					catch (Exception value3)
					{
						Debug.WriteLine(value3);
					}
				}
				foreach (string systemFolder in SystemFolders)
				{
					try
					{
						parser.Parse(Path.Combine(systemFolder, ModelFolder.ObjectDefinitionFile), overrideExisting: true);
					}
					catch (Exception value4)
					{
						Debug.WriteLine(value4);
					}
				}
				try
				{
					parser.Parse(Path.Combine(Folder.FolderPath, ModelFolder.ObjectDefinitionFile), overrideExisting: true);
				}
				catch (Exception value5)
				{
					Debug.WriteLine(value5);
				}
				return parser.FindUdas(UDATypes.String).ConvertAll((UDA item) => item.Name);
			}
		}

		public IEnumerable<string> UserDefinedAttributesOptionType
		{
			get
			{
				Parser parser = new Parser();
				parser.ValidationOn = false;
				try
				{
					parser.Parse(Path.Combine(this["XS_INP"], ModelFolder.ObjectDefinitionFile), overrideExisting: false);
				}
				catch (Exception value)
				{
					Debug.WriteLine(value);
				}
				foreach (string companyFolder in CompanyFolders)
				{
					try
					{
						parser.Parse(Path.Combine(companyFolder, ModelFolder.ObjectDefinitionFile), overrideExisting: true);
					}
					catch (Exception value2)
					{
						Debug.WriteLine(value2);
					}
				}
				foreach (string projectFolder in ProjectFolders)
				{
					try
					{
						parser.Parse(Path.Combine(projectFolder, ModelFolder.ObjectDefinitionFile), overrideExisting: true);
					}
					catch (Exception value3)
					{
						Debug.WriteLine(value3);
					}
				}
				foreach (string systemFolder in SystemFolders)
				{
					try
					{
						parser.Parse(Path.Combine(systemFolder, ModelFolder.ObjectDefinitionFile), overrideExisting: true);
					}
					catch (Exception value4)
					{
						Debug.WriteLine(value4);
					}
				}
				try
				{
					parser.Parse(Path.Combine(Folder.FolderPath, ModelFolder.ObjectDefinitionFile), overrideExisting: true);
				}
				catch (Exception value5)
				{
					Debug.WriteLine(value5);
				}
				return parser.FindUdas(UDATypes.Option).ConvertAll((UDA item) => item.Name);
			}
		}

		public string Version
		{
			get
			{
				if (IsActive)
				{
					return SeparateThread.Execute((SeparateThread.Action<string>)TeklaStructuresInfo.GetCurrentProgramVersion);
				}
				return string.Empty;
			}
		}

		public string this[string variableName]
		{
			get
			{
				if (IsActive)
				{
					string value = string.Empty;
					if (SeparateThread.Execute(() => TeklaStructuresSettings.GetAdvancedOption(variableName, ref value)))
					{
						return value;
					}
				}
				return string.Empty;
			}
		}

		public event EventHandler ApplicationClosed
		{
			add
			{
				UnregisterEvents();
				ApplicationClosed_Event += value;
				RegisterEvents();
			}
			remove
			{
				UnregisterEvents();
				ApplicationClosed_Event -= value;
				RegisterEvents();
			}
		}

		public event EventHandler Changed
		{
			add
			{
				UnregisterEvents();
				Changed_Event += value;
				RegisterEvents();
			}
			remove
			{
				UnregisterEvents();
				Changed_Event -= value;
				RegisterEvents();
			}
		}

		public event EventHandler ChangesCommitted;

		public event EventHandler ChangesDiscarded;

		public event EventHandler Loaded
		{
			add
			{
				UnregisterEvents();
				Loaded_Event += value;
				RegisterEvents();
			}
			remove
			{
				UnregisterEvents();
				Loaded_Event -= value;
				RegisterEvents();
			}
		}

		public event EventHandler Numbering
		{
			add
			{
				UnregisterEvents();
				Numbering_Event += value;
				RegisterEvents();
			}
			remove
			{
				UnregisterEvents();
				Numbering_Event -= value;
				RegisterEvents();
			}
		}

		public event EventHandler Saved
		{
			add
			{
				UnregisterEvents();
				Saved_Event += value;
				RegisterEvents();
			}
			remove
			{
				UnregisterEvents();
				Saved_Event -= value;
				RegisterEvents();
			}
		}

		public event EventHandler SelectionChanged
		{
			add
			{
				UnregisterEvents();
				SelectionChanged_Event += value;
				RegisterEvents();
			}
			remove
			{
				UnregisterEvents();
				SelectionChanged_Event -= value;
				RegisterEvents();
			}
		}

		private event EventHandler ApplicationClosed_Event;

		private event EventHandler Changed_Event;

		private event EventHandler Loaded_Event;

		private event EventHandler Numbering_Event;

		private event EventHandler Saved_Event;

		private event EventHandler SelectionChanged_Event;

		public void CommitChanges(IEnumerable<object> objects)
		{
			if (!IsActive)
			{
				return;
			}
			try
			{
				foreach (object @object in objects)
				{
					try
					{
						(@object as ModelObject)?.Modify();
						(@object as View)?.Modify();
					}
					catch (NotImplementedException)
					{
					}
				}
				if (SeparateThread.Execute((SeparateThread.Action<bool>)connection.CommitChanges) && this.ChangesCommitted != null)
				{
					this.ChangesCommitted(this, EventArgs.Empty);
				}
			}
			catch (RemotingException ex2)
			{
				Debug.WriteLine(ex2.ToString());
				DiscardChanges(objects);
			}
		}

		public bool Connect()
		{
			try
			{
				if (connection == null)
				{
					connection = new Tekla.Structures.Model.Model();
				}
				if (events == null)
				{
					events = new Events();
					RegisterEvents();
				}
				if (Localization == null)
				{
					throw new NotSupportedException("Localization is not available.");
				}
			}
			catch (Exception value)
			{
				Debug.WriteLine(value);
				Disconnect();
			}
			return IsActive;
		}

		public void DiscardChanges(IEnumerable<object> objects)
		{
			if (this.ChangesDiscarded != null)
			{
				this.ChangesDiscarded(this, EventArgs.Empty);
			}
		}

		public void Disconnect()
		{
			try
			{
				UnregisterEvents();
			}
			catch (Exception value)
			{
				Debug.WriteLine(value);
			}
			finally
			{
				events = null;
				connection = null;
				localization = null;
			}
		}

		public void LoadLocalizationFile(string fileName)
		{
			if (IsActive)
			{
				try
				{
					Localization.LoadFile(Path.Combine(this["XS_DIR"], "messages\\DotAppsStrings\\" + fileName));
				}
				catch (ConstraintException innerException)
				{
					throw new InvalidDataException("The localization file contains duplicate keys or keys that have already been loaded.", innerException);
				}
			}
		}

		public object PickObject(string prompt)
		{
			if (IsActive)
			{
				try
				{
					return new Picker().PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, prompt);
				}
				catch (RemotingException ex)
				{
					Debug.WriteLine(ex.ToString());
				}
				catch (ApplicationException)
				{
				}
			}
			return null;
		}

		public void RunMacro(string macroName)
		{
			if (!IsActive)
			{
				return;
			}
			if (!Path.HasExtension(macroName))
			{
				macroName += ".cs";
			}
			SeparateThread.Execute(delegate
			{
				while (Operation.IsMacroRunning())
				{
					Thread.Sleep(100);
				}
				Operation.RunMacro(macroName);
			});
		}

		public object SelectObjectByIdentifier(Identifier identifier)
		{
			if (IsActive)
			{
				return SeparateThread.Execute(() => connection.SelectModelObject(identifier));
			}
			return null;
		}

		private void OnApplicationClosed()
		{
			this.ApplicationClosed_Event.BeginInvoke(this, EventArgs.Empty, null, null);
		}

		private void OnChanged()
		{
			this.Changed_Event.BeginInvoke(this, EventArgs.Empty, null, null);
		}

		private void OnLoaded()
		{
			this.Loaded_Event.BeginInvoke(this, EventArgs.Empty, null, null);
		}

		private void OnNumbering()
		{
			this.Numbering_Event.BeginInvoke(this, EventArgs.Empty, null, null);
		}

		private void OnSaved()
		{
			this.Saved_Event.BeginInvoke(this, EventArgs.Empty, null, null);
		}

		private void OnSelectionChanged()
		{
			this.SelectionChanged_Event.BeginInvoke(this, EventArgs.Empty, null, null);
		}

		private void RegisterEvents()
		{
			if (events == null || eventsRegistered)
			{
				return;
			}
			SeparateThread.Execute(delegate
			{
				int num = 0;
				if (this.ApplicationClosed_Event != null)
				{
					events.TeklaStructuresExit += OnApplicationClosed;
					num++;
				}
				if (this.Numbering_Event != null)
				{
					events.Numbering += OnNumbering;
					num++;
				}
				if (this.Loaded_Event != null)
				{
					events.ModelLoad += OnLoaded;
					num++;
				}
				if (this.Saved_Event != null)
				{
					events.ModelSave += OnSaved;
					num++;
				}
				if (this.SelectionChanged_Event != null)
				{
					events.SelectionChange += OnSelectionChanged;
					num++;
				}
				if (this.Changed_Event != null)
				{
					events.ModelChanged += OnChanged;
					num++;
				}
				if (num > 0)
				{
					events.Register();
					eventsRegistered = true;
				}
			});
		}

		private void UnregisterEvents()
		{
			if (events == null || !eventsRegistered)
			{
				return;
			}
			SeparateThread.Execute(delegate
			{
				eventsRegistered = false;
				events.UnRegister();
				if (this.ApplicationClosed_Event != null)
				{
					events.TeklaStructuresExit -= OnApplicationClosed;
				}
				if (this.Numbering_Event != null)
				{
					events.Numbering -= OnNumbering;
				}
				if (this.Loaded_Event != null)
				{
					events.ModelLoad -= OnLoaded;
				}
				if (this.Saved_Event != null)
				{
					events.ModelSave -= OnSaved;
				}
				if (this.SelectionChanged_Event != null)
				{
					events.SelectionChange -= OnSelectionChanged;
				}
				if (this.Changed_Event != null)
				{
					events.ModelChanged -= OnChanged;
				}
			});
		}
	}
}
