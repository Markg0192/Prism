using System.Drawing;
using Microsoft.Win32;

namespace Tekla.Structures
{
	internal class Registry : IRegistry
	{
		private RegistryKey currentVersion;

		private RegistryKey root;

		public RegistryKey CurrentVersion
		{
			get
			{
				if (currentVersion == null)
				{
					currentVersion = GetVersion(TeklaStructures.Version);
				}
				return currentVersion;
			}
		}

		public RegistryKey Root
		{
			get
			{
				if (root == null)
				{
					root = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Tekla\\Structures");
				}
				return root;
			}
		}

		public RegistryKey GetVersion(string version)
		{
			if (version.ToLower().Contains("99.") || version.ToLower().Contains("test package"))
			{
				version = "Next";
			}
			else if (version.IndexOf(' ') > 0)
			{
				version = version.Split(' ')[0];
			}
			if (version.Contains("."))
			{
				string[] array = version.Split('.');
				string text = array[0];
				string text2 = array[1];
				version = text + "." + text2;
			}
			return Root.OpenSubKey(version);
		}

		public Rectangle LoadDialogBounds(string dialogName)
		{
			return LoadDialogBounds(dialogName, TeklaStructures.Version);
		}

		public Rectangle LoadDialogBounds(string dialogName, string version)
		{
			/*using*/ RegistryKey registryKey = GetVersion(version);
			/*using*/ RegistryKey registryKey2 = registryKey.OpenSubKey("Dialogs");
			/*using*/ RegistryKey registryKey3 = registryKey2.OpenSubKey(dialogName);
			Rectangle empty = Rectangle.Empty;
			if (registryKey3 != null)
			{
				empty.X = (int)registryKey3.GetValue("x", 0);
				empty.Y = (int)registryKey3.GetValue("y", 0);
				empty.Width = (int)registryKey3.GetValue("width", 0);
				empty.Height = (int)registryKey3.GetValue("height", 0);
			}
			return empty;
		}

		public void SaveDialogBounds(string dialogName, Rectangle bounds)
		{
			SaveDialogBounds(dialogName, TeklaStructures.Version, bounds);
		}

		public void SaveDialogBounds(string dialogName, string version, Rectangle bounds)
		{
			/*using*/ RegistryKey registryKey = GetVersion(version);
			/*using*/ RegistryKey registryKey2 = registryKey.OpenSubKey("Dialogs", writable: true);
			/*using*/ RegistryKey registryKey3 = registryKey2.CreateSubKey(dialogName);
			registryKey3.SetValue("x", bounds.X);
			registryKey3.SetValue("y", bounds.Y);
			registryKey3.SetValue("width", bounds.Width);
			registryKey3.SetValue("height", bounds.Height);
		}
	}
}
