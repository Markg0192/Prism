using System;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Model;
using static Prism.Enums;
using ModelObject = Tekla.Structures.Model.ModelObject;
using Part = Tekla.Structures.Model.Part;

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

			SherwinDft = GetStringProperty(p, ModelUDA.FireDFT());
			SherwinWft = GetStringProperty(p, ModelUDA.FireWFT());
			HempelDft = GetStringProperty(p, ModelUDA.HempelFireDFT());
			HempelWft = GetStringProperty(p, ModelUDA.HempelFireWFT());
			Finish = Part.Finish;
		}

		public PrismPart(string[] items, Model model, StageTypes stageType)
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
			if (IsLocked) PartErrors.Add(Enums.Error.PartLocked);

			IsFabsec = Profile.StartsWith("PG");
			IsFitting = Profile == "PL" || Profile == "RS" || Profile == "FL";
			PartMark = Trim(items[10]);
			NumbersOutOfDate = PartMark.Contains("?");
			if (NumbersOutOfDate && (stageType == StageTypes.FAB || stageType == StageTypes.RocketPacket)) PartErrors.Add(Enums.Error.NumberingNotUpToDate);

			Part = model.SelectModelObject(model.GetIdentifierByGUID(Guid)) as Part;
			
			if (IsMainPart) Assembly = Part.GetAssembly();
			ModelObject = Part;
			DrawingRevision = Trim(items[11]); // redundant now because of items 18 which works for both types of drawings. Keeping it so not to mess to much with items order...

			SherwinDft = Trim(items[12]);
			SherwinWft = Trim(items[13]);
			HempelDft = Trim(items[14]);
			HempelWft = Trim(items[15]);
			Finish = Trim(items[16]);
			HasDrawing = Trim(items[17]) != "0";

			string trimmedRevNote = Trim(items[18]);
			string trimmedFolder = Trim(items[19]);

			DrawingFolder drawingFolder;

			HasProperlyAssignedFolder = TryGetDrawingFolder(trimmedFolder, out drawingFolder);

			bool revisionNotRequired =
				HasProperlyAssignedFolder &&
				(drawingFolder == DrawingFolder.NotRequired ||
				 drawingFolder == DrawingFolder.AssNotRequired);

			HasRevision =
				revisionNotRequired ||
				(trimmedRevNote != "0" && trimmedRevNote != "");

			AssemblyPrefix = Trim(items[20]);
			PartPrefix = Trim(items[21]);
			IsAbnormal = Trim(items[22]) == "Yes";
		}

		private static bool TryGetDrawingFolder(string value, out DrawingFolder folder)
		{
			folder = DrawingFolder.Default;

			if (string.IsNullOrWhiteSpace(value))
				return false;

			string normalised = NormaliseFolderName(value);

			// Remove common descriptive word
			if (normalised.EndsWith("Folder", StringComparison.OrdinalIgnoreCase))
			{
				normalised = normalised.Substring(0, normalised.Length - "Folder".Length);
			}

			return System.Enum.TryParse(normalised, true, out folder);
		}

		private static string NormaliseFolderName(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return string.Empty;

			return new string(value
				.Where(char.IsLetterOrDigit)
				.ToArray());
		}

		private string Trim(string s)
		{
			return s.TrimEnd(' ').TrimStart(' ');
		}

		public bool HasProperlyAssignedFolder { get; set; }
		public bool HasDrawing { get; set; }
		public bool HasRevision { get; set; }
		public string Finish {  get; set; }
		public string DrawingRevision { get; set; }
		public string SherwinDft { get; set; }
		public string SherwinWft { get; set; }
		public string HempelDft { get; set; }
		public string HempelWft { get; set; }
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
		public bool IsAbnormal { get; set; }
		public string PartPrefix { get; set;  }
		public string AssemblyPrefix { get; set; }

		public List<Enums.Error> PartErrors = new List<Enums.Error>();
		public DrawingClassification DrawingClassification = DrawingClassification.Unclassified;

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

		private string GetStringProperty(Part part, string reportProperty)
		{
			string value = "";
			part.GetReportProperty(reportProperty, ref value);
			return value;
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