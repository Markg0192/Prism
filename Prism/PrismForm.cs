using System;
using System.Diagnostics;
using System.Windows.Forms;
using Tekla.Structures.Dialog;
using Tekla.Structures.Model;

namespace Prism
{
    public partial class PrismForm : PluginFormBase
    {
        private Model _model;
        private ReportManager _myReportManager;
        //private DrawingManager _myDrawingManager; temporarily not in use
        private SevFolders _myFolderManager;
        private SevModelData _modelData;
        private SevModelEnumerator _modelEnum;
        private PreRunChecks _myPreRunChecks;
        private ModelModifiers _myModelModifiers;
        private BswxExporter _myBswxExporter;
        private EmailWriter _myEmailWriter = new EmailWriter();
        private PrelimMarker _myPrelimMarker = new PrelimMarker();
        private string _unavailableLocation = "Sorry, this package location has not been added yet, please try another location.";

        public PrismForm()
        {
            InitializeComponent();
            _model = new Model();
            _myPreRunChecks = new PreRunChecks(_model);
            _modelData = _model.CreateSevModelData();
            _myModelModifiers = new ModelModifiers(_model);
        }

        private void btnRunThroughMaterialChecks_Click(object sender, EventArgs e)
        {
            MaterialStatusLabel.Text = "Working";
            int stageNumber = 1;
            string stageType = "PRELIM ";

            bool isValid = InitialSetup(stageNumber, stageType, false)&&
                           _myPreRunChecks.CheckExecutionField(_modelEnum)&& 
                           _myPreRunChecks.CheckNameAndClassAlignment(_modelEnum);
            if (!isValid) { return; }

            _myModelModifiers.ModifyAttributes(_modelEnum, stageNumber);
            MaterialStatusLabel.Text = "Complete";
        }

        private void btn_AddStartNumbers_Click(object sender, EventArgs e)
        {
            MaterialStatusLabel.Text = "Working";
            int stageNumber = 2;
            string stageType = "PRELIM";

            bool isValid = InitialSetup(stageNumber, stageType, true, true, txt_StartNumber.Text, null);
            if (!isValid) { return; }

            _myModelModifiers.AddStartNumbers(_modelEnum, txt_StartNumber.Text);
            _myModelModifiers.ModifyAttributes(_modelEnum, stageNumber);
            MaterialStatusLabel.Text = "Complete";
        }

        private void btnOrderMaterial_Click(object sender, EventArgs e)
        {
            MaterialStatusLabel.Text = "Working";
            int stageNumber = 3;
            string stageType = "PRELIM";
            _myFolderManager = new SevFolders(_model, txt_MaterialPhaseNumber.Text, txt_MaterialIssueNumber.Text);
            _myReportManager = new ReportManager(_model, txt_MaterialPhaseNumber.Text, txt_MaterialIssueNumber.Text);
            _myBswxExporter = new BswxExporter();

            bool isValid = InitialSetup(stageNumber, stageType, true, true, txt_MaterialPhaseNumber.Text, txt_MaterialIssueNumber.Text);
            if(!isValid) { return; }

            _myPrelimMarker.AddPrelimMarks(_modelEnum, _model);
            _myFolderManager.CreateMatFolder();
            _myReportManager.CreateMaterialReports(_modelEnum.SelectedModelParts, cmb_OrderMaterial.Text);
            if (cmb_OrderMaterial.Text != "Omit Material")
            {
                _myBswxExporter.ExportBSWX(_modelEnum, _myFolderManager.MatPath, _modelData, txt_MaterialPhaseNumber.Text, txt_MaterialIssueNumber.Text, stageType);
            }
            else
            {
                _myModelModifiers.MoveAndRenameOmittedMembers(_modelEnum, _model);
                stageNumber = 8;
            }

            _myModelModifiers.ModifyAttributes(_modelEnum, stageNumber);
            DialogResult finishBox = MessageBox.Show($"Thanks {_modelData.First}, your material order is now complete, please forward the following email to the relevant purchasing team", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (finishBox == DialogResult.OK)
            {
                _myEmailWriter.WriteMatEmail(_myReportManager.MatReportPrefix, _modelEnum.SelectedModelParts.Count, txt_MaterialIssueNumber.Text, txt_MaterialPhaseNumber.Text, _modelData.ProjNumber, _modelData.ProjName, _modelData.Full, cmb_OrderMaterial.Text);
            }
            MaterialStatusLabel.Text = "Complete";
        }

        private void btnRunThroughDetailingChecks_Click(object sender, EventArgs e)
        {
            DetailingStatusLabel.Text = "Working";
            int stageNumber = 4;
            string stageType = "Check";

            bool isValid = InitialSetup(stageNumber, stageType, false) &&
                           _myPreRunChecks.RunStage4Checks(_modelEnum);
            if (!isValid) { return; }

            _myModelModifiers.ModifyAttributes(_modelEnum, stageNumber);
            DetailingStatusLabel.Text = "Complete";
        }

        private void btnDetailingChecksComplete_Click(object sender, EventArgs e)
        {
            DetailingStatusLabel.Text = "Working";
            int stageNumber = 5;
            string stageType = "Check";

            bool isValid = InitialSetup(stageNumber, stageType, true) &&
                           _myPreRunChecks.RunStage4Checks(_modelEnum);
            if(!isValid) { return; }

            _myModelModifiers.ModifyAttributes(_modelEnum, stageNumber);
            DetailingStatusLabel.Text = "Complete";
        }

        private void btn_CreateDrawings_Click(object sender, EventArgs e)
        {
            DetailingStatusLabel.Text = "Working";
            int stageNumber = 6;
            string stageType = "Check";

            bool isValid = InitialSetup(stageNumber, stageType, true) &&
                           _myPreRunChecks.RunStage4Checks(_modelEnum);
            if (!isValid) { return; }

            _myModelModifiers.PerformNumbering();

            const string notUpToDateMessage = "Are you happy with your numbering?";
            const string notUpToDateTitle = "Numbering";
            DialogResult result = MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                _myModelModifiers.CreateDrawings(_modelEnum);
            }
            else
            {
                DetailingStatusLabel.Text = "Cancelled";
                return;
            }

            _myModelModifiers.ModifyAttributes(_modelEnum, stageNumber);
            DetailingStatusLabel.Text = "Complete";
        }

        private void btnCreatePackage_Click(object sender, EventArgs e)
        {
            StatusLabel.Text = "Working";
            int stageNumber = 7;
            string stageType = "FAB";
            Cursor = Cursors.AppStarting;
            _modelEnum = _model.CreateSevModelEnumerator(stageType);
            if (!_modelEnum.NumbersNotUpToDate)
            {
                return;
            }
            _myReportManager = new ReportManager(_model, phaseNumber.Text, issueNumber.Text);
            //MyDrawingManager = new DrawingManager(Model, phaseNumber.Text, issueNumber.Text, ModelEnum); Temporarily not in use
            _myFolderManager = new SevFolders(_model, phaseNumber.Text, issueNumber.Text);
            _myBswxExporter = new BswxExporter();

            bool packageSNI = cmbPackageLocation.Text == "SNI";
            bool packageSUK = cmbPackageLocation.Text == "SUK";
            bool packageSDB = cmbPackageLocation.Text == "SDB";
            bool packageHarryPeers = cmbPackageLocation.Text == "Harry Peers";
            bool packageDAMStructures = cmbPackageLocation.Text == "DAM Structures";

            if (packageSNI)
            {
                _myBswxExporter.ExportBSWX(_modelEnum, _myFolderManager.DspPath, _modelData, phaseNumber.Text, issueNumber.Text, stageType);
                _myFolderManager.CreateFabFolders();
                _myReportManager.CreateFabReports(_modelEnum.SelectedModelParts, _modelEnum.SelectedModelBolts, cmbPackageLocation.Text);
                // MyDrawingManager.PrintDrawings(StatusLabel); Temporarily not in use
                _myModelModifiers.ModifyAttributes(_modelEnum, stageNumber);
                //MyFolderManager.RemoveUnusedFolders(); Temporarily not in use
                _myModelModifiers.LockSelected(_modelEnum);
                Cursor = Cursors.Default;
                DialogResult finishBox = MessageBox.Show($"Thanks {_modelData.First}, your fab package is now complete, please attach your fab package, located in your model folder, " +
                    $"to the following email and send to the relevant team. PLEASE NOTE: This version of Prism does NOT print drawings, you will have to do this bit yourself.", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (finishBox == DialogResult.OK)
                {
                    _myEmailWriter.WriteFabEmail(_myReportManager.FabReportPrefix, _modelEnum.AssembliesList.Count, _modelEnum.SelectedModelParts.Count, issueNumber.Text, phaseNumber.Text, _modelData.ProjNumber, _modelData.ProjName, _modelData.Full);
                }
                StatusLabel.Text = "Complete";
            }

            if (packageSUK)
            {
                MessageBox.Show(_unavailableLocation);
            }

            if (packageSDB)
            {
                MessageBox.Show(_unavailableLocation);
            }

            if (packageHarryPeers)
            {
                MessageBox.Show(_unavailableLocation);
            }

            if (packageDAMStructures)
            {
                MessageBox.Show(_unavailableLocation);
            }
        }

        private void txt_StartNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txt_MaterialPhaseNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txt_MaterialIssueNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private bool InitialSetup(int stageNumber, string stageType, bool checkForPreviousSteps, bool checkForInputs=false , string input1="", string input2="")
        {
            bool isValid=true ;
            if (checkForInputs)
            {
                isValid = _myPreRunChecks.CheckAllInputsAreCorrect(stageNumber, input1, input2);
            }

            if (isValid)
            {
                _modelEnum = _model.CreateSevModelEnumerator(stageType);
            }

            if (isValid && checkForPreviousSteps)
            {
                isValid = _myPreRunChecks.CheckPreviousStepsAreComplete(_modelEnum, stageNumber);               
            }

            if (!isValid)
            {
                 DetailingStatusLabel.Text = "Cancelled"; 
            }

            return isValid;
        }
    }
}