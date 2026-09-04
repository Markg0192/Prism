using System;
using System.Threading;
using System.Collections;
using static Prism.Enums;
using Tekla.Structures.Model;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace Prism
{
	public static class BswxExporter
	{
		 public static async Task ExportBSWX(this SelectedObjects selectedObjects, string myFolder, PrismProjectData modelData, string phaseNumber,
			 string issueNumber, StageTypes stageType, ToolStrip toolStrip, ToolStripStatusLabel statusLabel)
		 {
			toolStrip.Invoke(new Action(() =>
			{
				statusLabel.Text = "Exporting BSWX";
			}));

			//To run the bswx exporter we need to give it an input, this input can be an ArrayList, only 1 part is required, the exporter will then create a bswx of all parts selected in the model
			ArrayList myInputList = new ArrayList
			 {  selectedObjects.PrismParts[0].Part};
			 await RunBswxExport(myInputList, myFolder, modelData, phaseNumber, issueNumber, stageType);
		 }

		public static async Task ExportBSWX(this SelectedObjects selectedObjects, string myFolder, PrismProjectData modelData, string phaseNumber,
	string issueNumber, StageTypes stageType)
		{
			//To run the bswx exporter we need to give it an input, this input can be an ArrayList, only 1 part is required, the exporter will then create a bswx of all parts selected in the model
			ArrayList myInputList = new ArrayList
			 {  selectedObjects.PrismParts[0].Part};
			await RunBswxExport(myInputList, myFolder, modelData, phaseNumber, issueNumber, stageType);
		}

		private static async Task RunBswxExport(ArrayList inputList, string myFolder, PrismProjectData modelData, string phaseNumber,
			string issueNumber, StageTypes stageType)
		{
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
			bimRevExp.SetAttribute("selected_parts_only", 1);

			// Monitor for the standard bswx export pop ups, do it on a seperate thread to the current one.
			BswxPopUpCloser popupCloser = new BswxPopUpCloser(TimeSpan.FromMinutes(10));
			Thread popUpMonitorThread = new Thread(() => popupCloser.Run());
			popUpMonitorThread.IsBackground = true;
			popUpMonitorThread.Start();	

			bimRevExp.Insert();
		}
	}
}