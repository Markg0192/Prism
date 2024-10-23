using Tekla.Structures.Model;
using System.Collections.Generic;
using System.IO;
using Task = System.Threading.Tasks.Task;
using System.Windows.Forms;
using System.Collections;
using System;
using System.Linq;

namespace Prism
{
	public static class IFCExporter
	{
		public static async Task ExportIndividualIFC(this SelectedObjects selectedObjects, string myFolder, ToolStrip toolStrip, ToolStripStatusLabel statusLabel)
		{
			if (ModelChecker.IsCurrentUser("dean.johnston",  "mark.gibson", "ian.partridge"))
			{
				await RunIFCExport(GetDistinctByPartMark(selectedObjects.GetMainParts()), myFolder, toolStrip, statusLabel);
				selectedObjects.PrismParts.SelectParts();
			}
		}

		private static List<PrismPart> GetDistinctByPartMark(List<PrismPart> prismParts)
		{
			return prismParts
				.GroupBy(part => part.PartMark)
				.Select(group => group.First())
				.ToList();
		}

		private static async Task RunIFCExport(List<PrismPart> prismParts, string localFolder, ToolStrip toolStrip, ToolStripStatusLabel statusLabel)
		{
			int numberOfParts = prismParts.Count;
			int currentPartNo = 1;
			Tekla.Structures.Model.UI.ModelObjectSelector MS = new Tekla.Structures.Model.UI.ModelObjectSelector();

			await Task.Run(() =>
			{
				foreach (PrismPart prismPart in prismParts)
				{
					ExportIFC(prismPart, localFolder, MS, SetUpIfcComponent());

					// Since UI updates must be on the UI thread, use Invoke or BeginInvoke to update the status label
					toolStrip.Invoke(new Action(() =>
					{
						UpdateStatusLabel(toolStrip, statusLabel, currentPartNo, numberOfParts);
					}));

					currentPartNo++;
				}

				CleanFolder(localFolder);
			});
		}

		private static Component SetUpIfcComponent()
		{
			ComponentInput componentInput = new ComponentInput();
			componentInput.AddOneInputPosition(new Tekla.Structures.Geometry3d.Point(0.0, 0.0, 0.0));
			Component component = new Component(componentInput)
			{
				Name = "ExportIFC",
				Number = -100000
			};
			component.LoadAttributesFromFile("-SEV-IFC");
			return component;
		}

		private static void ExportIFC(PrismPart part, string myFolder, Tekla.Structures.Model.UI.ModelObjectSelector MS, Component component)
		{
			MS.Select(new ArrayList() { part.Assembly });

			component.SetAttribute("OutputFile", $"{myFolder}/{part.PartMark}-{part.DrawingRevision}");
			component.Insert();
		}

		static void CleanFolder(string folderPath)
		{
			// Get all .log files in the folder
			string[] logFiles = Directory.GetFiles(folderPath + "\\", "*.log");

			foreach (string file in logFiles)
			{
				File.Delete(file);
			}
		}

		private static void UpdateStatusLabel(ToolStrip toolStrip, ToolStripStatusLabel statusLabel, int currentFileCount, int desiredFileCount)
		{
			toolStrip.Invoke(new System.Action(() =>
			{
				statusLabel.Text = $"Exporting IFC: {currentFileCount} of {desiredFileCount}";
			}));
		}
	}
}
