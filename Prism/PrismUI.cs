using MarksWebService;
using Prism.ButtonOperations;
using Prism.CustomDialogs;
using Prism.ExternalService;
using Prism.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures.Model;
using static Prism.Enums;
using Model = Tekla.Structures.Model.Model;
using Task = System.Threading.Tasks.Task;
using TextBox = System.Windows.Forms.TextBox;

namespace Prism
{
    public partial class PrismUI : Form
    {
        private Model _model;
        private PrismProjectData _projectData;
        public static SelectedObjects _selectedObjects;
        private WebService1 _webService;

        public PrismUI()
        {
            InitializeComponent();
            CenterToScreen();
            InitializePrism();
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

            string orderType = $"{cmb_OrderCall.Text} {cmb_OrderMaterial.Text}";

            if (orderType.Contains("Special Fittings"))
            {
                if (!await Task.Run(() => ProcessSpecialFittings(orderType))) return;
            }
            else
            {
                if (!orderType.Contains("Bolts"))
                {
                    if (!await Task.Run(() => InitialSetup(StageTypes.Prelim3, true))) { EndFunction(0); return; };
                }
                else { ModelChecker.ClearOldLists(); }

                if (!await Task.Run(() => _selectedObjects.MaterialButton3op(_projectData, txt_MaterialPhaseNumber.Text, txt_MaterialIssueNumber.Text,
                    orderType, (int)StageTypes.Prelim3, StageTypes.Prelim3, _model, txt_MatSiteDate.Text))) { EndFunction(0); return; }

                _model.CommitChanges();
            }

            SetNextPrelimToUseLabel();

            Logging.AddToMaterialOrderProcessedCount(Constants.ModelProjectInforLocation(_projectData.ProjNumberAndGuid), _webService);



            EndFunction(1);
        }

        private async void btn_Detail1_Click_1(object sender, EventArgs e)
        {
            StartFunction();

            if (!await Task.Run(() => InitialSetup(StageTypes.Check1, false))) { EndFunction(0); return; }

            if (!await Task.Run(() => _selectedObjects.DetailButton1op(_projectData, (int)StageTypes.Check1))) { EndFunction(0); return; }

            EndFunction(1);
        }

        private bool OrderSpecials(int orderAction, string orderType, StageTypes stageType, ReportManager myReportManager)
        {
            bool runDrawings = PrismWarnings.RunSpecialFittingDrawings();
            if (!myReportManager.Folders.CreateMatFolder(false, runDrawings)) return false;

            if (orderAction == 2) //User wants to order using special fitting tags
            {
                ModelModifiers.SelectSpecialTaggedInSelection(_selectedObjects);
                ModelModifiers.AddPrelimMarks(_selectedObjects, _projectData, _webService);
                _selectedObjects = new SelectedObjects(stageType); // we reset selected objects here (because we just changed the selection)
                myReportManager.CreateMaterialReports(_selectedObjects, orderType, stageType);
            }
            if (orderAction == 3) //User wants to order all selected
            {
                ModelModifiers.AddPrelimMarks(_selectedObjects, _projectData, _webService);
                myReportManager.CreateMaterialReports(_selectedObjects, orderType, stageType);
            }

            if (runDrawings)
            {
                ReportManager.SelectDrawingsInDocManager(null);
                DrawingManager dm = new DrawingManager(_model, _projectData, txt_MaterialPhaseNumber.Text, txt_MaterialIssueNumber.Text);
                if (dm.NotLabelledDrawings.Count != 0) { PrismWarnings.IncorrectlyAssignedDrawings(); return false; }

                List<int> drawingCount = new List<int> { 0, dm.AllFittings.Count };
                DrawingManager.PrintAndIssueDrawings(myReportManager.Folders.MatFolder, myReportManager.Folders.MatPath, drawingCount, "\\SPC", 0, 1, myReportManager, false);
                PrismMacroBuilder.ClearPrintDialog();
            }
            return true;
        }

        private async void btn_Detail2_Click_1(object sender, EventArgs e)
        {
            StartFunction();

            ModelModifiers.ResetWorkPlane(_model);
            if (!await Task.Run(() => InitialSetup(StageTypes.Check2, true))) { EndFunction(0); return; }

            string orientationType = cmb_ColumnOrientationType.Text; //we need this to avoid cross threading. (unsure why...)
            if (!await Task.Run(() => _selectedObjects.DetailButton2op(_projectData, (int)StageTypes.Check2, orientationType, txt_PlateOnFlange.Text))) { return; }

            EndFunction(1);
        }

        private async void btn_Detail3_Click_1(object sender, EventArgs e)
        {
            StartFunction();

            if (!await Task.Run(() => InitialSetup(StageTypes.Check3, true))) { EndFunction(0); return; }

            if (!_selectedObjects.DetailButton3op(_projectData, (int)StageTypes.Check3)) { EndFunction(0); return; }

            EndFunction(1);
        }

        private async void btn_FabMisc_Click(object sender, EventArgs e)
        {
            StartFunction();

            if (!await Task.Run(() => InitialSetup(StageTypes.Bolt, false))) { EndFunction(0); return; }

            // await Task.Run(() => FabMisc.FabMiscOp(txt_SiteDate.Text, _selectedObjects, false, null));

            EndFunction(1);
        }

        private void btnCreatePackage1_Click_1(object sender, EventArgs e)
        {
            Logging.DebugLog("create package start", "", _projectData.WebService);
            CreatePackageAsync();
            // CreatePackageNotAsync();
        }

        private void btn_SpecialOperations_Click(object sender, EventArgs e)
        {
         
            PrismWarnings.FabPackComplete(_projectData, true);
         //   _webService.CreateDirectory(50, "\\\\sev-los-fs1\\application data$\\Prism\\BadFile");


            //Logging.AddToMaterialOrderProcessedCount(Constants.ModelProjectInforLocation(_projectData.ProjNumberAndName), _webService);
            //Logging.AddToFabCompleteCount(Constants.ModelProjectInforLocation(_projectData.ProjNumberAndName), _webService);

            /*Logging.LogProgress(_projectData.ProjName, "1", 1, 10, _projectData.WebService);

              int currentLastNumber = Logging.GetLastUsedPrelim(Constants.ModelProjectInforLocation(_projectData.ProjNumberAndName), _projectData.WebService);

              Logging.SetLastUsedPrelim(Constants.ModelProjectInforLocation(_projectData.ProjNumberAndName), 20, _projectData.WebService);

              currentLastNumber = Logging.GetLastUsedPrelim(Constants.ModelProjectInforLocation(_projectData.ProjNumberAndName), _projectData.WebService);

              Logging.UpdateFrozenDrawingCount(Constants.ModelProjectInforLocation(_projectData.ProjNumberAndName), 12, 21, _projectData.WebService);*/
        }

        public bool InitialSetup(StageTypes stageType, bool checkForPreviousSteps)
        {
            SetStatusLabels("Gathering Parts");
            ModelChecker.ClearOldLists();
            _selectedObjects = new SelectedObjects(stageType);

            if (Environment.UserName != "mark.gibson")
            {
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
                if (_selectedObjects.LockedParts.Count > 0)
                {
                    SetStatusLabels("Locked Parts Selected");
                    PrismWarnings.LockedPartsSelected();
                    ModelModifiers.SetPartsRed(_selectedObjects.LockedParts);
                    return false;
                }
            }
            SetStatusLabels("Running Operation");
            return true;
        }

        private async void CreatePackageAsync()
        {
            StartFunction();

            if (!await Task.Run(() => InitialSetup(StageTypes.FAB, true))) { EndFunction(0); return; }

            if (!_selectedObjects.NumbersUpToDate) { SetStatusLabels("Numbers not up to date"); EndFunction(0); return; }

            bool runSeversafe = false;
            int divisionNo = 0;
            if (_selectedObjects.SeversafePresent)
            {
                runSeversafe = PrismWarnings.ShouldSeverSafeBeProcessed();
                if (runSeversafe)
                {
                    divisionNo = PrismWarnings.DivsionFrom();
                    if (divisionNo == 0)
                    {
                        runSeversafe = false;
                        PrismWarnings.SeversafeOrderCancelled();
                    }
                }
            }

            SetStatusLabels("Creating Fab Package");

            Logging.DebugLog("setup complete", "", _projectData.WebService);

            ReportManager myReportManager = new ReportManager(_projectData, phaseNumber.Text, issueNumber.Text);
            if (!await Task.Run(() => _selectedObjects.CreateFabPackage(_model, _projectData, phaseNumber.Text, issueNumber.Text, StageTypes.FAB, txt_SiteDate.Text, runSeversafe))) { EndFunction(0); return; }

            await Task.Run(() => FabMisc.FabMiscOp(_model, txt_SiteDate.Text, _selectedObjects, runSeversafe, myReportManager, divisionNo, _projectData));

            Logging.AddToFabCompleteCount(Constants.ModelProjectInforLocation(_projectData.ProjNumberAndGuid), _webService);

            EndFunction(1);
        }

        private void CreatePackageNotAsync()
        {
            StartFunction();

            InitialSetup(StageTypes.FAB, true);

            if (!_selectedObjects.NumbersUpToDate) { SetStatusLabels("Numbers not up to date"); EndFunction(0); return; }

            SetStatusLabels("Creating Fab Package");

            bool doSeversafeOrder = false;
            if (!_selectedObjects.CreateFabPackage(_model, _projectData, phaseNumber.Text, issueNumber.Text, StageTypes.FAB, txt_SiteDate.Text, doSeversafeOrder)) { EndFunction(0); return; }
            // FabMisc.FabMiscOp(phaseNumber.Text, issueNumber.Text, _projectData, txt_SiteDate.Text, _selectedObjects);

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
            flowLayoutPanel1.BackColor = cancelledOrComplete == 0 ? Color.Tomato : Color.PaleGreen;
            flowLayoutPanel1.Enabled = true;
            SetStatusLabels(message);
            Cursor = Cursors.Default;
        }

        private void InitializePrism()
        {
            SetupWebService();

            CheckModelConnection();

            CheckSpecialUser();

            CompleteSetup();
        }

        private void CompleteSetup()
        {
            Logging.LoginMessage(_projectData.ProjName, _webService, "Login Succesful");

            Logging.CreateModelLog(_projectData, _webService);
            SetNextPrelimToUseLabel();
            SetStatusLabels($"Connected to: {_projectData.ProjNumber}-{_projectData.ProjName}");
        }

        private void CheckSpecialUser()
        {
            if (Constants.SpecialOperationUser())
            {
                btn_SpecialOperations.BackgroundImage = Resources.Gears;
                btn_SpecialOperations.Enabled = true;
            }
        }

        private void CheckModelConnection()
        {
            _model = new Model();
            if (!_model.GetConnectionStatus())
            {
                MessageBox.Show("Failed to connect to a correct version of Tekla Model");
                Logging.LoginMessage("", _webService, "Login Failed");
                Application.Exit();
            }
            _projectData = new PrismProjectData(_model.GetProjectInfo(), _model.GetInfo().ModelPath, _webService);
        }

        private void SetupWebService()
        {
            _webService = new WebService1(); 
            _webService.Url = @"https://webapps.severfield.com/CETExtWebService/ExternalService.asmx";

            AuthHeader soapHead = new AuthHeader();
            SecurityUtils secUtils = new SecurityUtils("Extd6L!u8nO1%qR7");

            soapHead.Username = secUtils.Encrypt(Environment.UserName);
            soapHead.ProgramName = secUtils.Encrypt("Prism");
            soapHead.ProgramVersion = secUtils.Encrypt(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            soapHead.DomainName = secUtils.Encrypt(Environment.UserDomainName);

            _webService.AuthHeaderValue = soapHead;
           

            try { _webService.HelloWorld(); }
            catch
            {
                MessageBox.Show("Failed to connect to the web service, please ensure internet connection. If the problem persists, contact help.");
                Environment.Exit(1);
            }
        }

        private async Task<bool> ProcessSpecialFittings(string orderType)
        {
            ReportManager myReportManager = new ReportManager(_projectData, txt_MaterialPhaseNumber.Text, txt_MaterialIssueNumber.Text);
            if (orderType.Contains("Omit"))
            {

                if (!myReportManager.Folders.CreateMatFolder(false)) { EndFunction(0); return false; };
                myReportManager.CreateMaterialReports(_selectedObjects, orderType, StageTypes.Prelim3);
                if (!MaterialButton3.FinishOrder(_selectedObjects, (int)StageTypes.Prelim3, _projectData, myReportManager.MatReportPrefix,
                    txt_MaterialIssueNumber.Text, txt_MaterialPhaseNumber.Text, orderType, myReportManager, txt_MatSiteDate.Text)) { return false; }
            }
            else
            {
                int orderAction = PrismWarnings.SpecialFittingOrder();

                if (!await Task.Run(() => InitialSetup(StageTypes.Prelim3, true))) { EndFunction(0); return false; };
                if (orderAction == 2 || orderAction == 3) //then user wants to create a material order
                {
                    if (!OrderSpecials(orderAction, orderType, StageTypes.Prelim3, myReportManager)) { EndFunction(0); return false; }
                    if (!MaterialButton3.FinishOrder(_selectedObjects, (int)StageTypes.Prelim3, _projectData, myReportManager.MatReportPrefix,
                        txt_MaterialIssueNumber.Text, txt_MaterialPhaseNumber.Text, orderType, myReportManager, txt_MatSiteDate.Text, true)) { return false; }
                }
                if (orderAction == 4 || orderAction == 5) //then user wants to make drawings
                {
                    //if order action == 4 then the user wants to run drawings on just the tagged stuff.
                    List<Part> selectedParts = orderAction == 4 ? ModelModifiers.SelectSpecialTaggedInSelection(_selectedObjects) : _selectedObjects.SelectedModelParts;
                    ModelModifiers.PerformNumbering();
                    ModelModifiers.CreateDrawings(selectedParts);
                }
            }
            return true;
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

        private void txt_SiteDate_TextChanged(object sender, EventArgs e)
        {
            CheckForAcceptableSiteDate(ref txt_SiteDate);
        }

        private void CheckForAcceptableSiteDate(ref TextBox textBox)
        {
            if (textBox.Text.Length > 7)
            {
                textBox.BackColor = Color.White;
            }
            else
            {
                textBox.BackColor = Color.Moccasin;
            }
        }

        private void userGuideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help.Open(_projectData.WebService);
        }

        private void PrismUI_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            Cursor = Cursors.Help;
        }

        private void info_Mat_Click(object sender, EventArgs e)
        {
            Help.OpenAt("Material Checks", "Index", _projectData.WebService);
        }

        private void info_Detail_Click(object sender, EventArgs e)
        {
            Help.OpenAt("Detailing Checks", "Index", _projectData.WebService);
        }

        private void info_Fab_Click(object sender, EventArgs e)
        {
            Help.OpenAt("Fabrication Packaging", "Index", _projectData.WebService);
        }

        private void info_Home_Click(object sender, EventArgs e)
        {
            Help.OpenAt("Home", "Index", _projectData.WebService);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmb_OrderMaterial_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckForMaterialButton();
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
            flowLayoutPanel1.BackColor = Color.DodgerBlue;
        }

        private void btn_HomePackage_Click_1(object sender, EventArgs e)
        {
            pnl_Home.Visible = true;
            pnl_Package.Visible = false;
            flowLayoutPanel1.BackColor = Color.DodgerBlue;
        }

        private void btn_HomeMaterial_Click_1(object sender, EventArgs e)
        {
            pnl_Home.Visible = true;
            pnl_Material.Visible = false;
            flowLayoutPanel1.BackColor = Color.DodgerBlue;
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

        private void projectUsersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new ProjectControllers(_projectData.ProjNumberAndGuid, _webService, _model.GetProjectInfo());
            form.ShowDialog();
        }

        private void SetNextPrelimToUseLabel()
        {
            lbl_NextPrelim.Text = Logging.GetLastUsedPrelim(_projectData.ProjNumberAndGuid, _projectData.WebService).ToString();
        }

        private void btn_PrelimLabelRefresh_Click(object sender, EventArgs e)
        {
            SetNextPrelimToUseLabel();
        }

        private void uniClassCodesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new UniClass_Codes(_projectData.ProjNumberAndGuid, _webService);
            form.ShowDialog();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new About();
            form.ShowDialog();
        }

        private void txt_MatSiteDate_TextChanged(object sender, EventArgs e)
        {
            CheckForAcceptableSiteDate(ref txt_MatSiteDate);
        }

        private void advancedSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetPrelim resetPrelim = new ResetPrelim(_projectData, _webService);
            resetPrelim.ShowDialog();
            SetNextPrelimToUseLabel();
        }
    }
}