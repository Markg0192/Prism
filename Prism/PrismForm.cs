using System;
using System.Diagnostics;
using System.Windows.Forms;
using Tekla.Structures.Dialog;
using Tekla.Structures.Model;

namespace Prism
{
    public partial class PrismForm : PluginFormBase
    {
        private Model Model;
        private ReportManager MyReportManager;
        private DrawingManager MyDrawingManager;
        private SevFolders MyFolderManager;
        private SevModelData ModelData;
        private SevModelEnumerator ModelEnum;
        private PreRunChecks MyPreRunChecks;
        private ModelModifiers MyModelModifiers;
        private BswxExporter MyBswxExporter;
        private string UnavailableLocation = "Sorry, this package location has not been added yet, please try another location.";

        public PrismForm()
        {
            InitializeComponent();
            Model = new Model();
            MyPreRunChecks = new PreRunChecks(Model);
            ModelData = Model.CreateSevModelData();
            MyModelModifiers = new ModelModifiers(Model);
        }

        private void btnRunThroughMaterialChecks_Click(object sender, EventArgs e)
        {
            ModelEnum = Model.CreateSevModelEnumerator();
            MyModelModifiers.MaterialCheckModifier(ModelEnum, 1);
        }

        public void btnMaterialChecksComplete_Click(object sender, EventArgs e)
        {
            ModelEnum = Model.CreateSevModelEnumerator();
            MyModelModifiers.MaterialCheckModifier(ModelEnum, 2);
        }

        private void btnMaterialOrdered_Click(object sender, EventArgs e)
        {
            ModelEnum = Model.CreateSevModelEnumerator();
            MyModelModifiers.MaterialCheckModifier(ModelEnum, 3);
        }

        private void btnRunThroughDetailingChecks_Click(object sender, EventArgs e)
        {
            ModelEnum = Model.CreateSevModelEnumerator();
            MyModelModifiers.DetailingCheckModifier(ModelEnum, 1);
        }

        private void btnDetailingChecksComplete_Click(object sender, EventArgs e)
        {
            ModelEnum = Model.CreateSevModelEnumerator();
            MyModelModifiers.DetailingCheckModifier(ModelEnum, 2);
        }

        private void btnDrawingsCreated_Click(object sender, EventArgs e)
        {
            ModelEnum = Model.CreateSevModelEnumerator();
            MyModelModifiers.DetailingCheckModifier(ModelEnum, 3);
        }

        private void btnCreatePackage_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.AppStarting;
            ModelEnum = Model.CreateSevModelEnumerator();
            MyReportManager = new ReportManager(Model, phaseNumber.Text, issueNumber.Text);
            MyDrawingManager = new DrawingManager(Model, phaseNumber.Text, issueNumber.Text, ModelEnum);
            MyFolderManager = new SevFolders(Model, phaseNumber.Text, issueNumber.Text);
            MyBswxExporter = new BswxExporter();
            MyBswxExporter.ExportBSWX(ModelEnum, MyFolderManager, ModelData, phaseNumber.Text, issueNumber.Text);

            bool packageSNI = cmbPackageLocation.Text == "SNI";
            bool packageSUK = cmbPackageLocation.Text == "SUK";
            bool packageSDB = cmbPackageLocation.Text == "SDB";
            bool packageHarryPeers = cmbPackageLocation.Text == "Harry Peers";
            bool packageDAMStructures = cmbPackageLocation.Text == "DAM Structures";
            
            if (packageSNI)
            {
                StatusLabel.Text = "Checking numbering is up to date";
                if (!MyPreRunChecks.CheckNumberingIsUpToDate(ModelEnum.SelectedModelParts))
                {
                    this.Close();
                    return;
                }

                StatusLabel.Text = "Getting drawings from model selection";
                MyDrawingManager.CreateDrawingList();

                StatusLabel.Text = "Checking all drawings are up to date";
                if (!MyPreRunChecks.CheckDrawingsAreUpToDate(MyDrawingManager.PrismDrawingList))
                {
                    this.Close();
                    return;
                }

                MyFolderManager.CreateFolders();
                MyReportManager.CreateReports(ModelEnum.SelectedModelParts, ModelEnum.SelectedModelBolts, cmbPackageLocation.Text);
                MyDrawingManager.PrintDrawings(StatusLabel);
                MyModelModifiers.MarkAsFabPackComplete(ModelEnum);
                MyFolderManager.RemoveUnusedFolders();
                MyModelModifiers.LockSelected(ModelEnum);
                Cursor = Cursors.Default;
                DialogResult finishBox = MessageBox.Show($"Thanks {ModelData.First}, your fab package is now complete, please attach your fab package, located in your model folder, to the following email and send to the relevent team", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
               
                const string MailNewLine = "%0D%0A";
                if (finishBox == DialogResult.OK)
                {
                    FormIssueEmail("Test@email.com", $"{MyReportManager.ReportPrefix} Fab Issue", 
                                                                                     $" Hello,{MailNewLine}" +
                                                                                     $"{MailNewLine}" +
                                                                                     $"This is the fab package Issue {issueNumber.Text} for phase {phaseNumber.Text} in {ModelData.ProjNumber}, {ModelData.ProjName}.{MailNewLine}" +
                                                                                     $"Please issue this package to the works when possible.{MailNewLine}" +
                                                                                     $"{MailNewLine}" +
                                                                                     $"This fab package contains the following;{MailNewLine}" +
                                                                                     $"{ModelEnum.AssembliesList.Count} Assemblies.{MailNewLine}" +
                                                                                     $"{ModelEnum.SelectedModelParts.Count} Parts.{MailNewLine}" +
                                                                                     $"{MailNewLine}" +
                                                                                     $"Regards,{MailNewLine}{MailNewLine}" +
                                                                                     $"{ModelData.Full}");
                    this.Close();
                }
            }

            if (packageSUK)
            {
                MessageBox.Show(UnavailableLocation);
            }

            if (packageSDB)
            {
                MessageBox.Show(UnavailableLocation);
            }

            if (packageHarryPeers)
            {
                MessageBox.Show(UnavailableLocation);
            }

            if (packageDAMStructures)
            {
                MessageBox.Show(UnavailableLocation);
            }
        }

        private static void FormIssueEmail(string emailAddress, string subject, string body)
        {
            Process.Start("mailto:" + emailAddress + "?subject=" + subject + "&body=" + body);
        }
    }
}

