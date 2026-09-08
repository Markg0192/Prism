using Prism;
using Prism.ButtonOperations;
using PrismNewInterface.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Tekla.Structures.Model;
using static Prism.Enums;
using Task = System.Threading.Tasks.Task;

namespace PrismNewInterface
{
	public partial class PrepareSpecialFittingsWindow : Window
	{
		private readonly PrismOperations _prismOperations;

		public PrepareSpecialFittingsWindow(PrismOperations prismOperations)
		{
			if (prismOperations == null)
			{
				throw new ArgumentNullException(nameof(prismOperations));
			}

			_prismOperations = prismOperations;

			InitializeComponent();
		}

		private void TagFittingsButton_Click(object sender, RoutedEventArgs e)
		{
			SetStatus("Reading selected parts...");

			var selectedParts = SelectedObjects.GetSelectedParts();

			SetStatus("Tagging selected parts as Special Fittings...");

			ModelModifiers.ModifySpecialTag("Special", selectedParts);

			SetStatus("Highlighting tagged fittings...");

			ModelModifiers.SetPartsGreen(selectedParts, false);

			SetStatus(selectedParts.Count + " Special Fittings tagged.");
		}

		private void RemoveTagsButton_Click(object sender, RoutedEventArgs e)
		{
			SetStatus("Reading selected parts...");

			var selectedParts = SelectedObjects.GetSelectedParts();

			SetStatus("Removing Special Fitting tags...");

			ModelModifiers.ModifySpecialTag("", selectedParts);

			SetStatus("Updating model highlighting...");

			ModelModifiers.SetPartsRed(selectedParts, false);

			SetStatus(selectedParts.Count + " Special Fitting tags removed.");
		}

		private async void ShowTaggedButton_Click(object sender, RoutedEventArgs e)
		{
			SetStatus("Checking the current selection for tagged fittings...");

			SelectedObjects selectedParts = await Task.Run(() => new SelectedObjects(_prismOperations.ProjectData.ProjPath, StageTypes.Prelim1, "", "", _prismOperations.Model, false));

			List<ModelObject> specialFittings = new List<ModelObject>();

			foreach (PrismPart part in selectedParts.PrismParts)
			{
				string tagInfo = "";

				part.Part.GetUserProperty(ModelUDA.SpecialFittingTag(), ref tagInfo);

				if (tagInfo != "")
				{
					specialFittings.Add(part.Part);
				}
			}

			SetStatus("Highlighting tagged Special Fittings...");

			ModelModifiers.SetPartsBlue(specialFittings);

			SetStatus(specialFittings.Count + " tagged Special Fittings found.");
		}

		private async void CreateDrawingsButton_Click(object sender, RoutedEventArgs e)
		{
			SetStatus("Reading selected Special Fittings...");

			SelectedObjects selectedObjects = await Task.Run(() => new SelectedObjects(_prismOperations.ProjectData.ProjPath, StageTypes.Prelim1, "", "", _prismOperations.Model, false));

			SetStatus("Filtering selection to tagged Special Fittings...");

			List<PrismPart> selectedParts = await Task.Run(() => ModelModifiers.SelectSpecialTaggedInSelection(selectedObjects));

			if (selectedParts.Count == 0)
			{
				SetStatus("No tagged Special Fittings found in the current selection.");
				return;
			}

			SetStatus("Running Tekla numbering, user will have to accept numbering before continuing...");

			bool numberingSuccessful = await Task.Run(() => ModelModifiers.PerformNewNumbering());

			if (!numberingSuccessful)
			{
				SetStatus("Numbering was not completed. Drawings were not created.");
				return;
			}

			SetStatus("Creating " + selectedParts.Count + " Special Fitting drawings...");

			await Task.Run(() => ModelModifiers.CreateDrawings(selectedParts));

			SetStatus(selectedParts.Count + " Special Fitting drawings created.");
		}

		private void TagFittingsInformationButton_Click(object sender, RoutedEventArgs e)
		{
			ShowInformation(
				"Tag Special Fittings",
				"Tags the currently selected Tekla parts as Special Fittings.\n\n" +
				"Prism stamps the Special Fitting UDA on each selected part so it can identify the fitting later during drawing creation and material ordering.\n\n" +
				"Once tagged, the selected parts are highlighted green in the model.\n\n" +
				"All fittings that are to be processed as Special Fittings must be tagged before continuing.");
		}

		private void RemoveTagsInformationButton_Click(object sender, RoutedEventArgs e)
		{
			ShowInformation(
				"Remove Special Fitting Tags",
				"Removes the Special Fitting tag from the currently selected Tekla parts.\n\n" +
				"Once the tag has been removed, Prism will no longer treat those parts as Special Fittings during drawing creation or material ordering.\n\n" +
				"Parts that have had their tag removed are highlighted red in the model.");
		}

		private void ShowTaggedInformationButton_Click(object sender, RoutedEventArgs e)
		{
			ShowInformation(
				"Show Tagged Special Fittings",
				"Checks the current Tekla selection and identifies every part that has been tagged as a Special Fitting.\n\n" +
				"Tagged fittings within the selection are highlighted blue in the model.\n\n" +
				"This can be used to confirm which fittings Prism will process before creating drawings or placing an order.");
		}

		private void CreateDrawingsInformationButton_Click(object sender, RoutedEventArgs e)
		{
			ShowInformation(
				"Create Special Fitting Drawings",
				"Creates drawings for Special Fittings that have been tagged within the current Tekla selection.\n\n" +
				"Prism filters the selection so that only tagged Special Fittings are processed.\n\n" +
				"Before creating the drawings, Prism performs a Tekla numbering operation. The numbering must complete successfully before Prism can continue.\n\n" +
				"Once numbering is complete, Prism creates drawings for the tagged fittings.\n\n" +
				"Special Fitting drawings must be created before the fittings can be included in a Special Fittings material order.");
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void ShowInformation(string title, string information)
		{
			InformationWindow informationWindow = new InformationWindow(title, information);

			informationWindow.Owner = this;
			informationWindow.ShowDialog();
		}

		private void SetStatus(string message)
		{
			StatusTextBlock.Text = message;
		}
	}
}