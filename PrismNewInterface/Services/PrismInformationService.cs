using PrismNewInterface.Models;
using System.Collections.Generic;
using System.Windows;

namespace PrismNewInterface.Services
{
	/// <summary>
	/// Contains the help and information shown from the information buttons in Prism.
	/// Keeps the information text separate from the main window code.
	/// </summary>
	internal static class PrismInformationService
	{
		public static void ShowMaterialCheck(Window owner)
		{
			List<InformationSection> sections = new List<InformationSection>
			{
				new InformationSection
				{
					Title = "Name and Class",
					Summary = "Checks that part names and classes follow the required GDOM rules.",
					Details =
						"To pass this check, parts with the following names must use one of the associated classes:\n\n" +
						"• BEAM — Class 3\n" +
						"• COLUMN — Class 2 or 5\n" +
						"• BRACE — Class 4 or 13\n" +
						"• FABSEC — Class 7\n" +
						"• RAFTER — Class 8\n" +
						"• PORTAL-RAFTER — Class 8\n\n" +
						"Auto-Complete — Prism will automatically apply the correct class to each part based on its name and the rules above."
				},

				new InformationSection
				{
					Title = "Execution Class",
					Summary = "Checks that all main parts have an execution class assigned.",
					Details =
						"To pass this check, all main parts must have an execution class assigned in the 'EN1090_EXC_PART' UDA.\n\n" +
						"• EXC1, EXC2, EXC3 or EXC4 must be assigned.\n\n" +
						"Auto-Complete — Select the required execution class and Prism will apply it to all parts that failed this check."
				},

				new InformationSection
				{
					Title = "Member Orientation",
					Summary = "Checks that beams, columns and rafters are modelled in the required direction and rotation.",
					Details =
						"This check applies to supported I-section profiles: UB, UKB, UC, UKC, IPE, IPEA, IPN, HAU, HD, HEM, HEA and HEB.\n\n" +

						"Beams\n" +
						"• Horizontal members with the name BEAM are checked using their start and end points.\n" +
						"• A tolerance of 5 mm is used when determining whether the member is horizontal.\n" +
						"• Beams should always be modelled from SOUTH to NORTH and WEST to EAST along with other main members with symmetrical profiles within the Tekla model coordinate system.\n\n" +

						"Columns\n" +
						"• COLUMN members must use FRONT or BELOW rotation.\n" +
						"• BACK and TOP rotation will fail the check.\n" +
						"• The start point must be lower than the end point.\n\n" +

						"Rafters\n" +
						"• Members with a name containing RAFTER must be modelled with the start point at the apex.\n" +
						"• The start point must therefore be higher than the end point.\n" +
						"• Because this determines the modelling direction, SOUTH/NORTH and EAST/WEST direction is not considered for rafters.\n\n" +

						"Auto-Complete — Prism will swap the start and end handles of parts that failed this check to correct their orientation."
				}
			};

			Show(
				owner,
				"Material Order Check",
				"Checks the selected parts are ready for material ordering.",
				sections);
		}

		public static void ShowPrepareFabsecs(Window owner)
		{
			Show(
				owner,
				"Prepare Fabsecs",
				"Fabsec preparation is completed in two stages and must be carried out in the correct order.\n\n" +

				"1. Prepare Fabsec\n" +
				"This must be completed before ordering the raw material required to manufacture the Fabsec carcass.\n\n" +

				"2. Prepare Carcass\n" +
				"This must be completed before ordering the Fabsec carcass. The carcass cannot be prepared until the raw material has been ordered.\n\n" +

				"Clicking Prepare Fabsecs will open the preparation screen, where both stages can be completed. More detailed information about each operation is available from the information buttons beside each option.");
		}

		public static void ShowCreateMaterialOrder(Window owner)
		{
			List<InformationSection> sections = new List<InformationSection>
			{
				new InformationSection
				{
					Title = "Material",
					Summary = "Creates a standard material order for selected parts that have completed the required Prism checks.",
					Details =
						"Before Prism can create a material order, all selected parts must have completed the standard Material Order checks.\n\n" +

						"Fabsec Raw Material\n" +
						"• If Fabsec raw material is included, the Fabsec members must also have completed the Prepare Fabsec routine.\n\n" +

						"Previously Ordered Material\n" +
						"• Prism checks SEV-UDA-114 on each selected part.\n" +
						"• If this UDA already contains data, Prism considers the material to have already been ordered and the order will not continue.\n\n" +

						"Preliminary Marks\n" +
						"• Prism assigns a unique preliminary mark to each eligible part.\n" +
						"• Preliminary numbering starts at 1 by default and continues sequentially.\n" +
						"• The starting number can be changed or reset in Settings.\n" +
						"• A default prefix can also be added to all preliminary marks in Settings.\n" +
						"• Prism will not overwrite preliminary marks that have already been assigned manually or by another Prism process, including Fabsec preparation.\n\n" +

						"Create Order Files\n" +
						"• Prism creates the required reports and files for the material order. The order folder can be found within 'Prism Files' in the model folder.\n" +
						"• Files and folders are automatically named and organised in accordance with the standard material ordering procedure.\n" +
						"• Prism creates a standard material order email and automatically attaches the completed order files as a ZIP file.\n" +
						"• The email is automatically addressed to the Central Purchasing team.\n" +
						"• If Fabsec material is included, the Fabsec Processing team is also added.\n\n" +

						"Final Review\n" +
						"• The user should review the generated reports, files and email for completeness before sending the order.\n" +
						"• Relevant project team members should also be included in the email where appropriate. Default recipients can be configured in Settings.\n" +
						"• Once confirmed, send the email to complete the material ordering process.\n\n" +
						"• For future reference the length of each ordered piece and the phase and issue number used to order have been stored in the Prism UDA tab"
				},

				new InformationSection
				{
					Title = "Special Fittings",
					Summary = "Creates an order for special fittings.",
					Details = ""
				},

				new InformationSection
				{
					Title = "Bolts",
					Summary = "Creates an order for bolts.",
					Details = ""
				},

				new InformationSection
				{
					Title = "Seversafe",
					Summary = "Creates an order for Seversafe items.",
					Details = ""
				},

				new InformationSection
				{
					Title = "Fabsec Carcasses",
					Summary = "Creates an order for prepared Fabsec carcasses.",
					Details = ""
				}
			};

			Show(
				owner,
				"Create Material Order",
				"Creates material orders from the current Tekla selection. The process used depends on the type of material being ordered.",
				sections);
		}

		public static void ShowPrepareSpecialFittings(Window owner)
		{
			Show(
				owner,
				"Prepare Special Fittings",
				"Special fittings must be prepared before they can be ordered.\n\n" +
				"Preparation includes tagging the required fittings as Special Fittings in the Tekla model and creating their drawings.\n\n" +
				"Only when both of these steps have been completed will Prism consider the fittings ready to be included in a Special Fittings material order.");
		}

		public static void ShowDrawingCheck(Window owner)
		{
			List<InformationSection> sections = new List<InformationSection>
			{
				new InformationSection
				{
					Title = "Name and Class",
					Summary = "Checks that part names and classes follow the required GDOM rules.",
					Details =
						"To pass this check, parts with the following names must use one of the associated classes:\n\n" +
						"• BEAM — Class 3\n" +
						"• COLUMN — Class 2 or 5\n" +
						"• BRACE — Class 4 or 13\n" +
						"• FABSEC — Class 7\n" +
						"• RAFTER — Class 8\n" +
						"• PORTAL-RAFTER — Class 8\n\n" +
						"Auto-Complete — Prism will automatically apply the correct class to each part based on its name and the rules above."
				},

				new InformationSection
				{
					Title = "Execution Class",
					Summary = "Checks that all main parts have an execution class assigned.",
					Details =
						"To pass this check, all main parts must have an execution class assigned in the 'EN1090_EXC_PART' UDA.\n\n" +
						"• EXC1, EXC2, EXC3 or EXC4 must be assigned.\n\n" +
						"Auto-Complete — Select the required execution class and Prism will apply it to all parts that failed this check."
				},

				new InformationSection
				{
					Title = "Order Status",
					Summary = "Checks that main parts have had material ordered.",
					Details =
						"• Prism checks SEV-UDA-114 on each selected main part.\n" +
						"• If this UDA does not contain data, Prism considers the material to have not been ordered and the part will fail the check."
				},

				new InformationSection
				{
					Title = "Finish",
					Summary = "Checks that all main parts have a finish assigned.",
					Details =
						"Prism checks each main part to confirm that a finish has been assigned.\n\n" +
						"Parts without a finish will fail the drawing check."
				},

				new InformationSection
				{
					Title = "Intumescent Loading",
					Summary = "Checks that intumescent main parts have the required DFT or WFT loading information.",
					Details =
						"Parts with a finish beginning with IP are considered to have an intumescent finish.\n\n" +
						"• Intumescent parts must have a DFT or WFT loading value assigned.\n" +
						"• Prism first checks FIRE_DFT and FIRE_WFT for the required loading information.\n" +
						"• If these are empty, Prism also checks Heet.DFT and Heet.WFT.\n" +
						"• If no loading information is found in any of these four values, the part will fail the check."
				},

				new InformationSection
				{
					Title = "Start Number Match",
					Summary = "Checks that secondary parts use the same start number as their main part.",
					Details =
						"Prism compares each secondary part with its main part.\n\n" +
						"The secondary part start number must match the start number assigned to the main part.\n\n" +
						"Auto-Complete — Prism can copy the main part start number onto secondary parts that failed this check."
				},

				new InformationSection
				{
					Title = "Phase Number Match",
					Summary = "Checks that secondary parts are in the same phase as their main part.",
					Details =
						"Prism compares the phase number of each secondary part with its main part.\n\n" +
						"Secondary parts must be assigned to the same phase as the main part.\n\n" +
						"Auto-Complete — Prism can copy the main part phase onto secondary parts that failed this check."
				},

				new InformationSection
				{
					Title = "Next Step",
					Summary = "Once all drawing checks pass, the selection is ready for drawing creation.",
					Details =
						"When all drawing checks have passed, the selected parts are ready for Prism to create drawings.\n\n" +
						"Any parts that fail a check are shown in the results table and highlighted in the Tekla model."
				}
			};

			Show(
				owner,
				"Drawing Selection Check",
				"Checks the selected parts are ready for drawing creation. Prism validates the required model information and relationships before drawings can be created.",
				sections);
		}

		public static void ShowDetailOrientationHoles(Window owner)
		{
			List<InformationSection> sections = new List<InformationSection>
			{
				new InformationSection
				{
					Title = "Eligible Members",
					Summary = "Identifies members that are suitable for automatic orientation detailing.",
					Details =
						"Prism checks the selected members and only applies orientation detailing to eligible columns.\n\n" +

						"Standard Columns\n" +
						"• Members named COLUMN are eligible, except for PFC, RSA, SHS, CHS and RHS profiles.\n\n" +

						"Fabsec Columns\n" +
						"• Members with a profile containing PG are also eligible when they are modelled vertically.\n" +
						"• Prism checks the start and end points of the member to determine whether it is vertical.\n\n" +

						"Members that do not meet these requirements are ignored."
				},

				new InformationSection
				{
					Title = "Detailing Type",
					Summary = "Controls whether Prism adds orientation holes, orientation plates, or selects between the two automatically.",
					Details =
						"The orientation detailing method is selected before the process is run.\n\n" +

						"Holes\n" +
						"• Prism adds orientation holes to each eligible member.\n\n" +

						"Plate\n" +
						"• Prism adds an orientation plate to each eligible member.\n\n" +

						"Holes and Plate\n" +
						"• Prism automatically chooses between orientation holes and an orientation plate based on the column flange thickness.\n" +
						"• If the flange thickness is less than or equal to the configured flange thickness limit, orientation holes are added.\n" +
						"• If the flange thickness exceeds the limit, an orientation plate is added."
				},

				new InformationSection
				{
					Title = "Orientation Holes",
					Summary = "Adds orientation holes positioned automatically from the south-west corner of the member.",
					Details =
						"Prism analyses the geometry of the member and uses the south-west corner at the lower end of the column as the orientation set-out point.\n\n" +

						"• The orientation hole is set out 100mm from the end of the member.\n" +
						"• Prism automatically selects a suitable hole size of 20 mm, 16 mm or 12 mm based on the available space between the web and root radius.\n" +
						"• The holes are created and are identified with the bolt comment 'Orientation Hole'."
				},

				new InformationSection
				{
					Title = "Orientation Plate",
					Summary = "Adds a welded orientation plate at the south-west side of the member.",
					Details =
						"Prism uses the south-west corner at the lower end of the column to determine the orientation plate position.\n\n" +

						"• Prism creates an 8 mm thick plate.\n" +
						"• The plate is positioned between 100 mm and 200 mm from the end of the member.\n" +
						"• The plate is shop welded to the column.\n" +
						"• The weld is created with the reference 'P15'."
				}
			};

			Show(
				owner,
				"Detail Orientation Holes",
				"Automatically adds the required orientation detailing to eligible columns in the current selection.",
				sections);
		}

		public static void ShowCreateDrawings(Window owner)
		{
			List<InformationSection> sections = new List<InformationSection>
			{
				new InformationSection
				{
					Title = "Drawing Checks",
					Summary = "Runs both stages of the Drawing Selection checks again before creating any drawings.",
					Details =
						"Prism runs the same drawing checks carried out by the Check Selection button.\n\n" +
						"• Stage 1 checks Name and Class, material order status and Execution Class.\n" +
						"• Stage 2 checks Finish, Intumescent Loading, Start Number matching and Phase Number matching.\n\n" +
						"If any part fails these checks, drawing creation stops and the failures are shown in the results table."
				},

				new InformationSection
				{
					Title = "Create Drawings",
					Summary = "Numbers the selected parts and creates drawings once every required check has passed.",
					Details =
						"Once all checks have passed, Prism performs a Tekla numbering operation.\n" +
						"• The user must accept this numbering before Prism can continue.\n\n" +

						"Prism then creates the required drawings using the '-SEV-GENERAL Drawing Wizard' from the Firm Folder."
				},

				new InformationSection
				{
					Title = "Drawing Classification",
					Summary = "Assigns each piece a drawing classification based on the estimated workload required to complete its fabrication drawing.",
					Details =
						"After the drawings are created, Prism assigns a Drawing Classification to each piece and stores it in SEV-UDA-353.\n\n" +
				
						"The classification gives an indication of the expected workload required to tidy and complete the fabrication drawing. A higher number represents a drawing that is expected to require more work.\n\n" +
				
						"Assembly Drawings\n" +
						"• ASS1 — Default.\n" +
						"• ASS2 — Assembly has 1 to 3 fittings.\n" +
						"• ASS3 — Assembly has 4 to 12 fittings, or has a galvanised finish.\n" +
						"• ASS4 — Assembly has more than 12 fittings, has an HR part or assembly prefix, has an AS assembly prefix, or has a finish ending in M.\n" +
						"• ASS5 — The main part has been identified as abnormal.\n\n" +
				
						"Fitting Drawings\n" +
						"• FIT1 — Default.\n" +
						"• FIT2 — DP, BO, C or PP fitting.\n" +
						"• FIT3 — H, DMP or MP fitting.\n\n" +
				
						"Where more than one assembly rule applies, Prism uses the highest applicable classification."
				},


				new InformationSection
				{
					Title = "Next Steps",
					Summary = "Review and complete the created drawings before continuing with the fabrication process.",
					Details =
						"Once the drawings have been created, they should be reviewed and tidied as required in Tekla.\n\n" +
						"When the drawings are complete and ready for fabrication, return to Prism and use the Fab Pack Issuing tab to continue the workflow."
				}
			};

			Show(
				owner,
				"Create Drawings",
				"Checks the current selection and creates drawings only when all required drawing checks pass.",
				sections);
		}

		public static void ShowFabPackCheck(Window owner)
		{
			List<InformationSection> sections = new List<InformationSection>
			{
				new InformationSection
				{
					Title = "Numbering",
					Summary = "Checks that the selected parts numbering is up to date.",
					Details =
						"• Prism checks the numbering status of the selected model objects.\n\n" +
						"• The selected parts must not have out-of-date numbering before the Fab Pack can be created."
				},

				new InformationSection
				{
					Title = "Previous Steps",
					Summary = "Checks that the required previous Prism workflow steps have been completed.",
					Details =
						"• Prism checks SEV-UDA-120 to ensure the previous Prism step has been completed on all selected parts.\n\n" +
						"• Parts that have not completed the required previous step will fail the selection check."
				},

				new InformationSection
				{
					Title = "Locked Parts",
					Summary = "Checks that the selected parts are available for the Fab Pack operation.",
					Details =
						"• Locked parts can cause issues during a Prism run, therefore any locked parts are rejected.\n\n" +
						"• Locked parts must be unlocked before the Fab Pack can be created."
				},

				new InformationSection
				{
					Title = "Drawing Information",
					Summary = "Checks the drawings associated with each selected part to ensure completeness.",
					Details =
						"• Drawings must have been created for every selected piece. Each main part must have an assembly drawing and each secondary part must have a fitting drawing.\n\n" +
						"• Each drawing must have at least one revision.\n\n" +
						"• Title 1 on each drawing must identify its destination folder, for example ASS, FIT or PRT."
				},

				new InformationSection
				{
					Title = "Next Step",
					Summary = "Once all checks pass, the selection is ready for Fab Pack creation.",
					Details =
						"When the selection checks have passed, enter the required Fab Pack information and use Create Fab Pack to issue the fabrication package.\n\n" +
						"Any problems found by Prism are shown in the validation results so they can be corrected before continuing."
				}
			};

			Show(
				owner,
				"Fab Pack Selection Check",
				"Checks that the selected Tekla model objects are ready for Fab Pack creation. Prism validates the model, workflow and drawing information required before the fabrication package is issued.",
				sections);
		}

		public static void ShowCreateFabPack(Window owner)
		{
			List<InformationSection> sections = new List<InformationSection>
			{
				new InformationSection
				{
					Title = "Initial Checks",
					Summary = "Runs the required Fab Pack checks before any fabrication files are created.",
					Details =
						"• Prism runs the Fab Pack selection checks to confirm the selected parts are ready for fabrication.\n\n" +
						"• The drawing information is also checked again before the Fab Pack is created.\n\n" +
						"• If any required checks fail, Fab Pack creation will stop so the issues can be corrected."
				},

				new InformationSection
				{
					Title = "Seversafe",
					Summary = "Checks whether Seversafe items are present and require additional processing.",
					Details =
						"• If Seversafe items are present in the selection, Prism will ask whether they should be processed.\n\n" +
						"• If required, a Seversafe order will be compiled alongside the main fab pack."
				},

				new InformationSection
				{
					Title = "Bolt Orders",
					Summary = "Checks whether unordered bolts are present and require additional processing.",
					Details =
						"• For each selected bolt group, Prism retrieves data from SEV-UDA-138 and SEV-UDA-350.\n\n" +
						"• These values identify the phase and issue number in which each bolt group was ordered, if applicable.\n\n" +
						"• If these values are empty, Prism assumes the bolts have never been ordered and a bolt order will be compiled alongside the Fab Pack.\n\n" +
						"• If the phase and issue number match the current run, Prism assumes this is a re-run and will order the bolts again.\n\n" +
						"• If the phase and issue number are different from the current run, Prism assumes the bolts have been ordered previously and will not include them in the new order.\n\n" +
						"• If a selected bolt group is included in the new order, Prism updates its UDAs with the order date, phase and issue number for future use."
				},

				new InformationSection
				{
					Title = "Fabrication Drawings",
					Summary = "Creates the drawing package required for fabrication.",
					Details =
						"• Prism gathers the drawing information associated with the selected parts.\n\n" +
						"• Fabrication drawings are printed to PDF and placed into their appropriate Fab Pack folders.\n\n" +
						"• For all assembly drawins, QR codes are created and added to the drawing PDFs."
				},

				new InformationSection
				{
					Title = "BSWX Export",
					Summary = "Creates the BSWX files required for the selected fabrication parts.",
					Details =
						"• Prism exports the required BSWX fabrication information for the selected parts.\n\n" +
						"• The completed BSWX files are included within the Fab Pack."
				},

				new InformationSection
				{
					Title = "Reports and NC Data",
					Summary = "Creates the fabrication reports and NC data required for production.",
					Details =
						"• Prism creates the required fabrication and bolt reports for the selected parts.\n\n" +
						"• NC data is generated for the applicable fabrication drawings.\n\n" +
						"• The required reports are converted to PDF and stored in the Fab Pack."
				},

				new InformationSection
				{
					Title = "NC Verification",
					Summary = "Checks that the expected NC files have been successfully created.",
					Details =
						"• Prism compares the number of required NC files against the NC data created during the Fab Pack run.\n\n" +
						"• If NC data is missing, Prism identifies the affected files and reports the failure.\n\n" +
						"• The number of required NC files is calculated by totalling the number of unique marks in the pack, sometimes the modelled part can fall foul of the pre-determined NC settings" +
						"causing it to fail creation on certain parts.\n\n"
				},

				new InformationSection
				{
					Title = "Prism Attributes",
					Summary = "Updates the Prism attributes on the parts included in the Fab Pack.",
					Details =
						"• Prism updates the relevant workflow attributes on each processed part.\n\n" +
						"• These attributes record the progress of the parts through the Prism fabrication workflow."
				},

				new InformationSection
				{
					Title = "Individual IFCs",
					Summary = "Creates individual IFC files for the selected fabrication parts.",
					Details =
						"• Prism exports the required individual IFC files for the selected parts.\n\n" +
						"• The completed IFC files are stored within the Fab Pack."
				},

				new InformationSection
				{
					Title = "Package Creation",
					Summary = "Completes and packages the generated fabrication files.",
					Details =
						"• Prism creates a zip file containing the completed fabrication package.\n\n" +
						"• The Fab Pack completion information and email are then prepared."
				},

				new InformationSection
				{
					Title = "Project Directories",
					Summary = "Moves the completed Fab Pack to the configured project directories.",
					Details =
						"• By default the creted packages are kept in the model folder.\n\n" +
						"• In settings the user can define alternate directories.\n\n" +
						"• If an alternate has been defined Prism will offer to move relevant packages to these locations at the end of the run."
				},

				new InformationSection
				{
					Title = "Fabiration Model View",
					Summary = "Aview containing all steel in the current phase.",
					Details =
						"• Production requires the Drawing Office to create a view containing only the phase of steel as issued to the works.\n\n" +
						"• As one of the final steps in Fab Pack creation, Prism creates this view.\n\n" +
						"• The view size is based on the geometry of the selection and creates a view filter that shows items with the appropriate Prism UDAs."
				},

				new InformationSection
				{
					Title = "Complete",
					Summary = "The selected parts have completed the Fab Pack process.",
					Details =
						"Once this operation has completed the items are now considered issued to fabrication and the Prism workflow has been completed on the selected members."
				}
			};

			Show(
				owner,
				"Create Fab Pack",
				"Creates the complete fabrication package for the selected parts. Prism produces the required drawings, fabrication files, reports and exports," +
				" verifies the NC data and moves the completed package to the project directories.",
				sections);
		}

		private static void Show(Window owner, string title, string information)
		{
			InformationWindow informationWindow = new InformationWindow(title, information)
			{
				Owner = owner
			};

			informationWindow.ShowDialog();
		}

		private static void Show(Window owner, string title, string information, IList<InformationSection> sections)
		{
			InformationWindow informationWindow = new InformationWindow(title, information, sections)
			{
				Owner = owner
			};

			informationWindow.ShowDialog();
		}
	}
}