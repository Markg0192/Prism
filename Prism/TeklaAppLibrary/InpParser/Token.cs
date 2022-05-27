namespace Tekla.Structures.InpParser
{
	public class Token
	{
		private readonly TokenType tokenType;

		private readonly string value;

		public TokenType TokenType => tokenType;

		public string Value => value;

		public Token(string stringValue, TokenType type)
		{
			value = stringValue;
			tokenType = type;
		}
	}
}
