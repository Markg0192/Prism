using Tekla.Structures.Model;

namespace Tekla.Structures.Concrete
{
	public class RebarGroupConversionData
	{
		public int DepthLocation { get; set; }

		public Part FatherSlab { get; set; }

		public double Spacing { get; set; }

		public int SpliceSection { get; set; }

		public RebarGroup.RebarGroupStirrupTypeEnum StirrupType { get; set; }

		public RebarGroupConversionData()
		{
			DepthLocation = 0;
			FatherSlab = new ContourPlate();
			Spacing = 0.0;
			SpliceSection = 0;
			StirrupType = RebarGroup.RebarGroupStirrupTypeEnum.STIRRUP_TYPE_POLYGONAL;
		}
	}
}
