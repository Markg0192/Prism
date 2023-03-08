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

        private bool InitialSetup(StageTypes stageType, bool checkForPreviousSteps)
        {
            _selectedObjects = new SelectedObjects(stageType);

            if (checkForPreviousSteps && !ModelChecker.ArePreviousStepsComplete(_selectedObjects, (int)stageType))
            {
                DetailingStatusLabel.Text = "Cancelled";
                return false;
            }
            return true;
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

        private void CheckForFabButton()
        {
            if (phaseNumber.Text.Length > 0 & issueNumber.Text.Length > 1 && cmbPackageLocation.Text == "SNI")
            {
                btnCreatePackage1.Enabled = true;
                btnCreatePackage1.BackColor = Color.Chartreuse;
            }
            else
            {
                btnCreatePackage1.Enabled = false;
                btnCreatePackage1.BackColor = Color.Gainsboro;
            }
        }

        private void CheckForBoltOrderButton()
        {
            if (phaseNumber.Text.Length > 0 & issueNumber.Text.Length > 1)
            {
                btn_BoltOrder1.Enabled = true;
                btn_BoltOrder1.BackColor = Color.Chartreuse;
            }
            else
            {
                btn_BoltOrder1.Enabled = false;
                btn_BoltOrder1.BackColor = Color.Gainsboro;
            }
        }

        private void CheckForMaterialButton()
        {
            if (txt_MaterialIssueNumber.Text.Length > 1 && txt_MaterialPhaseNumber.Text.Length > 0)
            {
                btn_Material3.Enabled = true;
                btn_Material3.BackColor = Color.Chartreuse;
            }
            else
            {
                btn_Material3.Enabled = false;
                btn_Material3.BackColor = Color.Gainsboro;
            }
        }

        private void btn_Material1_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.AppStarting;
            MaterialStatusLabel.Text = "Working";

            if (!InitialSetup(StageTypes.Prelim1, false)) { return; }

            _selectedObjects.MaterialButton1op(_projectData, (int)StageTypes.Prelim1);

            Cursor = Cursors.Default;
            MaterialStatusLabel.Text = "Complete";
        }

        private void btn_Material2_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.AppStarting;
            MaterialStatusLabel.Text = "Working";

            if (!InitialSetup(StageTypes.Prelim2, true)) { return; };

            _selectedObjects.MaterialButton2op(txt_StartNumber.Text, (int)StageTypes.Prelim2, _projectData, _model);

            Cursor = Cursors.Default;
            MaterialStatusLabel.Text = "Complete";
        }

        private void btn_Material3_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.AppStarting;
            MaterialStatusLabel.Text = "Working";

            if (!InitialSetup(StageTypes.Prelim3, true)) { return; };

            _selectedObjects.MaterialButton3op(_projectData, _model.GetProjectInfo(), txt_MaterialPhaseNumber.Text, txt_MaterialIssueNumber.Text,
                cmb_OrderMaterial.Text, (int)StageTypes.Prelim3, StageTypes.Prelim3, _model);
            _model.CommitChanges();

            Cursor = Cursors.Default;
            MaterialStatusLabel.Text = "Complete";
        }

        private void btn_Detail1_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.AppStarting;
            DetailingStatusLabel.Text = "Working";

            if (!InitialSetup(StageTypes.Check1, false)) { return; }

            _selectedObjects.DetailButton1op(_projectData, (int)StageTypes.Check1);

            Cursor = Cursors.Default;
            DetailingStatusLabel.Text = "Complete";
        }

        private void btn_Detail2_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.AppStarting;
            DetailingStatusLabel.Text = "Working";

            if (!InitialSetup(StageTypes.Check2, true)) { return; }

           // _selectedObjects.DetailButton2op(_projectData, (int)StageTypes.Check2);

            Cursor = Cursors.Default;
            DetailingStatusLabel.Text = "Complete";
        }

        private void btn_Detail3_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.AppStarting;
            DetailingStatusLabel.Text = "Working";

            if (!InitialSetup(StageTypes.Check3, true)) { return; }

            //DetailingStatusLabel.Text = _selectedObjects.DetailButton3op(_projectData, (int)StageTypes.Check3);
            Cursor = Cursors.Default;
        }

        private void btnCreatePackage1_Click(object sender, EventArgs e)
        {
            StatusLabel.Text = "Working";
            Cursor = Cursors.AppStarting;

            if (!InitialSetup(StageTypes.FAB, false)) { return; }
            if (!_selectedObjects.NumbersNotUpToDate) { return; }

            StatusLabel.Text = _selectedObjects.CreateFabPackage(_projectData, cmbPackageLocation.Text, phaseNumber.Text, issueNumber.Text, StageTypes.FAB, txt_SiteDate.Text);

            Cursor = Cursors.Default;
        }

        private void btn_BoltOrder1_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.AppStarting;
            StatusLabel.Text = "Working";

            if (!InitialSetup(StageTypes.Bolt, false)) { return; }
            FabMisc.CreateBoltOrder(phaseNumber.Text, issueNumber.Text, _projectData, txt_SiteDate.Text);
            foreach (BoltArray bolts in _selectedObjects.AllBolts[0])
            {
                bolts.SetUserProperty(ModelUDA.BoltOrderedBy(), _projectData.Full);
                bolts.SetUserProperty(ModelUDA.BoltOrderedDate(), _projectData.Date);
            }

            DetailingStatusLabel.Text = "Complete";
            Cursor = Cursors.Default;
        }

        private void btn_MainMaterialCheck_Click(object sender, EventArgs e)
        {
            pnl_Material.Visible = true;
            pnl_Home.Visible = false;
        }

        private void btn_MainDetailCheck_Click(object sender, EventArgs e)
        {
            pnl_Home.Visible = false;
            pnl_Detail.Visible = true;
        }

        private void btn_MainPackageCreation_Click(object sender, EventArgs e)
        {
            pnl_Home.Visible = false;
            pnl_Package.Visible = true;
        }

        private void btn_HomeMaterial_Click(object sender, EventArgs e)
        {
            pnl_Home.Visible = true;
            pnl_Material.Visible = false;
        }

        private void btn_HomeDetail_Click(object sender, EventArgs e)
        {
            pnl_Home.Visible = true;
            pnl_Detail.Visible = false;
        }

        private void btn_HomePackage_Click(object sender, EventArgs e)
        {
            pnl_Home.Visible = true;
            pnl_Package.Visible = false;
        }

        private void txt_StartNumber_TextChanged_1(object sender, EventArgs e)
        {
            if (txt_StartNumber.Text.Length > 0)
            {
                txt_StartNumber.BackColor = Color.White;
                btn_Material2.BackColor = Color.Gold;
                btn_Material2.Enabled = true;
            }
            else
            {
                txt_StartNumber.BackColor = Color.LightCoral;
                btn_Material2.BackColor = Color.Gainsboro;
                btn_Material2.Enabled = false;
            }
        }

        private void txt_StartNumber_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            AllowNumbersAndDeleteOnly(e);
        }

        private void txt_MaterialIssueNumber_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            AllowNumbersAndDeleteOnly(e);
        }

        private void issueNumber_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            AllowNumbersAndDeleteOnly(e);
        }

        private void phaseNumber_TextChanged_1(object sender, EventArgs e)
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
            CheckForBoltOrderButton();
        }

        private void issueNumber_TextChanged_1(object sender, EventArgs e)
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
            CheckForBoltOrderButton();
        }

        private void cmbPackageLocation_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            //The logic below will be needed when fab packages are ana vailable option so just commented out for now
            /*if (cmbPackageLocation.Text == "SNI")
            {
                cmbPackageLocation.BackColor = Color.White;
            }
            else
            {
                cmbPackageLocation.BackColor = Color.LightCoral;
            }
            CheckForFabButton();*/

        }

        private void txt_MaterialPhaseNumber_TextChanged_1(object sender, EventArgs e)
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

        private void txt_MaterialIssueNumber_TextChanged_1(object sender, EventArgs e)
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

        private void txt_SiteDate_TextChanged_1(object sender, EventArgs e)
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

        private void PrismForm_Load(object sender, EventArgs e)
        {

        }
    }
}