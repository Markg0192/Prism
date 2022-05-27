using System;
using System.Collections.Generic;
using System.IO;

namespace Tekla.Structures
{
	public static class SearchPath
	{
		public const char Separator = ';';

		private static readonly char[] Separators = new char[1] { ';' };

		public static string FindFile(string searchPath, string filename)
		{
			filename = Path.GetFileName(filename);
			if (!string.IsNullOrEmpty(filename))
			{
				foreach (string directory in GetDirectories(searchPath))
				{
					string text = Path.Combine(directory, filename);
					if (File.Exists(text))
					{
						return text;
					}
				}
			}
			return null;
		}

		public static IEnumerable<string> FindFiles(string searchPath, string searchPattern)
		{
			Dictionary<string, string> files = new Dictionary<string, string>();
			foreach (string directory in GetDirectories(searchPath))
			{
				if (!Directory.Exists(directory))
				{
					continue;
				}
				string[] files2 = Directory.GetFiles(directory, searchPattern);
				foreach (string path in files2)
				{
					string file = Path.GetFileName(path);
					if (!files.ContainsKey(file))
					{
						files.Add(file, path);
						yield return path;
					}
				}
			}
		}

		public static IEnumerable<string> FindFilesWithExtension(string searchPath, string extension)
		{
			if (string.IsNullOrEmpty(extension))
			{
				yield break;
			}
			extension = extension.ToLowerInvariant();
			if (!extension.StartsWith("."))
			{
				extension = "." + extension;
			}
			foreach (string path in FindFiles(searchPath, "*" + extension))
			{
				if (Path.GetExtension(path).ToLowerInvariant() == extension)
				{
					yield return path;
				}
			}
		}

		private static IEnumerable<string> GetDirectories(string searchPath)
		{
			if (!string.IsNullOrEmpty(searchPath))
			{
				string[] array = searchPath.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
				foreach (string directory in array)
				{
					yield return Path.GetFullPath(directory);
				}
			}
		}
	}
}
