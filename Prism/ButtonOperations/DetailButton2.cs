using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.ButtonOperations
{
    public static class DetailButton2
    {
        public static void DetailButton2op(this SelectedObjects myObjects, PrismProjectData projectData, int stageNumber)
        {
            if (!myObjects.RunStage4Checks()) { return; }

            myObjects.ModifyAttributes(stageNumber, projectData);           

        }
    }
}