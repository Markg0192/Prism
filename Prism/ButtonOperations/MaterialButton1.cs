using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.ButtonOperations
{
    public static class MaterialButton1
    {
        public static void MaterialButton1op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber)
        {
            bool isValid = myObjects.CheckExecutionField() &&
                           myObjects.CheckNameAndClassAlignment();
            if (!isValid) { return; }

           myObjects.ModifyAttributes(stageNumber, projectData);
        }
    }
}
