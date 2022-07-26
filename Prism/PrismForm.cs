using Prism.ButtonOperations;
using System;
using System.Drawing;
using System.Windows.Forms;
using Tekla.Structures.Dialog;
using Tekla.Structures.Model;
using static Prism.Enums;

namespace Prism
{
    public partial class PrismForm : PluginFormBase
    {
        private Model _model;
        //private DrawingManager _myDrawingManager; temporarily not in use
        private PrismProjectData _projectData;
        private SelectedObjects _selectedObjects;

        public PrismForm()
        {
            InitializeComponent();
            _model = new Model();
            _projectData = new PrismProjectData(_model.GetProjectInfo(), _model.GetInfo().ModelPath);
        }

        private void btnRunThroughMaterialChecks_Click(object sender, EventArgs e)
        {
            MaterialStatusLabel.Text = "Working";
            if (!InitialSetup(stageTypes.Prelim1, false)) { return; }

            _selectedObjects.MaterialButton1op(_projectData, (int)stageTypes.Prelim1);

            MaterialStatusLabel.Text = "Complete";
        }

        private void btn_AddStartNumbers_Click(object sender, EventArgs e)
        {
            MaterialStatusLabel.Text = "Working";
            if (!InitialSetup(stageTypes.Prelim2, true)) { return; };

            _selectedObjects.MaterialButton2op(txt_StartNumber.Text, (int)stageTypes.Prelim2, _projectData);

            MaterialStatusLabel.Text = "Complete";
        }

        private void btnOrderMaterial_Click(object sender, EventArgs e)
        {
            MaterialStatusLabel.Text = "Working";
            if (!InitialSetup(stageTypes.Prelim3, true)) { return; };

            _selectedObjects.MaterialButton3op(_projectData, _model.GetProjectInfo(), txt_MaterialPhaseNumber.Text, txt_MaterialIssueNumber.Text, cmb_OrderMaterial.Text, (int)stageTypes.Prelim3, stageTypes.Prelim3);
            _model.CommitChanges();
            MaterialStatusLabel.Text = "Complete";
        }

        private void btnRunThroughDetailingChecks_Click(object sender, EventArgs e)
        {
            DetailingStatusLabel.Text = "Working";
            if (!InitialSetup(stageTypes.Check1, false)) { return; }

            _selectedObjects.DetailButton1op(_projectData, (int)stageTypes.Check1);

            DetailingStatusLabel.Text = "Complete";
        }

        private void btnDetailingChecksComplete_Click(object sender, EventArgs e)
        {
            DetailingStatusLabel.Text = "Working";
            if (!InitialSetup(stageTypes.Check2, true)) { return; }

            _selectedObjects.DetailButton2op(_projectData, (int)stageTypes.Check2);

            DetailingStatusLabel.Text = "Complete";
        }

        private void btn_CreateDrawings_Click(object sender, EventArgs e)
        {
            DetailingStatusLabel.Text = "Working";
            if (!InitialSetup(stageTypes.Check3, true)) { return; }      

            DetailingStatusLabel.Text = _selectedObjects.DetailButton3op(_projectData, (int)stageTypes.Check3);
        }

        private void btn_BoltOrder_Click(object sender, EventArgs e)
        {
            if (!InitialSetup(stageTypes.Bolt, false)) { return; }
            CreatePackageButton.CreateBoltList(phaseNumber.Text, issueNumber.Text, _projectData, txt_SiteDate.Text);
        }

        private void btnCreatePackage_Click(object sender, EventArgs e)
        {
            StatusLabel.Text = "Working";
            Cursor = Cursors.AppStarting;

            if (!InitialSetup(stageTypes.FAB, false)) { return; }
            if (!_selectedObjects.NumbersNotUpToDate) { return; } 
            
            StatusLabel.Text = _selectedObjects.CreateFabPackage(_projectData, cmbPackageLocation.Text, phaseNumber.Text, issueNumber.Text, stageTypes.FAB, txt_SiteDate.Text);
            Cursor = Cursors.Default;
        }

        private bool InitialSetup(stageTypes stageType, bool checkForPreviousSteps)
        {
            _selectedObjects = new SelectedObjects(stageType);

            if (checkForPreviousSteps && !ModelChecker.ArePreviousStepsComplete(_selectedObjects, (int)stageType))
            {
                DetailingStatusLabel.Text = "Cancelled";
                return false;
            }
            return true;
        }

        private void txt_StartNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            AllowNumbersAndDeleteOnly(e);
        }

        private void txt_MaterialIssueNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            AllowNumbersAndDeleteOnly(e);
        }

        private void issueNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            AllowNumbersAndDeleteOnly(e);
        }

        private void AllowNumbersAndDeleteOnly(KeyPressEventArgs e)
        {
            char delete = new char();
            delete = '\b'; //This is the code created when backspace is pressed.
            if (!Char.IsNumber(e.KeyChar) && (e.KeyChar != delete)) //If the key press is neither a number or delete key then ignore.
            {
                e.Handled = true;
                return;
            }
        }

        private void phaseNumber_TextChanged(object sender, EventArgs e)
        {
            if (phaseNumber.Text.Length > 0)
            {
                phaseNumber.BackColor = Color.White;
            }
            else
            {
                phaseNumber.BackColor = Color.LightCoral;
            }
            CheckForFabButton();
        }

        private void issueNumber_TextChanged(object sender, EventArgs e)
        {
            if (issueNumber.Text.Length > 0)
            {
                issueNumber.BackColor = Color.White;
            }
            else
            {
                issueNumber.BackColor = Color.LightCoral;
            }
            CheckForFabButton();
        }

        private void cmbPackageLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPackageLocation.Text == "SNI")
            {
                cmbPackageLocation.BackColor = Color.White;
            }
            else
            {
                cmbPackageLocation.BackColor = Color.LightCoral;
            }
            CheckForFabButton();
        }

        private void CheckForFabButton()
        {
            if (phaseNumber.Text.Length > 0 & issueNumber.Text.Length > 1 && cmbPackageLocation.Text == "SNI")
            {
                btnCreatePackage.Enabled = true;
                btnCreatePackage.BackColor = Color.Chartreuse;
            }
            else
            {
                btnCreatePackage.Enabled = false;
                btnCreatePackage.BackColor = Color.Gainsboro;
            }

        }

        private void txt_StartNumber_TextChanged(object sender, EventArgs e)
        {
            if (txt_StartNumber.Text.Length > 0)
            {
                txt_StartNumber.BackColor = Color.White;
                btn_AddStartNumbers.BackColor = Color.Gold;
                btn_AddStartNumbers.Enabled = true;
            }
            else
            {
                txt_StartNumber.BackColor = Color.LightCoral;
                btn_AddStartNumbers.BackColor = Color.Gainsboro;
                btn_AddStartNumbers.Enabled = false;
            }
        }

        private void txt_MaterialPhaseNumber_TextChanged(object sender, EventArgs e)
        {
            if (txt_MaterialPhaseNumber.Text.Length > 0)
            {
                txt_MaterialPhaseNumber.BackColor = Color.White;
            }
            else
            {
                txt_MaterialPhaseNumber.BackColor = Color.LightCoral;
            }
            CheckForMaterialButton();
        }

        private void txt_MaterialIssueNumber_TextChanged(object sender, EventArgs e)
        {
            if (txt_MaterialIssueNumber.Text.Length > 1)
            {
                txt_MaterialIssueNumber.BackColor = Color.White;
            }
            else
            {
                txt_MaterialIssueNumber.BackColor = Color.LightCoral;
            }
            CheckForMaterialButton();
        }

        private void CheckForMaterialButton()
        {
            if (txt_MaterialIssueNumber.Text.Length > 1 && txt_MaterialPhaseNumber.Text.Length > 0)
            {
                btnOrderMaterial.Enabled = true;
                btnOrderMaterial.BackColor = Color.Chartreuse;
            }
            else
            {
                btnOrderMaterial.Enabled = false;
                btnOrderMaterial.BackColor = Color.Gainsboro;
            }
        }

        private void txt_SiteDate_TextChanged(object sender, EventArgs e)
        {
            if (txt_SiteDate.Text.Length > 7)
            {
                txt_SiteDate.BackColor = Color.White;
            }
            else
            {
                txt_SiteDate.BackColor = Color.Moccasin;
            }
        }
    }
}