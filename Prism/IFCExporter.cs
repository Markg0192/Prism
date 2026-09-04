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
			await RunIFCExport(selectedObjects.GetDistinctByPartMark(selectedObjects.GetMainParts()), myFolder, toolStrip, statusLabel);
			selectedObjects.PrismParts.SelectParts();
		}

		public static async Task ExportIndividualIFC(this SelectedObjects selectedObjects, string myFolder)
		{
			await RunIFCExport(selectedObjects.GetDistinctByPartMark(selectedObjects.GetMainParts()), myFolder);
			selectedObjects.PrismParts.SelectParts();
		}

		private static async Task RunIFCExport(List<PrismPart> prismParts, string localFolder)
		{
			int numberOfParts = prismParts.Count;
			int currentPartNo = 1;
			Tekla.Structures.Model.UI.ModelObjectSelector MS = new Tekla.Structures.Model.UI.ModelObjectSelector();

			await Task.Run(() =>
			{
				foreach (PrismPart prismPart in prismParts)
				{
					ExportIFC(prismPart, localFolder, MS, SetUpIfcComponent());

					currentPartNo++;
				}

				CleanFolder(localFolder);
			});
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
						statusLabel.Text = $"Exporting IFC: {currentPartNo} of {numberOfParts}";
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

		public static async Task ExportIndividualIFC(this SelectedObjects selectedObjects, string myFolder, Action<int, string> progress)
		{
			List<PrismPart> partsToExport = selectedObjects.GetDistinctByPartMark(selectedObjects.GetMainParts());

			progress?.Invoke(0, partsToExport.Count == 0 ? "No individual IFCs require exporting." : $"Preparing to export {partsToExport.Count} individual IFCs...");

			Action<int, string> exportProgress = CreateProgressRange(progress, 0, 95);

			await RunIFCExport(partsToExport, myFolder, exportProgress);

			progress?.Invoke(95, "Restoring selected parts...");

			selectedObjects.PrismParts.SelectParts();

			progress?.Invoke(100, partsToExport.Count == 1 ? "1 individual IFC exported." : $"{partsToExport.Count} individual IFCs exported.");
		}

		private static async Task RunIFCExport(List<PrismPart> prismParts, string localFolder, Action<int, string> progress)
		{
			int numberOfParts = prismParts.Count;

			if (numberOfParts == 0)
			{
				progress?.Invoke(100, "No individual IFCs require exporting.");
				return;
			}

			progress?.Invoke(0, $"Exporting IFCs: 0 of {numberOfParts}");

			Tekla.Structures.Model.UI.ModelObjectSelector modelObjectSelector = new Tekla.Structures.Model.UI.ModelObjectSelector();

			await Task.Run(() =>
			{
				for (int i = 0; i < numberOfParts; i++)
				{
					PrismPart prismPart = prismParts[i];

					ExportIFC(prismPart, localFolder, modelObjectSelector, SetUpIfcComponent());

					int completedCount = i + 1;
					int percentage = (int)Math.Round((completedCount / (double)numberOfParts) * 95.0);

					progress?.Invoke(percentage, $"Exporting IFC: {completedCount} of {numberOfParts} - {prismPart.PartMark}");
				}

				progress?.Invoke(95, "Cleaning IFC export folder...");

				CleanFolder(localFolder);
			});

			progress?.Invoke(100, "Individual IFC export complete.");
		}

		private static Action<int, string> CreateProgressRange(Action<int, string> progress, int startPercentage, int endPercentage)
		{
			if (progress == null) return null;

			return (percentage, message) =>
			{
				int clampedPercentage = Math.Max(0, Math.Min(100, percentage));
				int mappedPercentage = startPercentage + (int)Math.Round((endPercentage - startPercentage) * (clampedPercentage / 100.0));

				progress(mappedPercentage, message);
			};
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
	}
}
