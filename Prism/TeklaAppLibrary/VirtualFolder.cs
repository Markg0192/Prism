using System;
using System.Collections.Generic;
using System.IO;

namespace Tekla.Structures
{
	public class VirtualFolder
	{
		private readonly string folderPath;

		private readonly string searchPath;

		public string FolderName => Path.GetFileName(folderPath);

		public string FolderPath => folderPath;

		public VirtualFolder(string folderPath, string searchPath)
		{
			if (folderPath == null)
			{
				throw new ArgumentNullException("folderPath");
			}
			if (searchPath == null)
			{
				throw new ArgumentNullException("searchPath");
			}
			folderPath = folderPath.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			this.folderPath = folderPath;
			this.searchPath = folderPath + ";" + searchPath;
		}

		public string CreateWritableCopy(string filename)
		{
			string writablePath = GetWritablePath(filename);
			if (!File.Exists(writablePath))
			{
				string text = FindFile(filename);
				if (text != null)
				{
					File.Copy(text, writablePath, overwrite: true);
				}
			}
			return writablePath;
		}

		public string FindFile(string filename)
		{
			return SearchPath.FindFile(searchPath, filename);
		}

		public IEnumerable<string> FindFiles(string pattern)
		{
			return SearchPath.FindFiles(searchPath, pattern);
		}

		public IEnumerable<string> FindFilesWithExtension(string extension)
		{
			return SearchPath.FindFilesWithExtension(searchPath, extension);
		}

		public string GetWritablePath(string filename)
		{
			return Path.Combine(folderPath, Path.GetFileName(filename));
		}

		public bool IsWritable(string filename)
		{
			return File.Exists(GetWritablePath(filename));
		}
	}
}
