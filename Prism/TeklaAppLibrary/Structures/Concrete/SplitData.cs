namespace Tekla.Structures.Concrete
{
	public class SplitData
	{
		public int DepthLocation { get; set; }

		public double LappingLength { get; set; }

		public double MaxLength { get; set; }

		public double MinSplitDistance { get; set; }

		public double SpliceOffset { get; set; }

		public int SpliceSection { get; set; }

		public int SpliceSymmetry { get; set; }

		public int SpliceType { get; set; }

		public SplitData()
		{
			DepthLocation = 0;
			LappingLength = 0.0;
			MaxLength = 0.0;
			MinSplitDistance = 0.0;
			SpliceOffset = 0.0;
			SpliceSection = 0;
			SpliceSymmetry = 0;
			SpliceType = 0;
		}
	}
}
