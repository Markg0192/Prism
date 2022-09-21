using Tekla.Structures.Model;

namespace Prism
{
    public class ModelPart
    {
        public ModelPart(Part part, int startNumber)
        {
            Part = part;
            StartNumber = startNumber;
        }

        public ModelPart(Part part, Phase phase)
        {
            Part = part;
            Phase = phase;
        }

        public Phase Phase { get; set; }
        public Part Part { get; set; }
        public int StartNumber { get; set; }
    }
}