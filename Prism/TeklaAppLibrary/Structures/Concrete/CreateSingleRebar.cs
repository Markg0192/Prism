using System.Collections;
using Tekla.Structures.Model;

namespace Tekla.Structures.Concrete
{
	public class CreateSingleRebar
	{
		private const double OverhangEpsilon = 0.01;

		private readonly Reinforcement originalRebar;

		public CreateSingleRebar(Reinforcement newOriginalRebar)
		{
			originalRebar = newOriginalRebar;
		}

		public SingleRebar GetSingleRebar(ArrayList rebarPoints)
		{
			SingleRebar singleRebar = new SingleRebar();
			singleRebar.Polygon.Points = new ArrayList(rebarPoints);
			SingleRebar singleRebar2 = originalRebar as SingleRebar;
			if (singleRebar2 != null)
			{
				SetNewRebarPropertiesFromSingleRebar(singleRebar, singleRebar2);
			}
			else
			{
				RebarGroup rebarGroup = originalRebar as RebarGroup;
				if (rebarGroup != null)
				{
					SetNewRebarPropertiesFromRebarGroup(singleRebar, rebarGroup);
				}
			}
			if (singleRebar.EndPointOffsetType == Reinforcement.RebarOffsetTypeEnum.OFFSET_TYPE_COVER_THICKNESS && singleRebar.EndPointOffsetValue < 0.01)
			{
				singleRebar.EndPointOffsetValue = 0.01;
			}
			if (singleRebar.StartPointOffsetType == Reinforcement.RebarOffsetTypeEnum.OFFSET_TYPE_COVER_THICKNESS && singleRebar.StartPointOffsetValue < 0.01)
			{
				singleRebar.StartPointOffsetValue = 0.01;
			}
			return singleRebar;
		}

		private void SetNewRebarPropertiesFromRebarGroup(SingleRebar newRebar, RebarGroup originalRebarGroup)
		{
			newRebar.Father = originalRebarGroup.Father;
			newRebar.Name = originalRebarGroup.Name;
			newRebar.Size = originalRebarGroup.Size;
			newRebar.Grade = originalRebarGroup.Grade;
			newRebar.Class = originalRebarGroup.Class;
			newRebar.StartHook.Shape = originalRebarGroup.StartHook.Shape;
			newRebar.StartHook.Angle = originalRebarGroup.StartHook.Angle;
			newRebar.StartHook.Radius = originalRebarGroup.StartHook.Radius;
			newRebar.StartHook.Length = originalRebarGroup.StartHook.Length;
			newRebar.EndHook.Shape = originalRebarGroup.EndHook.Shape;
			newRebar.EndHook.Angle = originalRebarGroup.EndHook.Angle;
			newRebar.EndHook.Radius = originalRebarGroup.EndHook.Radius;
			newRebar.EndHook.Length = originalRebarGroup.EndHook.Length;
			newRebar.OnPlaneOffsets = new ArrayList();
			newRebar.FromPlaneOffset = 0.0;
			newRebar.EndPointOffsetValue = 0.0;
			newRebar.EndPointOffsetType = originalRebarGroup.EndPointOffsetType;
			newRebar.StartPointOffsetValue = 0.0;
			newRebar.StartPointOffsetType = originalRebarGroup.StartPointOffsetType;
			newRebar.NumberingSeries.Prefix = originalRebarGroup.NumberingSeries.Prefix;
			newRebar.NumberingSeries.StartNumber = originalRebarGroup.NumberingSeries.StartNumber;
			newRebar.RadiusValues = originalRebarGroup.RadiusValues;
		}

		private void SetNewRebarPropertiesFromSingleRebar(SingleRebar newRebar, SingleRebar originalSingleRebar)
		{
			newRebar.Father = originalSingleRebar.Father;
			newRebar.Name = originalSingleRebar.Name;
			newRebar.Size = originalSingleRebar.Size;
			newRebar.Grade = originalSingleRebar.Grade;
			newRebar.Class = originalSingleRebar.Class;
			newRebar.StartHook.Shape = originalSingleRebar.StartHook.Shape;
			newRebar.StartHook.Angle = originalSingleRebar.StartHook.Angle;
			newRebar.StartHook.Radius = originalSingleRebar.StartHook.Radius;
			newRebar.StartHook.Length = originalSingleRebar.StartHook.Length;
			newRebar.EndHook.Shape = originalSingleRebar.EndHook.Shape;
			newRebar.EndHook.Angle = originalSingleRebar.EndHook.Angle;
			newRebar.EndHook.Radius = originalSingleRebar.EndHook.Radius;
			newRebar.EndHook.Length = originalSingleRebar.EndHook.Length;
			newRebar.FromPlaneOffset = originalSingleRebar.FromPlaneOffset;
			newRebar.OnPlaneOffsets = originalSingleRebar.OnPlaneOffsets;
			newRebar.NumberingSeries.Prefix = originalSingleRebar.NumberingSeries.Prefix;
			newRebar.NumberingSeries.StartNumber = originalSingleRebar.NumberingSeries.StartNumber;
			newRebar.RadiusValues = originalSingleRebar.RadiusValues;
			newRebar.EndPointOffsetValue = originalSingleRebar.EndPointOffsetValue;
			newRebar.EndPointOffsetType = originalSingleRebar.EndPointOffsetType;
			newRebar.StartPointOffsetValue = originalSingleRebar.StartPointOffsetValue;
			newRebar.StartPointOffsetType = originalSingleRebar.StartPointOffsetType;
		}
	}
}
