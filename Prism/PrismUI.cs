using Prism.ButtonOperations;
using System;
using System.Drawing;
using System.Windows.Forms;
using Tekla.Structures.Model;
using static Prism.Enums;
using Task = System.Threading.Tasks.Task;

namespace Prism
{
    public partial class PrismUI : Form
    {
        private Model _model;
        private PrismProjectData _projectData;
        private SelectedObjects _selectedObjects;

        public PrismUI()
        {
            InitializeComponent();
            CenterToScreen();
            _model = new Model();

            if (!_model.GetConnectionStatus())
            {
                MessageBox.Show("Failed to connect to a correct version of Tekla Model");
                Logging.DebugLog("Incorrect Connection to model", _model.GetProjectInfo().Name);
                Application.Exit();
            }

            _projectData = new PrismProjectData(_model.GetProjectInfo(), _model.GetInfo().ModelPath);
            Logging.Login(_projectData.ProjName);
        }

        private bool InitialSetup(StageTypes stageType, bool checkForPreviousSteps)
        {
            SetStatusLabels("Gathering Parts");
            ModelChecker.ClearOldLists();
            _selectedObjects = new SelectedObjects(stageType);

            if (checkForPreviousSteps && !ModelChecker.ArePreviousStepsComplete(_selectedObjects, (int)stageType))
            {
                SetStatusLabels("Previous Steps Incomplete");
                return false;
            }
            if (_selectedObjects.NumbersUpToDate && _selectedObjects.AssembliesList.Count == 0)
            {
                SetStatusLabels("No Parts Selected");
                PrismWarnings.NoPartsSelected();
                return false;
            }
            SetStatusLabels("Running Operation");
            return true;
        }

        private async void btn_Material1_Click_1(object sender, EventArgs e)
        {
            StartFunction();

            if (!await Task.Run(() => InitialSetup(StageTypes.Prelim1, false))) { EndFunction(0); return; }

            if (!await Task.Run(() => _selectedObjects.MaterialButton1op(_projectData, (int)StageTypes.Prelim1))) { EndFunction(0); return; }

            ModelModifiers.RedrawViews();

            EndFunction(1);
        }

        private async void btn_Material2_Click_1(object sender, EventArgs e)
        {
            StartFunction();

            if (!await Task.Run(() => InitialSetup(StageTypes.Prelim2, true))) { EndFunction(0); return; };

            if (!await Task.Run(() => _selectedObjects.MaterialButton2op(txt_StartNumber.Text, (int)StageTypes.Prelim2, _projectData, _model))) { EndFunction(0); return; }

            ModelModifiers.RedrawViews();

            EndFunction(1);
        }

        private async void btn_Material3_Click_1(object sender, EventArgs e)
        {
            StartFunction();

            if (!await Task.Run(() => InitialSetup(StageTypes.Prelim3, true))) { EndFunction(0); return; };

            string orderType = cmb_OrderMaterial.Text;
            if (!await Task.Run(() => _selectedObjects.MaterialButton3op(_projectData, _model.GetProjectInfo(), txt_MaterialPhaseNumber.Text, txt_MaterialIssueNumber.Text,
                orderType, (int)StageTypes.Prelim3, StageTypes.Prelim3, _model))){ EndFunction(0); return; }

            _model.CommitChanges();

            EndFunction(1);
        }

        private async void btn_Detail1_Click_1(object sender, EventArgs e)
        {
            StartFunction();

            if (!await Task.Run(() => InitialSetup(StageTypes.Check1, false))) { EndFunction(0); return; }

            await Task.Run(() => _selectedObjects.DetailButton1op(_projectData, (int)StageTypes.Check1));

            EndFunction(1);
        }

        private async void btn_Detail2_Click_1(object sender, EventArgs e)
        {
            StartFunction();


            ModelModifiers.ResetWorkPlane(_model);
            if (!await Task.Run(() => InitialSetup(StageTypes.Check2, true))) { EndFunction(0); return; }
            string orientationType = cmb_ColumnOrientationType.Text; //we need this to avoid cross threading. (unsure why...)
            await Task.Run(() => _selectedObjects.DetailButton2op(_projectData, (int)StageTypes.Check2, orientationType, txt_PlateOnFlange.Text));

            EndFunction(1);
        }

        private async void btn_Detail3_Click_1(object sender, EventArgs e)
        {
            StartFunction();

            if (!await Task.Run(() => InitialSetup(StageTypes.Check3, true))) { EndFunction(0); return; }

            if(!_selectedObjects.DetailButton3op(_projectData, (int)StageTypes.Check3)) { EndFunction(0); return; }

            EndFunction(1);
        }

        private async void btn_FabMisc_Click(object sender, EventArgs e)
        {
            StartFunction();

            if (!await Task.Run(() => InitialSetup(StageTypes.Bolt, false))) { EndFunction(0); return; }

            await Task.Run(() => FabMisc.FabMiscOp(phaseNumber.Text, issueNumber.Text, _projectData, txt_SiteDate.Text, _selectedObjects));

            EndFunction(1);
        }

        private async void btnCreatePackage1_Click_1(object sender, EventArgs e)
        {
            StartFunction();

            if (!await Task.Run(() => InitialSetup(StageTypes.FAB, true))) { EndFunction(0); return; }

            if (!_selectedObjects.NumbersUpToDate) { SetStatusLabels("Numbers not up to date"); EndFunction(0); return; }

            SetStatusLabels("Creating Fab Package");

            await Task.Run(() => _selectedObjects.CreateFabPackage(_model, _projectData, phaseNumber.Text, issueNumber.Text, StageTypes.FAB, txt_SiteDate.Text));

            EndFunction(1);
        }

        private void StartFunction()
        {
            Cursor = Cursors.AppStarting;
            flowLayoutPanel1.Enabled = false;
        }

        private void EndFunction(int cancelledOrComplete) //0 = cancelled 1 = Complete
        {
            string message = cancelledOrComplete == 0 ? "Cancelled" : "Complete";
            flowLayoutPanel1.Enabled = true;
            SetStatusLabels(message);
            Cursor = Cursors.Default;
        }

        private void txt_StartNumber_TextChanged(object sender, EventArgs e)
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

        private void txt_StartNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            AllowNumbersAndDeleteOnly(e);
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

        private void txt_MaterialIssueNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            AllowNumbersAndDeleteOnly(e);
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
            CheckForBoltOrderButton();
        }

        private void issueNumber_TextChanged(object sender, EventArgs e)
        {
            if (issueNumber.Text.Length > 1)
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

        private void issueNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            AllowNumbersAndDeleteOnly(e);
        }

        private void cmbPackageLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            //The logic below will be needed when fab packages are a vaiable option so just commented out for now
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

        private void userGuideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help.Open();
        }

        private void PrismUI_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

            Cursor = Cursors.Help;
        }

        private void info_Mat_Click(object sender, EventArgs e)
        {
            Help.OpenAt("Material Checks", "Index");
        }

        private void info_Detail_Click(object sender, EventArgs e)
        {
            Help.OpenAt("Detailing Checks", "Index");
        }

        private void info_Fab_Click(object sender, EventArgs e)
        {
            Help.OpenAt("Fabrication Packaging", "Index");
        }

        private void info_Home_Click(object sender, EventArgs e)
        {
            Help.OpenAt("Home", "Index");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmb_OrderMaterial_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckForMaterialButton();
        }

        private void btn_ResetPrelims_Click(object sender, EventArgs e)
        {
            bool performReset = PrismWarnings.ResetPrelimMarking();
            if (performReset)
            {
                ModelModifiers.ClearPrelimMarking(_model.GetProjectInfo(), txt_ResetPrelimTo.Text);
                PrismWarnings.PrelimStartReset(txt_ResetPrelimTo.Text);
            }
        }

        private void cmb_ColumnOrientationType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_ColumnOrientationType.Text == "Holes and plate")
            {
                txt_PlateOnFlange.Visible = true;
                lbl_PltOnFlange.Visible = true;
                btn_Detail2.Enabled = false;
                btn_Detail2.BackColor = Color.Gainsboro;
            }
            else
            {
                txt_PlateOnFlange.Text = "";
                txt_PlateOnFlange.Visible = false;
                lbl_PltOnFlange.Visible = false;
                btn_Detail2.Enabled = true;
                btn_Detail2.BackColor = Color.Gold;
            }
        }

        private void txt_PlateOnFlange_TextChanged(object sender, EventArgs e)
        {
            if (txt_PlateOnFlange.Text.Length > 0)
            {
                txt_PlateOnFlange.BackColor = Color.White;
                btn_Detail2.BackColor = Color.Gold;
                btn_Detail2.Enabled = true;
            }
            else
            {
                txt_PlateOnFlange.BackColor = Color.LightCoral;
                btn_Detail2.BackColor = Color.Gainsboro;
                btn_Detail2.Enabled = false;
            }
        }

        private void btn_HomeDetail_Click_1(object sender, EventArgs e)
        {
            pnl_Home.Visible = true;
            pnl_Detail.Visible = false;
        }

        private void btn_HomePackage_Click_1(object sender, EventArgs e)
        {
            pnl_Home.Visible = true;
            pnl_Package.Visible = false;
        }

        private void btn_HomeMaterial_Click_1(object sender, EventArgs e)
        {
            pnl_Home.Visible = true;
            pnl_Material.Visible = false;
        }

        private void btn_MainMaterialCheck_Click_1(object sender, EventArgs e)
        {
            pnl_Material.Visible = true;
            pnl_Home.Visible = false;
        }

        private void btn_MainPackageCreation_Click_1(object sender, EventArgs e)
        {
            pnl_Home.Visible = false;
            pnl_Package.Visible = true;
        }

        private void btn_MainDetailCheck_Click_1(object sender, EventArgs e)
        {
            pnl_Home.Visible = false;
            pnl_Detail.Visible = true;
        }

        private void SetStatusLabels(string message)
        {
            MaterialStatusLabel.Text = message;
            DetailingStatusLabel.Text = message;
            StatusLabel.Text = message;
        }

        private void CheckForFabButton()
        {
            if (phaseNumber.Text.Length > 0 & issueNumber.Text.Length > 1)
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
                btn_FabMisc.Enabled = true;
                btn_FabMisc.BackColor = Color.Chartreuse;
            }
            else
            {
                btn_FabMisc.Enabled = false;
                btn_FabMisc.BackColor = Color.Gainsboro;
            }
        }

        private void CheckForMaterialButton()
        {
            if (txt_MaterialIssueNumber.Text.Length > 1 && txt_MaterialPhaseNumber.Text.Length > 0 && cmb_OrderMaterial.Text != "Order HD Bolts")
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
    }
}