using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Model;
using static Prism.Enums;

namespace Prism
{
    public static class BswxExporter
    {
        private const int _maxNumberOfPartsInAnArray = 99;

        public static void ExportBSWX(this SelectedObjects selectedObjects, string myFolder, PrismProjectData modelData, string phaseNumber,
            string issueNumber, stageTypes stageType)
        {
            List<ArrayList> myLists = new List<ArrayList>();
            myLists.Add(new ArrayList());

            foreach (var item in selectedObjects.AssembliesList)
            {
                ArrayList currentList = myLists.Last();
                if (currentList.Count >= _maxNumberOfPartsInAnArray) myLists.Add(new ArrayList());
                currentList = myLists.Last();
                currentList.Add(item);
            } 
            RunBswxExport(myLists, myFolder, modelData, phaseNumber, issueNumber, stageType);
        }

        public static void ExportBSWX(this List<Part> myParts, string myFolder, PrismProjectData modelData, string phaseNumber,
            string issueNumber, stageTypes stageType)
        {
            List<ArrayList> myLists = new List<ArrayList>();
            myLists.Add(new ArrayList());

            foreach (var item in myParts)
            {
                ArrayList currentList = myLists.Last();
                if (currentList.Count >= _maxNumberOfPartsInAnArray) myLists.Add(new ArrayList());
                currentList = myLists.Last();
                currentList.Add(item);
            }
            RunBswxExport(myLists, myFolder, modelData, phaseNumber, issueNumber, stageType);
        }

        private static void RunBswxExport(List<ArrayList> myLists, string myFolder, PrismProjectData modelData, string phaseNumber,
            string issueNumber, stageTypes stageType)
        {   
            ModelModifiers.HideOrRestoreTekla(7);
            Component bimRevExp = new Component();
            bimRevExp.Name = "BIMREVIEW Export";
            bimRevExp.Number = -100000;
            ComponentInput myInputs = new ComponentInput();

            foreach (ArrayList a in myLists)
            {
                myInputs.AddInputObjects(a);
            }

            string typeString;
            if (stageType == stageTypes.Prelim1 || stageType == stageTypes.Prelim2 || stageType == stageTypes.Prelim3)
            {
                typeString = stageType.ToString().Substring(0, (stageType.ToString().Length - 1)).ToUpper();
            }
            else
            {
                typeString = stageType.ToString();
            }
            if (stageType == stageTypes.PrelimPG)
            {
                typeString = "PG-Prelim";
            }

            bimRevExp.SetComponentInput(myInputs);
            bimRevExp.SetAttribute("output_file_path", $@"{myFolder}\{modelData.ProjNumber}-{phaseNumber}-{typeString}-ISSUE{issueNumber}.bswx");
            bimRevExp.SetAttribute("export_cam_files", 0);
            bimRevExp.SetAttribute("cam_file_folder", "");
            bimRevExp.SetAttribute("export_gantt_charts", 0);
            bimRevExp.SetAttribute("selected_parts_only", 1);
            bimRevExp.SetAttribute("obj_in_assem", 0);
            bimRevExp.SetAttribute("complete_assemblies", 1);
            bimRevExp.SetAttribute("exclude_bolts", 0);
            bimRevExp.SetAttribute("exclude_holes", 0);
            bimRevExp.SetAttribute("exclude_welds", 0);
            bimRevExp.SetAttribute("exclude_cuts", 0);
            bimRevExp.SetAttribute("plate_prefixes", "");
            bimRevExp.SetAttribute(" ValidateOnExport", 0);
            bimRevExp.SetAttribute("split_bolts", 1);
            bimRevExp.SetAttribute("merge_girder", 0);
            bimRevExp.SetAttribute("modified_weight", 0);
            bimRevExp.SetAttribute("export_drawings", 0);
            bimRevExp.SetAttribute("drawing_file_folder", "");
            bimRevExp.SetAttribute("include_gas", 0);
            bimRevExp.SetAttribute("include_multi", 0);
            bimRevExp.SetAttribute("drawing_extension", 0);
            bimRevExp.Insert(); 
            ModelModifiers.HideOrRestoreTekla(9);
        }
    }
}