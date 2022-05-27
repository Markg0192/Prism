using System.Collections.Generic;

namespace Tekla.Structures.InpParser
{
	public class UDA : TSTabPageObject
	{
		public string AttributeValueMax { get; set; }

		public string AttributeValueMin { get; set; }

		public CheckSwitchValues CheckSwitch { get; set; }

		public string FieldFormat { get; set; }

		public bool IsUnique { get; set; }

		public string LabelText { get; set; }

		public int PositionValue1 { get; set; }

		public int PositionValue2 { get; set; }

		public int PositionValue3 { get; set; }

		public bool SpecialFlag { get; set; }

		public string ToggleField { get; set; }

		public UDATypes ValueType { get; set; }

		public List<UDAValue> Values { get; set; }

		public UDA(bool isUnique)
		{
			IsUnique = isUnique;
		}

		public UDA(string name)
			: base(name)
		{
			IsUnique = false;
		}
	}
}
