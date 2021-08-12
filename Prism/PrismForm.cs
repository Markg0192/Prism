using System;
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
            ModelData = Model.SevModelData();            
            MyModelModifiers = new ModelModifiers(Model);
        }       

        private void btnRunThroughMaterialChecks_Click(object sender, EventArgs e)
        {
            ModelEnum = Model.SevModelEnumerator();
            MyModelModifiers.MaterialCheckModifier(ModelEnum, 1);
        }

        public void btnMaterialChecksComplete_Click(object sender, EventArgs e)
        {
            ModelEnum = Model.SevModelEnumerator();
            MyModelModifiers.MaterialCheckModifier(ModelEnum, 2);
        }

        private void btnMaterialOrdered_Click(object sender, EventArgs e)
        {
            ModelEnum = Model.SevModelEnumerator();
            MyModelModifiers.MaterialCheckModifier(ModelEnum, 3);
        }

        private void btnRunThroughDetailingChecks_Click(object sender, EventArgs e)
        {
            ModelEnum = Model.SevModelEnumerator();
            MyModelModifiers.DetailingCheckModifier(ModelEnum, 1);
        }

        private void btnDetailingChecksComplete_Click(object sender, EventArgs e)
        {
            ModelEnum = Model.SevModelEnumerator();
            MyModelModifiers.DetailingCheckModifier(ModelEnum, 2);
        }

        private void btnDrawingsCreated_Click(object sender, EventArgs e)
        {
            ModelEnum = Model.SevModelEnumerator();
            MyModelModifiers.DetailingCheckModifier(ModelEnum, 3);
        }    
        
        private void btnCreatePackage_Click(object sender, EventArgs e)
        {
            ModelEnum = Model.SevModelEnumerator();
            MyReportManager = new ReportManager(Model, phaseNumber.Text, issueNumber.Text);
            MyDrawingManager = new DrawingManager(Model, phaseNumber.Text, issueNumber.Text);
            MyFolderManager = new SevFolders(Model, phaseNumber.Text, issueNumber.Text);            
            Cursor = Cursors.AppStarting;
            bool packageSNI = cmbPackageLocation.Text == "SNI";
            bool packageSUK = cmbPackageLocation.Text == "SUK";

            if (packageSNI)
            {
                if (!MyPreRunChecks.CheckDrawingsAreUpToDate(ModelEnum.DrawingEnum))
                {
                    this.Close();
                    return;
                }
                MyFolderManager.CreateFolders();
                MyReportManager.CreateReports(ModelEnum.SelectedModelParts, ModelEnum.SelectedModelBolts, cmbPackageLocation.Text);
                MyDrawingManager.PrintDrawings(ModelEnum.MyDrawingHandler, StatusLabel);
                MyModelModifiers.MarkAsFabPackComplete(ModelEnum);
                MyFolderManager.RemoveUnusedFolders();
                MyModelModifiers.LockSelected(ModelEnum);
            }

            if (packageSUK)
            {
                MessageBox.Show("Sorry, this function has not been added yet, please try again later.");
            }

            Cursor = Cursors.Default;
            DialogResult finishBox = MessageBox.Show($"Thanks {ModelData.First}, your fab package is now complete", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            if (finishBox == DialogResult.OK)
            {
                this.Close();
            }
        }
    }
}

