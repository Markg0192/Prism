using System;
using System.Collections.Generic;
using Tekla.Structures.Model;

namespace Prism
{
	public class PrismPart
	{
		public PrismPart(ModelObject modelObject)
		{
			ModelObject = modelObject;
			Guid = modelObject.Identifier.GUID.ToString();
			Part p = modelObject as Part;
			Prelim = p == null ? "*Failed to get*" : p.GetPrelimMark();
			Part = p;
			string lotName = "";
			p.GetReportProperty("ASSEMBLY.LOT_NAME", ref lotName);
			LotName = lotName;
			Assembly = p.GetAssembly();
			IsMainPart = Assembly.GetMainPart().Identifier.GUID == p.Identifier.GUID;

			PartMark = Part.GetPartMark();
			double weight = 0;
			p.GetReportProperty(ModelUDA.Weight(), ref weight);
			Weight = weight;
			Profile = p.Profile.ProfileString;

			p.GetPhase(out Phase partPhase);
			Phase = partPhase;
			string drawingRevision = "";
			Part.GetReportProperty("ASSEMBLY.DRAWING.REVISION.MARK", ref drawingRevision);
			DrawingRevision = drawingRevision;
		}

		public PrismPart(string[] items, Model model)
		{
			Guid = Trim(items[1]);
			Prelim = Trim(items[2]);
			LotName = Trim(items[3]);
			IsMainPart = Trim(items[4]) == "1";
			Weight = Convert.ToDouble(Trim(items[5]));
			Profile = Trim(items[6]);
			Phase = GetPhaseFromString(Trim(items[7]));
			Name = Trim(items[8]);
			IsSeversafe = IsSeversafePart(Name);
			IsLocked = Trim(items[9]) == "1";
			IsFabsec = Profile.StartsWith("PG");
			IsFitting = Profile == "PL" || Profile == "RS" || Profile == "FL";
			PartMark = Trim(items[10]);
			NumbersOutOfDate = PartMark.Contains("?");
			Part = model.SelectModelObject(model.GetIdentifierByGUID(Guid)) as Part;
			if (IsMainPart) Assembly = Part.GetAssembly();
			ModelObject = Part;
			DrawingRevision = Trim(items[11]);
		}

		private string Trim(string s)
		{
			return s.TrimEnd(' ').TrimStart(' ');
		}

		public string DrawingRevision { get; set; }
		public bool NumbersOutOfDate { get; set; }
		public string PartMark { get; set; }
		public bool StartNumberDoesntMatch { get; set; }
		public bool PhaseDoesntMatchMain { get; set; }
		private string Name { get; set; }
		public string Profile { get; set; }
		public Assembly Assembly { get; set; }
		public string Prelim { get; set; }
		public string Guid { get; set; }
		public ModelObject ModelObject { get; set; }
		public string ErrorString { get; set; }
		public Phase Phase { get; set; }
		public Part Part { get; set; }
		public int StartNumber { get; set; }
		public bool IsSeversafe { get; set; }
		public bool IsLocked { get; set; }
		public bool IsFabsec { get; set; }
		public bool IsFitting { get;set; }
		public string LotName { get; set; }
		public bool IsMainPart { get; set; }
		public double Weight { get; set; }

		private Phase GetPhaseFromString(string phaseNumberString)
		{
			int phaseNumber;
			if (int.TryParse(phaseNumberString, out phaseNumber))
			{
				// Create a phase object
				Phase phase = new Phase();

				// Set the phase number to the object
				phase.PhaseNumber = phaseNumber;
				return phase;
			}
			return null;
		}

		private bool IsSeversafePart(string name)
		{
			if (name.Contains("SS-"))
			{
				return true;
			}
			return false;
		}
	}
}