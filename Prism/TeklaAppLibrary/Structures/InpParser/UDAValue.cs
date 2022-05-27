namespace Tekla.Structures.InpParser
{
	public class UDAValue
	{
		public int DefaultSwitch { get; set; }

		public string Value { get; set; }

		public UDAValue(string attributeValue)
		{
			Value = attributeValue;
			DefaultSwitch = 0;
		}

		public UDAValue(string attributeValue, int defaultSwitch)
		{
			Value = attributeValue;
			DefaultSwitch = defaultSwitch;
		}
	}
}
