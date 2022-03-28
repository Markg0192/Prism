namespace Tekla.Structures.InpParser
{
	public class Picture : TSTabPageObject
	{
		public int Height { get; set; }

		public int Width { get; set; }

		public int X { get; set; }

		public int Y { get; set; }

		public Picture(string name)
			: base(name)
		{
		}
	}
}
