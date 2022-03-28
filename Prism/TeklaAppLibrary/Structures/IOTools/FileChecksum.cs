using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Tekla.Structures.IOTools
{
	public class FileChecksum
	{
		private readonly MD5 md5 = MD5.Create();

		public string Calculate(string file)
		{
			return CalculateChecksum(file);
		}

		private string CalculateChecksum(string file)
		{
			using FileStream inputStream = File.OpenRead(file);
			byte[] array = md5.ComputeHash(inputStream);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			return stringBuilder.ToString();
		}
	}
}
