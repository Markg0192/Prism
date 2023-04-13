using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Model;
using static Prism.Enums;
using Task = System.Threading.Tasks.Task;

namespace Prism
{
    public static class BswxExporter
    {
        public static void ExportBSWX(this SelectedObjects selectedObjects, string myFolder, PrismProjectData modelData, string phaseNumber,
            string issueNumber, StageTypes stageType)
        {
            //To run the bswx exporter we need to give it an input, this input can be an ArrayList, only 1 part is required, the exporter will then create a bswx of all parts selected in the model
            ArrayList myInputList = new ArrayList
            {  selectedObjects.AssembliesList[0]};
            RunBswxExport(myInputList, myFolder, modelData, phaseNumber, issueNumber, stageType);
        }

        private static bool RunBswxExport(ArrayList inputList, string myFolder, PrismProjectData modelData, string phaseNumber,
            string issueNumber, StageTypes stageType)
        {
            ModelModifiers.HideOrRestoreTekla(7);
            Component bimRevExp = new Component();
            bimRevExp.Name = "BIMREVIEW Export";
            bimRevExp.Number = -100000;
            ComponentInput myInputs = new ComponentInput();
            myInputs.AddInputObjects(inputList);

            string typeString;
            if (stageType == StageTypes.Prelim1 || stageType == StageTypes.Prelim2 || stageType == StageTypes.Prelim3)
            {
                typeString = stageType.ToString().Substring(0, (stageType.ToString().Length - 1)).ToUpper();
            }
            else
            {
                typeString = stageType.ToString();
            }
            if (stageType == StageTypes.PrelimPG)
            {
                typeString = "PG-Prelim";
            }

            bimRevExp.SetComponentInput(myInputs);
            bimRevExp.LoadAttributesFromFile(ModelUDA.BSWXAttributeName(stageType));
            bimRevExp.SetAttribute("output_file_path", $@"{myFolder}\{modelData.ProjNumber}-{phaseNumber}-{typeString}-ISSUE{issueNumber}.bswx");
            bimRevExp.Insert();
            ModelModifiers.HideOrRestoreTekla(9);
            return true;
        }
    }
}