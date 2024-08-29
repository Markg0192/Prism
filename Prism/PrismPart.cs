using System.Security.Policy;
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

            double weight = 0;
            p.GetReportProperty(ModelUDA.Weight(), ref weight);
            Weight = weight;
            Profile = p.Profile.ProfileString;

            p.GetPhase(out Phase partPhase);
            Phase = partPhase;
        }

        public bool StartNumberDoesntMatch { get; set; }
        public bool PhaseDoesntMatchMain { get; set; }

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
        public string LotName { get; set; }
        public bool IsMainPart { get; set; }
        public double Weight { get; set; }
    }
}