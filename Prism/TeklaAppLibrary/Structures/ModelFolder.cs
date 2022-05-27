using System.IO;

namespace Tekla.Structures
{
	public class ModelFolder : VirtualFolder
	{
		public static readonly string AssemblyFileExtension = ".ad";

		public static readonly string AttributesFolderName = "attributes";

		public static readonly string CastUnitFileExtension = ".cud";

		public static readonly string DatabaseFileExtension = ".db1";

		public static readonly string DrawingsFolderName = "drawings";

		public static readonly string ExtendedRulesetFileExtension = ".xdproc";

		public static readonly string GeneralArrangementFileExtension = ".gd";

		public static readonly string ObjectDefinitionFile = "objects.inp";

		public static readonly string ObjectSettingsFileExtension = ".objectSettings";

		public static readonly string RulesetFileExtension = ".dproc";

		public static readonly string SelectFilterFileExtension = ".SObjGrp";

		public static readonly string SinglePartFileExtension = ".wd";

		public static readonly string ViewFilterFileExtension = ".vf";

		private readonly VirtualFolder attributesFolder;

		private readonly VirtualFolder drawingsFolder;

		public VirtualFolder AttributesFolder => attributesFolder;

		public VirtualFolder DrawingsFolder => drawingsFolder;

		public ModelFolder(string folderPath, string searchPath)
			: base(folderPath, searchPath)
		{
			drawingsFolder = new VirtualFolder(Path.Combine(folderPath, DrawingsFolderName), searchPath);
			attributesFolder = new VirtualFolder(Path.Combine(folderPath, AttributesFolderName), searchPath);
		}

		public static bool ContainsModelDatabase(string modelFolder)
		{
			string databaseFile = GetDatabaseFile(modelFolder);
			return !string.IsNullOrEmpty(databaseFile) && File.Exists(databaseFile);
		}

		private static string GetDatabaseFile(string modelFolder)
		{
			string modelName = GetModelName(modelFolder);
			if (!string.IsNullOrEmpty(modelName))
			{
				return Path.Combine(modelFolder, modelName + DatabaseFileExtension);
			}
			return string.Empty;
		}

		private static string GetModelName(string modelFolder)
		{
			if (!string.IsNullOrEmpty(modelFolder))
			{
				return Path.GetFileName(modelFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
			}
			return string.Empty;
		}
	}
}
