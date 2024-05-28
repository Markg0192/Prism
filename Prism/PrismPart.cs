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
        }

        public string Prelim { get; set; }
        public string Guid { get; set; }
        public ModelObject ModelObject { get; set; }
        public string ErrorString { get; set; }
        public Phase Phase { get; set; }
        public Part Part { get; set; }
        public int StartNumber { get; set; }
    }
}
