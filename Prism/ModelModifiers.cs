using System;
using System.Collections;
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
        public SevModelEnumerator modelEnum;

        public ModelModifiers(Model model)
        {
            modelData = model.SevModelData();
            modelEnum = model.SevModelEnumerator();
        }
        public void FabPackComplete(SevModelEnumerator modelEnum)
        {
            foreach (Part part in modelEnum.selectedModelParts)
            {
                part.SetUserProperty("PRISM-FAB-1-NAME", modelData.Full);
                part.SetUserProperty("PRISM-FAB-1-DATE", modelData.date);
                part.SetUserProperty("PRISM-FAB-1-NUMBER", modelEnum.myParts.GetPartMark());
                part.Modify();
            }
        }
        public void RunThroughMaterialChecks(SevModelEnumerator modelEnum)
        {      
            foreach (Part part in modelEnum.selectedModelParts)
            {
                part.SetUserProperty("PRISM-MAT-1-NAME", modelData.Full);
                part.SetUserProperty("PRISM-MAT-1-DATE", modelData.date);
                part.Modify();                
            }
        }
        public void MaterialChecksComplete(SevModelEnumerator modelEnum)
        {
            foreach (Part part in modelEnum.selectedModelParts)
            {
                part.SetUserProperty("PRISM-MAT-2-NAME", modelData.Full);
                part.SetUserProperty("PRISM-MAT-2-DATE", modelData.date);
                part.Modify();
            }
        }
        public void MaterialOrderComplete(SevModelEnumerator modelEnum)
        {
            foreach (Part part in modelEnum.selectedModelParts)
            {
                part.SetUserProperty("PRISM-MAT-3-NAME", modelData.Full);
                part.SetUserProperty("PRISM-MAT-3-DATE", modelData.date);
                part.Modify();
            }
        }
        public void RunThroughDetailingChecks(SevModelEnumerator modelEnum)
        {
            foreach (Part part in modelEnum.selectedModelParts)
            {
                part.SetUserProperty("PRISM-DET-1-NAME", modelData.Full);
                part.SetUserProperty("PRISM-DET-1-DATE", modelData.date);
                part.Modify();
            }
        }
        public void RunThroughDetailingChecksComplete(SevModelEnumerator modelEnum)
        {
            foreach (Part part in modelEnum.selectedModelParts)
            {
                part.SetUserProperty("PRISM-DET-2-NAME", modelData.Full);
                part.SetUserProperty("PRISM-DET-2-DATE", modelData.date);
                part.Modify();
            }
        }
        public void DrawingsCreated(SevModelEnumerator modelEnum)
        {
            foreach (Part part in modelEnum.selectedModelParts)
            {
                part.SetUserProperty("PRISM-DET-3-NAME", modelData.Full);
                part.SetUserProperty("PRISM-DET-3-DATE", modelData.date);
                part.Modify();
            }
        }
    }
}
