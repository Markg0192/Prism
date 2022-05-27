using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Model;

namespace Prism
{
    public class BswxExporter
    {
        private int MaxNumberOfPartsInAnArray = 99;

        public void ExportBSWX(SevModelEnumerator modelEnum, SevFolders myFolderManager, SevModelData modelData, string phaseNumber, string issueNumber)
        {
            List<ArrayList> myLists = new List<ArrayList>();
            myLists.Add(new ArrayList());

            foreach (var item in modelEnum.AssembliesList)
            {
                ArrayList currentList = myLists.Last();
                if (currentList.Count >= MaxNumberOfPartsInAnArray) myLists.Add(new ArrayList());
                currentList = myLists.Last();
                currentList.Add(item);
            }

            Component bimRevExp = new Component();
            bimRevExp.Name = "BIMREVIEW Export";
            bimRevExp.Number = -100000;
            ComponentInput myInputs = new ComponentInput();

            foreach (ArrayList a in myLists)
            {
                myInputs.AddInputObjects(a);
            }

            bimRevExp.SetComponentInput(myInputs);
            bimRevExp.SetAttribute("output_file_path", $@"{myFolderManager.dspPath}\{modelData.ProjNumber}-{phaseNumber}-FAB-ISSUE{issueNumber}.bswx");
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
        }
    }
}

