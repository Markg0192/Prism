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
                MyDrawingManager.CreateDrawingList(StatusLabel);

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
                DialogResult finishBox = MessageBox.Show($"Thanks {ModelData.First}, your fab package is now complete, please attached your fab package, located in your model folder, to the following email and send to the relevent team", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (finishBox == DialogResult.OK)
                {
                    FormIssueEmail("Test@email.com", "Test Subject", "Some test text for the body");
                    this.Close();
                }
            }

            if (packageSUK)
            {
                MessageBox.Show("Sorry, this package location has not been added yet, please try another location.");
            }

            if (packageSDB)
            {
                MessageBox.Show("Sorry, this package location has not been added yet, please try another location.");
            }

            if (packageHarryPeers)
            {
                MessageBox.Show("Sorry, this package location has not been added yet, please try another location.");
            }

            if (packageDAMStructures)
            {
                MessageBox.Show("Sorry, this package location has not been added yet, please try another location.");
            }
        }

        public static void FormIssueEmail(string emailAddress, string subject, string body)
        {
            Process.Start("mailto:" + emailAddress + "?subject=" + subject + "&body=" + body);
        }
    }
}

