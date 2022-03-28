using System.Drawing;
using Microsoft.Win32;

namespace Tekla.Structures
{
	public interface IRegistry
	{
		RegistryKey CurrentVersion { get; }

		RegistryKey Root { get; }

		RegistryKey GetVersion(string version);

		Rectangle LoadDialogBounds(string dialogName);

		Rectangle LoadDialogBounds(string dialogName, string version);

		void SaveDialogBounds(string dialogName, Rectangle bounds);

		void SaveDialogBounds(string dialogName, string version, Rectangle bounds);
	}
}
