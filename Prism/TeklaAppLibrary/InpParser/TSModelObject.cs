using System.Collections.Generic;

namespace Tekla.Structures.InpParser
{
	public class TSModelObject
	{
		public List<UDA> Attributes { get; set; }

		public int DummyNumber { get; set; }

		public bool Modify { get; set; }

		public string Name { get; set; }

		public List<TSTabPageDeclaration> TabPages { get; set; }

		public TSModelObjectTypes Type { get; set; }

		public TSModelObject(TSModelObjectTypes type)
		{
			Type = type;
		}
	}
}
