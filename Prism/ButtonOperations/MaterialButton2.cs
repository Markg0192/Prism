using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.ButtonOperations
{
    public static class MaterialButton2
    {
        public static void MaterialButton2op(this SelectedObjects myObjects, string startNumber, int stageNumber, PrismProjectData projectData)
        {
            myObjects.AddStartNumbers(startNumber);
            myObjects.ModifyAttributes(stageNumber, projectData);
        }
    }
}
