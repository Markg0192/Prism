using System;
using System.Collections.Generic;
using System.Linq;

namespace Prism.Validation
{
	public static class ValidateDrawingSelection
	{
		public static List<PrismPart> ValidateSelection(List<PrismDrawing> myDrawings, List<PrismPart> myParts)
		{
			return RunSelectionValidation(myDrawings, myParts);
		}

		private static List<PrismPart> RunSelectionValidation(List<PrismDrawing> myDrawings, List<PrismPart> myParts)
		{
			// Build a HashSet of all drawing names (with case-insensitive comparer if needed)
			HashSet<string> drawingNames = new HashSet<string>(
				myDrawings.Select(d => d.DrawingNumber),
				StringComparer.OrdinalIgnoreCase
			);

			// Get all parts that do NOT have a matching drawing name
			return myParts.Where(p => !drawingNames.Contains(p.PartMark)).ToList();
		}
	}
}