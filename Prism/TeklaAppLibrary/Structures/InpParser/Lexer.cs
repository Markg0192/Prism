using System.IO;
using System.Text;

namespace Tekla.Structures.InpParser
{
	public class Lexer
	{
		public static Token GetLexeme(FileStream inputStream, ref int lineNumber)
		{
			int num = inputStream.ReadByte();
			if (num < 0)
			{
				return null;
			}
			char c = (char)num;
			if (char.IsSeparator(c) || c == '\n' || c == '\t' || c == '\r')
			{
				if (c == '\n')
				{
					lineNumber++;
				}
				return GetLexeme(inputStream, ref lineNumber);
			}
			CharType charType = CharType.NotDefined;
			int num2 = 0;
			StringBuilder stringBuilder = new StringBuilder();
			while (true)
			{
				if (num < 0 || char.IsSeparator(c) || c == '\n' || c == '\t' || c == '\r')
				{
					charType = CharType.Delimiter;
				}
				else if (char.IsNumber(c))
				{
					charType = CharType.Number;
				}
				else if (char.IsLetter(c) || c == '_')
				{
					charType = CharType.Letter;
				}
				else if (char.IsPunctuation(c))
				{
					charType = CharType.Punctuation;
				}
				switch (num2)
				{
				case 0:
					switch (charType)
					{
					case CharType.Delimiter:
						return GetLexeme(inputStream, ref lineNumber);
					default:
						stringBuilder = stringBuilder.Append(c);
						break;
					case CharType.NotDefined:
						break;
					}
					if (charType == CharType.Punctuation)
					{
						num2 = 1;
					}
					if (charType == CharType.Letter)
					{
						num2 = 2;
					}
					if (charType == CharType.Number)
					{
						num2 = 3;
					}
					break;
				case 1:
					if (charType == CharType.Punctuation && stringBuilder.ToString() == "/" && c == '*')
					{
						char c2 = ' ';
						char c3;
						do
						{
							c3 = c2;
							num = inputStream.ReadByte();
							if (num < 0)
							{
								throw new WrongFormatException();
							}
							c2 = (char)num;
							if (c2 == '\n')
							{
								lineNumber++;
							}
						}
						while (c3 != '*' || c2 != '/');
						stringBuilder = new StringBuilder();
						num2 = 0;
						break;
					}
					if (stringBuilder.ToString() == "\"")
					{
						stringBuilder = new StringBuilder();
						while (c != '"')
						{
							stringBuilder = stringBuilder.Append(c);
							num = inputStream.ReadByte();
							if (num < 0)
							{
								throw new WrongFormatException();
							}
							c = (char)num;
							if (c == '\n')
							{
								lineNumber++;
							}
						}
						return new Token(stringBuilder.ToString(), TokenType.String);
					}
					if (num >= 0)
					{
						inputStream.Seek(-1L, SeekOrigin.Current);
					}
					return new Token(stringBuilder.ToString(), TokenType.Punctuation);
				case 2:
					if (charType == CharType.Letter || charType == CharType.Number)
					{
						stringBuilder = stringBuilder.Append(c);
						num2 = 2;
					}
					if (charType == CharType.Punctuation || charType == CharType.Delimiter)
					{
						if (num >= 0)
						{
							inputStream.Seek(-1L, SeekOrigin.Current);
						}
						return new Token(stringBuilder.ToString(), TokenType.Identifier);
					}
					break;
				case 3:
					if (charType == CharType.Number || c == '.')
					{
						stringBuilder = stringBuilder.Append(c);
						num2 = 3;
					}
					else if (charType == CharType.Punctuation || charType == CharType.Letter || charType == CharType.Delimiter)
					{
						if (num >= 0)
						{
							inputStream.Seek(-1L, SeekOrigin.Current);
						}
						return new Token(stringBuilder.ToString(), TokenType.Number);
					}
					break;
				}
				num = inputStream.ReadByte();
				c = (char)num;
			}
		}
	}
}
