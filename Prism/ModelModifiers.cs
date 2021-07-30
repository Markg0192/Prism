using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Model;

namespace Prism
{
    public static class Modifiers
    {
        public static ModelModifiers ModelMods(this Model model)
        {
            return new ModelModifiers(model);
        }
    }

    public class ModelModifiers
    {
        public SevModelData modelData;

        public ModelModifiers(Model model)
        {
            modelData = model.SevModelData();

        }

        public void FabPackComplete(Part myPart)
        {
            myPart.SetUserProperty("SEV-UDA-16", modelData.Full);
            myPart.SetUserProperty("SEV-UDA-17", modelData.date);
            myPart.SetUserProperty("SEV-UDA-18", myPart.GetPartMark());
        }
    }
}
