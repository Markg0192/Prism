using System;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace Prism
{
	public static class AutoFix
	{
		public static void PartNameAndClass()
		{
			foreach (PrismPart p in ModelChecker.IncorrectNameAndClass)
			{
				if (p.Part.Name.IndexOf("TEMP", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					p.Part.Class = GdomValues.TemporaryObjectClass;
					p.Part.Modify();
				}
				else
				{
					List<string> myClass = GdomValues.PartClass()[p.Part.Name];
					p.Part.Class = myClass[0];
					p.Part.Modify();
				}
			}
			PrismWarnings.ErrorsFixed(ModelChecker.IncorrectNameAndClass.Count);
		}

		public static void PartNameAndClass(List<PrismPart> prismParts)
		{
			foreach (PrismPart p in prismParts)
			{
				if (p.Part.Name.IndexOf("TEMP", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					p.Part.Class = GdomValues.TemporaryObjectClass;
					p.Part.Modify();
				}
				else
				{
					List<string> myClass = GdomValues.PartClass()[p.Part.Name];
					p.Part.Class = myClass[0];
					p.Part.Modify();
				}
			}
		}

		public static void MemberOrientation(SelectedObjects selectedObjects)
		{
			foreach (PrismPart b in ModelChecker.IncorrectOrientation)
			{
				//we need to change the beams in the selected parts list, not the incorrectorientation list, this linq sorts that out
				PrismPart matchingPart = selectedObjects.PrismParts.FirstOrDefault(part => part.Guid == b.Guid);

				Beam beam = matchingPart.Part as Beam;
				if (b.Part.Name == GdomValues.BeamName || b.Part.Name == GdomValues.RafterName || b.Part.Name == GdomValues.PortalRafterName || b.Part.Name == GdomValues.BraceName)
				{
					SwapHandles(beam);
					matchingPart.Part.Modify();
				}
				if (b.Part.Name == GdomValues.ColumnName)
				{
					if (beam.Position.Rotation == Position.RotationEnum.TOP)
					{
						beam.Position.Rotation = Position.RotationEnum.BELOW;
					}
					if (beam.Position.Rotation == Position.RotationEnum.BACK)
					{
						beam.Position.Rotation = Position.RotationEnum.FRONT;
					}
					if (beam.StartPoint.Z > beam.EndPoint.Z)
					{
						SwapHandles(beam);
					}
					beam.Modify();
				}
			}
			PrismWarnings.ErrorsFixed(ModelChecker.IncorrectOrientation.Count);
		}

		public static void MemberOrientation(List<PrismPart> prismParts)
		{
			foreach (PrismPart prismPart in prismParts)
			{
				Beam beam = prismPart.Part as Beam;
				if (prismPart.Part.Name == GdomValues.BeamName || prismPart.Part.Name == GdomValues.RafterName
					|| prismPart.Part.Name == GdomValues.PortalRafterName || prismPart.Part.Name == GdomValues.BraceName)
				{
					SwapHandles(beam);
					prismPart.Part.Modify();
				}
				if (prismPart.Part.Name == GdomValues.ColumnName)
				{
					if (beam.Position.Rotation == Position.RotationEnum.TOP)
					{
						beam.Position.Rotation = Position.RotationEnum.BELOW;
					}
					if (beam.Position.Rotation == Position.RotationEnum.BACK)
					{
						beam.Position.Rotation = Position.RotationEnum.FRONT;
					}
					if (beam.StartPoint.Z > beam.EndPoint.Z)
					{
						SwapHandles(beam);
					}
					beam.Modify();
				}
			}
		}

		private static void SwapHandles(Beam b)
		{
			Point startPoint = b.StartPoint;
			Point endPoint = b.EndPoint;
			b.StartPoint = endPoint;
			b.EndPoint = startPoint;

		}

		public static void ExecutionClass()
		{
			int myExcClass = PrismWarnings.ExecutionClassWarning();
			foreach (PrismPart p in ModelChecker.MissingExecutionClass)
			{
				p.Part.SetUserProperty(ModelUDA.ExcecutionClass(), myExcClass);
				p.Part.Modify();
			}
			PrismWarnings.ErrorsFixed(ModelChecker.MissingExecutionClass.Count);
		}

		public static void ExecutionClass(List<PrismPart> prismParts, int executionClass)
		{
			foreach (PrismPart prismPart in prismParts)
			{
				prismPart.Part.SetUserProperty(ModelUDA.ExcecutionClass(), executionClass);
				prismPart.Part.Modify();
			}
		}

		public static void AssemblyAndStartNumbers(List<PrismPart> parts)
		{
			foreach (PrismPart p in parts)
			{
				p.Part.AssemblyNumber.StartNumber = p.StartNumber;
				p.Part.PartNumber.StartNumber = p.StartNumber;
				p.Part.Modify();
			}
			PrismWarnings.ErrorsFixed(parts.Count);
		}

		public static void PartPhasing(List<PrismPart> parts)
		{
			foreach (PrismPart p in parts)
			{
				p.Part.SetPhase(p.Phase);
				p.Part.Modify();
			}
			PrismWarnings.ErrorsFixed(parts.Count);
		}

		public static void AssemblyToPartStartNumber(List<PrismPart> parts)
		{
			foreach (PrismPart p in parts)
			{
				p.Part.AssemblyNumber.StartNumber = p.StartNumber;
				p.Part.PartNumber.StartNumber = p.StartNumber;
				p.Part.Modify();
			}
		}

		public static void AssemblyToPartPhaseNumber(List<PrismPart> parts)
		{
			foreach (PrismPart p in parts)
			{
				p.Part.SetPhase(p.Phase);
				p.Part.Modify();
			}
		}
	}
}