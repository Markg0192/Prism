namespace Tekla.Structures.InpParser
{
	public class EOFException : WrongFormatException
	{
		public bool IsCorrectEnd { get; set; }

		public EOFException()
		{
		}

		public EOFException(Token expectedToken)
			: base(expectedToken)
		{
		}

		public EOFException(bool correctEnd)
		{
			IsCorrectEnd = correctEnd;
		}
	}
}
