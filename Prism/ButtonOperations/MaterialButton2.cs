using System;
using System.Collections.Generic;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using static Prism.Enums;

namespace Prism.ButtonOperations
{
    public static class MaterialButton2
    {
        public static bool MaterialButton2op(this SelectedObjects myObjects, string startNumber, int stageNumber, PrismProjectData projectData, Model model)
        {
            //HDBolts.StampConnectionCodeOnMainMember(myObjects);
            if (ModelChecker.MemberOrientationIsCorrect(myObjects, out IgnoreType ignore))
            {
                if (!Constants.IsSpecialPerson())
                {
                    if (!myObjects.ProcessFabsecs(model, projectData)) { return false; }
                }
                myObjects.AddStartNumbers(startNumber);
                if (!myObjects.SelectedModelParts.ModifyAttributes(stageNumber, projectData)) { return false; }

                if (ignore == IgnoreType.AutoFix)
                {
                    AutoFix.MemberOrientation();
                }

                Logging.LogProgress(projectData.ProjNumberAndName, "Material 2", 0, myObjects.AssembliesList.Count, projectData.WebService);
                return true;
            }
            return false;
        }
    }
}