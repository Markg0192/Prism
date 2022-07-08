using Prism.ButtonOperations;
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
        //private DrawingManager _myDrawingManager; temporarily not in use
        private PrismProjectData _modelData;
        private SelectedObjects _modelEnum;
        private EmailWriter _myEmailWriter = new EmailWriter();


        public PrismForm()
        {
            InitializeComponent();
            _model = new Model();
            _modelData = new PrismProjectData(_model);
        }

        private void btnRunThroughMaterialChecks_Click(object sender, EventArgs e)
        {  
            MaterialStatusLabel.Text = "Working";
            if (!InitialSetup(1, "PRELIM", false)) { return; }

            _modelEnum.MaterialButton1op(_modelData, 1);

            MaterialStatusLabel.Text = "Complete";
        }

        private void btn_AddStartNumbers_Click(object sender, EventArgs e)
        {
            MaterialStatusLabel.Text = "Working";
            if(!InitialSetup(2, "PRELIM", true, true, txt_StartNumber.Text, null)) { return; } ;

            _modelEnum.MaterialButton2op(txt_StartNumber.Text, 2, _modelData);

            MaterialStatusLabel.Text = "Complete";
        }

        private void btnOrderMaterial_Click(object sender, EventArgs e)
        {
            MaterialStatusLabel.Text = "Working";      
            if(!InitialSetup(3, "PRELIM", true, true, txt_MaterialIssueNumber.Text, txt_MaterialPhaseNumber.Text)) { return; } ;

            _modelEnum.MaterialButton3op(_myEmailWriter, _model, _modelData, txt_MaterialPhaseNumber.Text, txt_MaterialIssueNumber.Text, cmb_OrderMaterial.Text, 3, "PRELIM");

            MaterialStatusLabel.Text = "Complete";
        }

        private void btnRunThroughDetailingChecks_Click(object sender, EventArgs e)
        {
            DetailingStatusLabel.Text = "Working";
            if(!InitialSetup(4, "Check", false)) { return; }

            _modelEnum.DetailButton1op(_modelData, 4);

            DetailingStatusLabel.Text = "Complete";
        }

        private void btnDetailingChecksComplete_Click(object sender, EventArgs e)
        {
            DetailingStatusLabel.Text = "Working";
            if(!InitialSetup(5, "Check", true)) { return; }

            _modelEnum.DetailButton2op(_modelData, 5);

            DetailingStatusLabel.Text = "Complete";
        }

        private void btn_CreateDrawings_Click(object sender, EventArgs e)
        {
            DetailingStatusLabel.Text = "Working";
            if(!InitialSetup(6, "Check", true)) { return; }

            string statusLabelMessage = _modelEnum.DetailButton3op(_modelData, 6);

            DetailingStatusLabel.Text = statusLabelMessage;
        }

        private void btn_BoltOrder_Click(object sender, EventArgs e)
        {
            CreatePackageButton.CreateBoltList(_myEmailWriter, _model, phaseNumber.Text, issueNumber.Text, _modelData);
        }

        private void btnCreatePackage_Click(object sender, EventArgs e)
        {
            StatusLabel.Text = "Working";
            Cursor = Cursors.AppStarting;

            if(!InitialSetup(7, "FAB", false)) { return; }
            if (!_modelEnum.NumbersNotUpToDate) {return;}

            string statusLabelMessage = _modelEnum.CreateFabPackage(_myEmailWriter, _modelData, _model, cmbPackageLocation.Text, phaseNumber.Text, issueNumber.Text, "FAB", 7);

            Cursor = Cursors.Default;
            StatusLabel.Text = statusLabelMessage;          
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

        private bool InitialSetup(int stageNumber, string stageType, bool checkForPreviousSteps, bool checkForInputs = false, string input1 = "", string input2 = "")
        {      
            if (checkForInputs && !ModelChecker.CheckForInputs(stageNumber, input1, input2))
            {          
                DetailingStatusLabel.Text = "Cancelled";
                return false;
            }         

            _modelEnum = new SelectedObjects(stageType);
            
            if (checkForPreviousSteps && !ModelChecker.ArePreviousStepsComplete(_modelEnum, stageNumber))
            {
                DetailingStatusLabel.Text = "Cancelled";
                return false;
            }
            return true;         
        }
    }
}