using System.Collections.Generic;

namespace Tekla.Structures.InpParser
{
	public class TSTabPageDefinition
	{
		private List<TSTabPageObject> objects = new List<TSTabPageObject>();

		public string Name { get; set; }

		public List<TSTabPageObject> Objects
		{
			get
			{
				return objects;
			}
			set
			{
				objects = value;
			}
		}

		public TSTabPageDefinition(string name)
		{
			Name = name;
		}
	}
}
