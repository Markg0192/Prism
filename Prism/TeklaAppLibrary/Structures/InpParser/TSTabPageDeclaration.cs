namespace Tekla.Structures.InpParser
{
	public class TSTabPageDeclaration
	{
		public TSTabPageDefinition Definition { get; set; }

		public int Index { get; set; }

		public string Name { get; set; }

		public string Prompt { get; set; }

		public TSTabPageDeclaration()
		{
		}

		public TSTabPageDeclaration(string name)
		{
			Name = name;
		}
	}
}
