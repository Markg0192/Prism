using System;
using System.Windows.Forms;
using Tekla.Structures.Dialog;
using Tekla.Structures.Model;

namespace Prism
{
    public partial class PrismForm : PluginFormBase
    {
        private Model model;
        private ReportManager myReportManager;
        private DrawingManager myDrawingManager;
        private SevFolders myFolderManager;
        private SevModelData modelData;
        private SevModelEnumerator modelEnum;
        private PreRunChecks myPreRunChecks;
        private ModelModifiers myModelModifiers;

        public PrismForm()
        {
            InitializeComponent();
            model = new Model();
            myPreRunChecks = new PreRunChecks(model);
            modelData = model.SevModelData();            
            myModelModifiers = new ModelModifiers(model);
        }       
        private void btnRunThroughMaterialChecks_Click(object sender, EventArgs e)
        {
            modelEnum = model.SevModelEnumerator();
            myModelModifiers.RunThroughMaterialChecks(modelEnum);
        }
        public void btnMaterialChecksComplete_Click(object sender, EventArgs e)
        {
            modelEnum = model.SevModelEnumerator();
            myModelModifiers.MaterialChecksComplete(modelEnum);
        }
        private void btnMaterialOrdered_Click(object sender, EventArgs e)
        {
            modelEnum = model.SevModelEnumerator();
            myModelModifiers.MaterialOrderComplete(modelEnum);
        }
        private void btnRunThroughDetailingChecks_Click(object sender, EventArgs e)
        {
            modelEnum = model.SevModelEnumerator();
            myModelModifiers.RunThroughDetailingChecks(modelEnum);
        }
        private void btnDetailingChecksComplete_Click(object sender, EventArgs e)
        {
            modelEnum = model.SevModelEnumerator();
            myModelModifiers.RunThroughDetailingChecksComplete(modelEnum);
        }
        private void btnDrawingsCreated_Click(object sender, EventArgs e)
        {
            modelEnum = model.SevModelEnumerator();
            myModelModifiers.DrawingsCreated(modelEnum);
        }        
        private void btnCreatePackage_Click(object sender, EventArgs e)
        {
            myReportManager = new ReportManager(model, phaseNumber.Text, issueNumber.Text);
            myDrawingManager = new DrawingManager(model, phaseNumber.Text, issueNumber.Text);
            myFolderManager = new SevFolders(model, phaseNumber.Text, issueNumber.Text);            
            Cursor = Cursors.AppStarting;
            bool packageSNI = cmbPackageLocation.Text == "SNI";
            bool packageSUK = cmbPackageLocation.Text == "SUK";

            if (packageSNI)
            {
                if (!myPreRunChecks.CheckDrawingsAreUpToDate(modelEnum.drawingEnum))
                {
                    this.Close();
                    return;
                }
                myFolderManager.CreateFolders();
                myReportManager.CreateReports(modelEnum.selectedModelParts, modelEnum.selectedModelBolts);
                myDrawingManager.PrintDrawings(modelEnum.drawingEnum, StatusLabel);
                myModelModifiers.FabPackComplete(modelEnum);
            }
            if (packageSUK)
            {
                MessageBox.Show("Sorry, this function has not been added yet, please try again later.");
            }
            Cursor = Cursors.Default;
            DialogResult finishBox = MessageBox.Show($"Thanks {modelData.First}, your fab package is now complete", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (finishBox == DialogResult.OK)
            {
                this.Close();
            }
        }
    }
}

