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
    public class ModelModifiers
    {
        public SevModelData ModelData;

        public ModelModifiers(Model model)
        {
            ModelData = model.CreateSevModelData();
        }

        public void MarkAsFabPackComplete(SevModelEnumerator modelEnum)
        {
            foreach (Part part in modelEnum.SelectedModelParts)
            {
                part.SetUserProperty("PRISM-FAB-1-NAME", ModelData.Full);
                part.SetUserProperty("PRISM-FAB-1-DATE", ModelData.Date);
                part.SetUserProperty("PRISM-FAB-1-NUMBER", part.GetPartMark());
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

        public void LockSelected(SevModelEnumerator modelEnum)
        {
            foreach (Part part in modelEnum.SelectedModelParts)
            {
                part.SetUserProperty("OBJECT_LOCKED", 1);
            }
        }
    }
}
