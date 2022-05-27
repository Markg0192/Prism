using Tekla.Structures.Model;

namespace Tekla.Structures.Concrete
{
	public class SpliceData
	{
		public RebarSplice.RebarSpliceBarPositionsEnum BarPositions { get; set; }

		public double LappingLength { get; set; }

		public double LappingLengthFactor { get; set; }

		public int SpliceType { get; set; }
	}
}
