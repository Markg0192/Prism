using System;

namespace Tekla.Structures.InpParser
{
	public class WrongFormatException : ApplicationException
	{
		public Token ExpectedToken { get; set; }

		public int LineNumber { get; set; }

		public Token ReceivedToken { get; set; }

		public WrongFormatException()
		{
		}

		public WrongFormatException(Token expectedToken)
		{
			ExpectedToken = expectedToken;
		}

		public WrongFormatException(Token receivedToken, Token expectedToken, int lineNumber)
		{
			ReceivedToken = receivedToken;
			ExpectedToken = expectedToken;
			LineNumber = lineNumber;
		}
	}
}
