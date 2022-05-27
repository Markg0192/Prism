using System.Collections.Generic;
using System.Globalization;
using Tekla.Structures.Dialog;

namespace Tekla.Structures
{
	public interface IEnvironment
	{
		IEnumerable<string> CloningTemplateModelFolders { get; }

		IEnumerable<string> CompanyFolders { get; }

		CultureInfo CultureInfo { get; }

		IEnumerable<string> DrawingMacros { get; }

		string Language { get; }

		Localization Localization { get; }

		string MacrosFolder { get; }

		IEnumerable<string> ModelMacros { get; }

		IEnumerable<Dictionary<string, string>> OptionTypeUDAIndexAndValue { get; }

		IEnumerable<string> ProjectFolders { get; }

		string SearchPath { get; }

		IEnumerable<string> SystemFolders { get; }

		bool UseUSImperialUnitsInInput { get; }

		IEnumerable<string> UserDefinedAttributes { get; }

		IEnumerable<string> UserDefinedAttributesOptionType { get; }

		string this[string variableName] { get; }

		void LoadLocalizationFile(string fileName);
	}
}
