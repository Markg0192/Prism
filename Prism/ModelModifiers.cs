using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Model;

namespace Prism
{
    /// <summary>
    /// The Model modifiers class is where all changes to the model take place.
    /// This is usually adding stamps to member and drawing user fields.
    /// </summary>
    public static class Modifiers
    {
        public static ModelModifiers ModelMods(this Model model)
        {
            return new ModelModifiers(model);
        }
    }
    public class ModelModifiers
    {
        public SevModelData ModelData;
        public SevModelEnumerator ModelEnum;

        public ModelModifiers(Model model)
        {
            ModelData = model.SevModelData();
            ModelEnum = model.SevModelEnumerator();
        }

        public void MarkAsFabPackComplete(SevModelEnumerator modelEnum)
        {
            foreach (Part part in modelEnum.SelectedModelParts)
            {
                part.SetUserProperty("PRISM-FAB-1-NAME", ModelData.Full);
                part.SetUserProperty("PRISM-FAB-1-DATE", ModelData.Date);
                part.SetUserProperty("PRISM-FAB-1-NUMBER", ModelEnum.MyPart.GetPartMark());
                part.Modify();
            }
        }
        
        public void MaterialCheckModifier(SevModelEnumerator modelEnum, int StageNumber)
        {
            foreach (Part part in modelEnum.SelectedModelParts)
            {
                part.SetUserProperty($"PRISM-MAT-{StageNumber}-NAME", ModelData.Full);
                part.SetUserProperty($"PRISM-MAT-{StageNumber}-DATE", ModelData.Date);
                part.Modify();
            }
        }
        public void DetailingCheckModifier(SevModelEnumerator modelEnum, int StageNumber)
        {
            foreach (Part part in modelEnum.SelectedModelParts)
            {
                part.SetUserProperty($"PRISM-DET-{StageNumber}-NAME", ModelData.Full);
                part.SetUserProperty($"PRISM-DET-{StageNumber}-DATE", ModelData.Date);
                part.Modify();
            }
        }
    }
}
