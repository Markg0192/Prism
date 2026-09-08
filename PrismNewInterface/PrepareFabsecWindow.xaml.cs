using PrismNewInterface.Services;
using PrismNewInterface.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PrismNewInterface
{
	public partial class PrepareFabsecsWindow : Window
	{
		public PrepareFabsecsWindow(PrismOperations prismOperations)
		{
			if (prismOperations == null)
			{
				throw new ArgumentNullException(nameof(prismOperations));
			}

			InitializeComponent();

			DataContext = new PrepareFabsecsViewModel(prismOperations);
		}

		private void PrepareMaterialInformationButton_Click(object sender, RoutedEventArgs e)
		{
			List<InformationSection> sections = new List<InformationSection>
			{
				new InformationSection
				{
					Title = "Add Green",
					Summary = "Adds additional material to each end of the selected Fabsec members.",
					Details =
						"Prism adds green to selected Fabsec members with a profile beginning PG.\n\n" +

						"• By default, 100 mm is added to each end of the member.\n" +
						"• The amount of green added can be changed in Advanced Settings.\n" +
						"• When complete, Prism stamps 'Green added' into the SEV-UDA-125 UDA.\n\n" +

						"Previously Prepared Members\n" +
						"• If SEV-UDA-125 already contains 'Green added', Prism will warn that the member has previously been prepared.\n" +
						"• The user can choose to add green again, but this is not recommended.\n\n" +

						"End Cuts\n" +
						"• Prism cannot add green where cuts have already been applied to the ends of the member.\n" +
						"• For this reason, Fabsec members should be prepared before detailing begins.\n" +
						"• Prism will warn the user when green cannot be added."
				},

				new InformationSection
				{
					Title = "Unique Numbering",
					Summary = "Assigns a unique PG prefix to each Fabsec profile used within the model.",
					Details =
						"Prism assigns a unique prefix to each different Fabsec profile.\n\n" +

						"• The first unique profile is assigned PG1.\n" +
						"• The next unique profile is assigned PG2, then PG3 and so on.\n" +
						"• Prefixes are unique across the entire model, not just the current selection.\n\n" +

						"After assigning the prefixes, Prism performs a numbering operation.\n" +
						"The user must accept this numbering before Prism can continue.\n" +
						"Once the numbering has been accepted, Prism saves the Tekla-assigned number as the preliminary mark.\n\n" +

						"Example\n" +
						"Members using the PG1 profile may be numbered:\n" +
						"• PG1-1\n" +
						"• PG1-2\n" +
						"• PG1-3\n\n" +

						"Members using the next unique profile may be numbered:\n" +
						"• PG2-1\n" +
						"• PG2-2\n" +
						"• and so on."
				},

				new InformationSection
				{
					Title = "Next Step",
					Summary = "Once Fabsec preparation is complete, the members can move on to raw material ordering.",
					Details =
						"When Add Green and Unique Numbering have both completed successfully, return to the main Material Order page.\n\n" +

						"The Fabsec raw material can then be ordered, provided the standard Material Order checks have also been completed successfully."
				}
			};

			InformationWindow informationWindow = new InformationWindow(
				"Prepare Fabsec",
				"Prepares selected Fabsec members for raw material ordering. Prism adds the required green and assigns unique numbering to the Fabsec profiles.",
				sections);

			informationWindow.Owner = this;
			informationWindow.ShowDialog();
		}

		private void CreateCarcassInformationButton_Click(object sender, RoutedEventArgs e)
		{
			List<InformationSection> sections = new List<InformationSection>
			{
				new InformationSection
				{
					Title = "Eligibility for Carcass Creation",
					Summary = "Checks that each selected Fabsec member is ready for a carcass to be created.",
					Details =
						"Prism first identifies selected Fabsec members with a profile beginning PG, then checks whether each member is eligible for carcass creation.\n\n" +

						"Raw Material Ordered\n" +
						"• Prism checks SEV-UDA-114 to confirm that the raw material has been ordered.\n" +
						"• If this UDA is blank, the raw material has not yet been ordered.\n" +
						"• Raw material must be ordered before a Fabsec carcass can be created.\n\n" +

						"Carcass Already Created\n" +
						"• Prism checks SEV-UDA-132 to determine whether the carcass creation process has already been completed.\n" +
						"• If this UDA contains data, Prism considers a carcass to already exist for the member.\n\n" +

						"Carcass Already Ordered\n" +
						"• Prism checks SEV-UDA-134 to determine whether the Fabsec carcass has already been ordered.\n" +
						"• If this UDA contains data, the member is not eligible for the carcass creation process."
				},

				new InformationSection
				{
					Title = "Create Carcass Copy",
					Summary = "Creates a clean copy of each eligible Fabsec member to use as the carcass.",
					Details =
						"Prism creates a copy of each eligible Fabsec member 1,000 m above the model. This copied member becomes the Fabsec carcass.\n\n" +

						"Remove Cuts\n" +
						"• Prism checks the copied carcass for fitting-type cuts.\n" +
						"• Where possible, Prism identifies the component responsible for the cut and removes the component.\n" +
						"• If no related component can be found, Prism removes the fitting cut directly.\n" +
						"• This continues until the carcass has been cleared of these cuts.\n\n" +

						"Restore Green\n" +
						"• Prism then adds the required green back onto the carcass.\n" +
						"• The green added during the original Fabsec preparation stage is removed when the raw material is ordered, so it must be restored on the new carcass."
				},

				new InformationSection
				{
					Title = "Restore Standard Numbering",
					Summary = "Restores normal model numbering and keeps the unique Fabsec number on the carcass.",
					Details =
						"Prism updates the numbering of both the original model Fabsec and the new carcass.\n\n" +

						"Model Fabsec\n" +
						"• The unique PG numbering applied during Fabsec preparation is removed from the original model member.\n" +
						"• The member is returned to the standard numbering system.\n" +
						"• Prism reads SEV-UDA-133 to determine the correct start number.\n" +
						"• This start number was assigned when the standard Material Order checks were completed.\n\n" +

						"Fabsec Carcass\n" +
						"• The unique Fabsec number is retained and forced onto the new carcass.\n" +
						"• This keeps the carcass linked to the unique Fabsec identity created during the preparation stage."
				},

				new InformationSection
				{
					Title = "Create Carcass Drawings",
					Summary = "Numbers the completed carcasses and creates their carcass drawings.",
					Details =
						"Once the carcasses have been created and prepared, Prism creates the Fabsec carcass drawings.\n\n" +

						"Final Numbering\n" +
						"• Prism performs a Tekla numbering operation to apply the numbers that have been forced onto the carcasses.\n" +
						"• The user must approve this numbering before Prism can continue.\n\n" +

						"Create Drawings\n" +
						"• Prism uses the 'SEV Fabsec Carcass Drawing Wizard' to create drawings for the prepared Fabsec carcasses.\n" +
						"• A carcass drawing is created for each carcass being processed.\n\n" +

						"Mark as Complete\n" +
						"• Once the drawings have been created, Prism updates SEV-UDA-132 on both the original model member and its carcass.\n" +
						"• The original model member is marked to show that a carcass has been created from it.\n" +
						"• The copied member is marked to identify it as the Fabsec carcass."
				},

				new InformationSection
				{
					Title = "Next Step",
					Summary = "Once the carcasses and their drawings are complete, they are ready to move on to ordering.",
					Details =
						"Before ordering the Fabsec carcasses, it may be appropriate to review the newly created carcass drawings and confirm they are correct.\n\n" +

						"When ready, return to the main Material Order page and create the Fabsec carcass order."
				}
			};

			InformationWindow informationWindow = new InformationWindow(
				"Create Fabsec Carcass",
				"Creates Fabsec carcasses for eligible selected members. Prism first checks that each Fabsec is at the correct stage of the material ordering process before continuing.",
				sections);

			informationWindow.Owner = this;
			informationWindow.ShowDialog();
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}