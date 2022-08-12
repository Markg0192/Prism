
namespace Prism
{
    partial class PrismForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrismForm));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.phaseNumber = new System.Windows.Forms.TextBox();
            this.btn_MainPackageCreation = new System.Windows.Forms.Button();
            this.btn_MainDetailCheck = new System.Windows.Forms.Button();
            this.btn_MainMaterialCheck = new System.Windows.Forms.Button();
            this.btn_HomeMaterial = new System.Windows.Forms.Button();
            this.btn_Material2 = new System.Windows.Forms.Button();
            this.btn_Material3 = new System.Windows.Forms.Button();
            this.btn_Material1 = new System.Windows.Forms.Button();
            this.btn_HomeDetail = new System.Windows.Forms.Button();
            this.btn_Detail2 = new System.Windows.Forms.Button();
            this.btn_Detail3 = new System.Windows.Forms.Button();
            this.btn_Detail1 = new System.Windows.Forms.Button();
            this.btn_HomePackage = new System.Windows.Forms.Button();
            this.btn_BoltOrder1 = new System.Windows.Forms.Button();
            this.btnCreatePackage1 = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.pnl_Home = new System.Windows.Forms.Panel();
            this.button7 = new System.Windows.Forms.Button();
            this.pnl_Material = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip4 = new System.Windows.Forms.StatusStrip();
            this.MaterialStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.txt_StartNumber = new System.Windows.Forms.TextBox();
            this.cmb_OrderMaterial = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txt_MaterialIssueNumber = new System.Windows.Forms.TextBox();
            this.txt_MaterialPhaseNumber = new System.Windows.Forms.TextBox();
            this.pnl_Detail = new System.Windows.Forms.Panel();
            this.statusStrip5 = new System.Windows.Forms.StatusStrip();
            this.DetailingStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnl_Package = new System.Windows.Forms.Panel();
            this.statusStrip6 = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.txt_SiteDate = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.cmbPackageLocation = new System.Windows.Forms.ComboBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.issueNumber = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1.SuspendLayout();
            this.pnl_Home.SuspendLayout();
            this.pnl_Material.SuspendLayout();
            this.statusStrip4.SuspendLayout();
            this.pnl_Detail.SuspendLayout();
            this.statusStrip5.SuspendLayout();
            this.pnl_Package.SuspendLayout();
            this.statusStrip6.SuspendLayout();
            this.SuspendLayout();
            // 
            // phaseNumber
            // 
            this.structuresExtender.SetAttributeName(this.phaseNumber, null);
            this.structuresExtender.SetAttributeTypeName(this.phaseNumber, null);
            this.phaseNumber.BackColor = System.Drawing.Color.LightCoral;
            this.structuresExtender.SetBindPropertyName(this.phaseNumber, null);
            this.phaseNumber.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.phaseNumber.Location = new System.Drawing.Point(15, 22);
            this.phaseNumber.Name = "phaseNumber";
            this.phaseNumber.Size = new System.Drawing.Size(124, 20);
            this.phaseNumber.TabIndex = 26;
            this.toolTip1.SetToolTip(this.phaseNumber, "Phase or VO number.\r\n\r\n");
            this.phaseNumber.TextChanged += new System.EventHandler(this.phaseNumber_TextChanged_1);
            // 
            // btn_MainPackageCreation
            // 
            this.structuresExtender.SetAttributeName(this.btn_MainPackageCreation, null);
            this.structuresExtender.SetAttributeTypeName(this.btn_MainPackageCreation, null);
            this.btn_MainPackageCreation.BackColor = System.Drawing.Color.Transparent;
            this.btn_MainPackageCreation.BackgroundImage = global::Prism.Properties.Resources.MyNewWelder1;
            this.btn_MainPackageCreation.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.structuresExtender.SetBindPropertyName(this.btn_MainPackageCreation, null);
            this.btn_MainPackageCreation.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_MainPackageCreation.Location = new System.Drawing.Point(3, 126);
            this.btn_MainPackageCreation.Name = "btn_MainPackageCreation";
            this.btn_MainPackageCreation.Size = new System.Drawing.Size(120, 120);
            this.btn_MainPackageCreation.TabIndex = 0;
            this.toolTip1.SetToolTip(this.btn_MainPackageCreation, "Go to fab window");
            this.btn_MainPackageCreation.UseVisualStyleBackColor = false;
            this.btn_MainPackageCreation.Click += new System.EventHandler(this.btn_MainPackageCreation_Click);
            // 
            // btn_MainDetailCheck
            // 
            this.structuresExtender.SetAttributeName(this.btn_MainDetailCheck, null);
            this.structuresExtender.SetAttributeTypeName(this.btn_MainDetailCheck, null);
            this.btn_MainDetailCheck.BackColor = System.Drawing.Color.Transparent;
            this.btn_MainDetailCheck.BackgroundImage = global::Prism.Properties.Resources.MyNewPen;
            this.btn_MainDetailCheck.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.structuresExtender.SetBindPropertyName(this.btn_MainDetailCheck, null);
            this.btn_MainDetailCheck.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_MainDetailCheck.Location = new System.Drawing.Point(126, 3);
            this.btn_MainDetailCheck.Name = "btn_MainDetailCheck";
            this.btn_MainDetailCheck.Size = new System.Drawing.Size(120, 120);
            this.btn_MainDetailCheck.TabIndex = 0;
            this.toolTip1.SetToolTip(this.btn_MainDetailCheck, "Go to detail window");
            this.btn_MainDetailCheck.UseVisualStyleBackColor = false;
            this.btn_MainDetailCheck.Click += new System.EventHandler(this.btn_MainDetailCheck_Click);
            // 
            // btn_MainMaterialCheck
            // 
            this.structuresExtender.SetAttributeName(this.btn_MainMaterialCheck, null);
            this.structuresExtender.SetAttributeTypeName(this.btn_MainMaterialCheck, null);
            this.btn_MainMaterialCheck.BackColor = System.Drawing.Color.Transparent;
            this.btn_MainMaterialCheck.BackgroundImage = global::Prism.Properties.Resources.order;
            this.btn_MainMaterialCheck.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.structuresExtender.SetBindPropertyName(this.btn_MainMaterialCheck, null);
            this.btn_MainMaterialCheck.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_MainMaterialCheck.Location = new System.Drawing.Point(3, 3);
            this.btn_MainMaterialCheck.Name = "btn_MainMaterialCheck";
            this.btn_MainMaterialCheck.Size = new System.Drawing.Size(120, 120);
            this.btn_MainMaterialCheck.TabIndex = 0;
            this.toolTip1.SetToolTip(this.btn_MainMaterialCheck, "Go to material window");
            this.btn_MainMaterialCheck.UseVisualStyleBackColor = false;
            this.btn_MainMaterialCheck.Click += new System.EventHandler(this.btn_MainMaterialCheck_Click);
            // 
            // btn_HomeMaterial
            // 
            this.structuresExtender.SetAttributeName(this.btn_HomeMaterial, null);
            this.structuresExtender.SetAttributeTypeName(this.btn_HomeMaterial, null);
            this.btn_HomeMaterial.BackColor = System.Drawing.Color.White;
            this.btn_HomeMaterial.BackgroundImage = global::Prism.Properties.Resources.Home;
            this.btn_HomeMaterial.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.structuresExtender.SetBindPropertyName(this.btn_HomeMaterial, null);
            this.btn_HomeMaterial.Location = new System.Drawing.Point(197, 7);
            this.btn_HomeMaterial.Name = "btn_HomeMaterial";
            this.btn_HomeMaterial.Size = new System.Drawing.Size(40, 40);
            this.btn_HomeMaterial.TabIndex = 39;
            this.toolTip1.SetToolTip(this.btn_HomeMaterial, "Home");
            this.btn_HomeMaterial.UseVisualStyleBackColor = false;
            this.btn_HomeMaterial.Click += new System.EventHandler(this.btn_HomeMaterial_Click);
            // 
            // btn_Material2
            // 
            this.structuresExtender.SetAttributeName(this.btn_Material2, null);
            this.structuresExtender.SetAttributeTypeName(this.btn_Material2, null);
            this.btn_Material2.BackColor = System.Drawing.Color.Gainsboro;
            this.btn_Material2.BackgroundImage = global::Prism.Properties.Resources.add;
            this.btn_Material2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.structuresExtender.SetBindPropertyName(this.btn_Material2, null);
            this.btn_Material2.Enabled = false;
            this.btn_Material2.Location = new System.Drawing.Point(15, 69);
            this.btn_Material2.Name = "btn_Material2";
            this.btn_Material2.Size = new System.Drawing.Size(61, 56);
            this.btn_Material2.TabIndex = 31;
            this.toolTip1.SetToolTip(this.btn_Material2, "Add start numbers");
            this.btn_Material2.UseVisualStyleBackColor = false;
            this.btn_Material2.Click += new System.EventHandler(this.btn_Material2_Click);
            // 
            // btn_Material3
            // 
            this.structuresExtender.SetAttributeName(this.btn_Material3, null);
            this.structuresExtender.SetAttributeTypeName(this.btn_Material3, null);
            this.btn_Material3.BackColor = System.Drawing.Color.Gainsboro;
            this.btn_Material3.BackgroundImage = global::Prism.Properties.Resources.order;
            this.btn_Material3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.structuresExtender.SetBindPropertyName(this.btn_Material3, null);
            this.btn_Material3.Enabled = false;
            this.btn_Material3.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btn_Material3.Location = new System.Drawing.Point(15, 131);
            this.btn_Material3.Name = "btn_Material3";
            this.btn_Material3.Size = new System.Drawing.Size(85, 56);
            this.btn_Material3.TabIndex = 30;
            this.toolTip1.SetToolTip(this.btn_Material3, "Run material order");
            this.btn_Material3.UseVisualStyleBackColor = false;
            this.btn_Material3.Click += new System.EventHandler(this.btn_Material3_Click);
            // 
            // btn_Material1
            // 
            this.structuresExtender.SetAttributeName(this.btn_Material1, null);
            this.structuresExtender.SetAttributeTypeName(this.btn_Material1, null);
            this.btn_Material1.BackColor = System.Drawing.Color.Tomato;
            this.btn_Material1.BackgroundImage = global::Prism.Properties.Resources.look;
            this.btn_Material1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.structuresExtender.SetBindPropertyName(this.btn_Material1, null);
            this.btn_Material1.Location = new System.Drawing.Point(15, 7);
            this.btn_Material1.Name = "btn_Material1";
            this.btn_Material1.Size = new System.Drawing.Size(61, 56);
            this.btn_Material1.TabIndex = 29;
            this.toolTip1.SetToolTip(this.btn_Material1, "Check Members");
            this.btn_Material1.UseVisualStyleBackColor = false;
            this.btn_Material1.Click += new System.EventHandler(this.btn_Material1_Click);
            // 
            // btn_HomeDetail
            // 
            this.structuresExtender.SetAttributeName(this.btn_HomeDetail, null);
            this.structuresExtender.SetAttributeTypeName(this.btn_HomeDetail, null);
            this.btn_HomeDetail.BackColor = System.Drawing.Color.White;
            this.btn_HomeDetail.BackgroundImage = global::Prism.Properties.Resources.Home;
            this.btn_HomeDetail.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.structuresExtender.SetBindPropertyName(this.btn_HomeDetail, null);
            this.btn_HomeDetail.Location = new System.Drawing.Point(148, 11);
            this.btn_HomeDetail.Name = "btn_HomeDetail";
            this.btn_HomeDetail.Size = new System.Drawing.Size(40, 40);
            this.btn_HomeDetail.TabIndex = 24;
            this.toolTip1.SetToolTip(this.btn_HomeDetail, "Home");
            this.btn_HomeDetail.UseVisualStyleBackColor = false;
            this.btn_HomeDetail.Click += new System.EventHandler(this.btn_HomeDetail_Click);
            // 
            // btn_Detail2
            // 
            this.structuresExtender.SetAttributeName(this.btn_Detail2, null);
            this.structuresExtender.SetAttributeTypeName(this.btn_Detail2, null);
            this.btn_Detail2.BackColor = System.Drawing.Color.Gold;
            this.btn_Detail2.BackgroundImage = global::Prism.Properties.Resources.tick1;
            this.btn_Detail2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.structuresExtender.SetBindPropertyName(this.btn_Detail2, null);
            this.btn_Detail2.Location = new System.Drawing.Point(15, 67);
            this.btn_Detail2.Name = "btn_Detail2";
            this.btn_Detail2.Size = new System.Drawing.Size(67, 56);
            this.btn_Detail2.TabIndex = 22;
            this.toolTip1.SetToolTip(this.btn_Detail2, "Checks complete");
            this.btn_Detail2.UseVisualStyleBackColor = false;
            this.btn_Detail2.Click += new System.EventHandler(this.btn_Detail2_Click);
            // 
            // btn_Detail3
            // 
            this.structuresExtender.SetAttributeName(this.btn_Detail3, null);
            this.structuresExtender.SetAttributeTypeName(this.btn_Detail3, null);
            this.btn_Detail3.BackColor = System.Drawing.Color.Chartreuse;
            this.btn_Detail3.BackgroundImage = global::Prism.Properties.Resources.MyNewPen;
            this.btn_Detail3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.structuresExtender.SetBindPropertyName(this.btn_Detail3, null);
            this.btn_Detail3.Location = new System.Drawing.Point(15, 129);
            this.btn_Detail3.Name = "btn_Detail3";
            this.btn_Detail3.Size = new System.Drawing.Size(67, 56);
            this.btn_Detail3.TabIndex = 21;
            this.toolTip1.SetToolTip(this.btn_Detail3, "Create drawings");
            this.btn_Detail3.UseVisualStyleBackColor = false;
            this.btn_Detail3.Click += new System.EventHandler(this.btn_Detail3_Click);
            // 
            // btn_Detail1
            // 
            this.structuresExtender.SetAttributeName(this.btn_Detail1, null);
            this.structuresExtender.SetAttributeTypeName(this.btn_Detail1, null);
            this.btn_Detail1.BackColor = System.Drawing.Color.Tomato;
            this.btn_Detail1.BackgroundImage = global::Prism.Properties.Resources.look;
            this.btn_Detail1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.structuresExtender.SetBindPropertyName(this.btn_Detail1, null);
            this.btn_Detail1.Location = new System.Drawing.Point(15, 7);
            this.btn_Detail1.Name = "btn_Detail1";
            this.btn_Detail1.Size = new System.Drawing.Size(67, 56);
            this.btn_Detail1.TabIndex = 20;
            this.toolTip1.SetToolTip(this.btn_Detail1, "Check members");
            this.btn_Detail1.UseVisualStyleBackColor = false;
            this.btn_Detail1.Click += new System.EventHandler(this.btn_Detail1_Click);
            // 
            // btn_HomePackage
            // 
            this.structuresExtender.SetAttributeName(this.btn_HomePackage, null);
            this.structuresExtender.SetAttributeTypeName(this.btn_HomePackage, null);
            this.btn_HomePackage.BackColor = System.Drawing.Color.White;
            this.btn_HomePackage.BackgroundImage = global::Prism.Properties.Resources.Home;
            this.btn_HomePackage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.structuresExtender.SetBindPropertyName(this.btn_HomePackage, null);
            this.btn_HomePackage.Location = new System.Drawing.Point(203, 5);
            this.btn_HomePackage.Name = "btn_HomePackage";
            this.btn_HomePackage.Size = new System.Drawing.Size(40, 40);
            this.btn_HomePackage.TabIndex = 37;
            this.btn_HomePackage.Text = "\r\n";
            this.toolTip1.SetToolTip(this.btn_HomePackage, "Home");
            this.btn_HomePackage.UseVisualStyleBackColor = false;
            this.btn_HomePackage.Click += new System.EventHandler(this.btn_HomePackage_Click);
            // 
            // btn_BoltOrder1
            // 
            this.structuresExtender.SetAttributeName(this.btn_BoltOrder1, null);
            this.structuresExtender.SetAttributeTypeName(this.btn_BoltOrder1, null);
            this.btn_BoltOrder1.BackColor = System.Drawing.Color.Chartreuse;
            this.btn_BoltOrder1.BackgroundImage = global::Prism.Properties.Resources.bolt;
            this.btn_BoltOrder1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.structuresExtender.SetBindPropertyName(this.btn_BoltOrder1, null);
            this.btn_BoltOrder1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_BoltOrder1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btn_BoltOrder1.Location = new System.Drawing.Point(191, 69);
            this.btn_BoltOrder1.Name = "btn_BoltOrder1";
            this.btn_BoltOrder1.Size = new System.Drawing.Size(52, 45);
            this.btn_BoltOrder1.TabIndex = 33;
            this.toolTip1.SetToolTip(this.btn_BoltOrder1, "Create bolt order");
            this.btn_BoltOrder1.UseVisualStyleBackColor = false;
            this.btn_BoltOrder1.Click += new System.EventHandler(this.btn_BoltOrder1_Click);
            // 
            // btnCreatePackage1
            // 
            this.structuresExtender.SetAttributeName(this.btnCreatePackage1, null);
            this.structuresExtender.SetAttributeTypeName(this.btnCreatePackage1, null);
            this.btnCreatePackage1.BackColor = System.Drawing.Color.Gainsboro;
            this.btnCreatePackage1.BackgroundImage = global::Prism.Properties.Resources.MyNewWelder1;
            this.btnCreatePackage1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.structuresExtender.SetBindPropertyName(this.btnCreatePackage1, null);
            this.btnCreatePackage1.Enabled = false;
            this.btnCreatePackage1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCreatePackage1.Location = new System.Drawing.Point(160, 127);
            this.btnCreatePackage1.Name = "btnCreatePackage1";
            this.btnCreatePackage1.Size = new System.Drawing.Size(83, 69);
            this.btnCreatePackage1.TabIndex = 34;
            this.toolTip1.SetToolTip(this.btnCreatePackage1, "Create fab package");
            this.btnCreatePackage1.UseVisualStyleBackColor = false;
            this.btnCreatePackage1.Click += new System.EventHandler(this.btnCreatePackage1_Click);
            // 
            // flowLayoutPanel1
            // 
            this.structuresExtender.SetAttributeName(this.flowLayoutPanel1, null);
            this.structuresExtender.SetAttributeTypeName(this.flowLayoutPanel1, null);
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.structuresExtender.SetBindPropertyName(this.flowLayoutPanel1, null);
            this.flowLayoutPanel1.Controls.Add(this.pnl_Home);
            this.flowLayoutPanel1.Controls.Add(this.pnl_Material);
            this.flowLayoutPanel1.Controls.Add(this.pnl_Detail);
            this.flowLayoutPanel1.Controls.Add(this.pnl_Package);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(1, 1);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(977, 282);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // pnl_Home
            // 
            this.structuresExtender.SetAttributeName(this.pnl_Home, null);
            this.structuresExtender.SetAttributeTypeName(this.pnl_Home, null);
            this.pnl_Home.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pnl_Home.BackgroundImage = global::Prism.Properties.Resources.watereddownlogo;
            this.pnl_Home.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.structuresExtender.SetBindPropertyName(this.pnl_Home, null);
            this.pnl_Home.Controls.Add(this.button7);
            this.pnl_Home.Controls.Add(this.btn_MainPackageCreation);
            this.pnl_Home.Controls.Add(this.btn_MainDetailCheck);
            this.pnl_Home.Controls.Add(this.btn_MainMaterialCheck);
            this.pnl_Home.Location = new System.Drawing.Point(3, 3);
            this.pnl_Home.Name = "pnl_Home";
            this.pnl_Home.Size = new System.Drawing.Size(249, 249);
            this.pnl_Home.TabIndex = 5;
            // 
            // button7
            // 
            this.structuresExtender.SetAttributeName(this.button7, null);
            this.structuresExtender.SetAttributeTypeName(this.button7, null);
            this.button7.BackColor = System.Drawing.Color.Transparent;
            this.structuresExtender.SetBindPropertyName(this.button7, null);
            this.button7.Enabled = false;
            this.button7.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button7.Location = new System.Drawing.Point(126, 126);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(120, 120);
            this.button7.TabIndex = 0;
            this.button7.UseVisualStyleBackColor = false;
            // 
            // pnl_Material
            // 
            this.structuresExtender.SetAttributeName(this.pnl_Material, null);
            this.structuresExtender.SetAttributeTypeName(this.pnl_Material, null);
            this.pnl_Material.BackColor = System.Drawing.Color.White;
            this.structuresExtender.SetBindPropertyName(this.pnl_Material, null);
            this.pnl_Material.Controls.Add(this.label4);
            this.pnl_Material.Controls.Add(this.label1);
            this.pnl_Material.Controls.Add(this.btn_HomeMaterial);
            this.pnl_Material.Controls.Add(this.statusStrip4);
            this.pnl_Material.Controls.Add(this.txt_StartNumber);
            this.pnl_Material.Controls.Add(this.cmb_OrderMaterial);
            this.pnl_Material.Controls.Add(this.label3);
            this.pnl_Material.Controls.Add(this.txt_MaterialIssueNumber);
            this.pnl_Material.Controls.Add(this.txt_MaterialPhaseNumber);
            this.pnl_Material.Controls.Add(this.btn_Material2);
            this.pnl_Material.Controls.Add(this.btn_Material3);
            this.pnl_Material.Controls.Add(this.btn_Material1);
            this.pnl_Material.Location = new System.Drawing.Point(258, 3);
            this.pnl_Material.Name = "pnl_Material";
            this.pnl_Material.Size = new System.Drawing.Size(255, 276);
            this.pnl_Material.TabIndex = 4;
            this.pnl_Material.Visible = false;
            // 
            // label4
            // 
            this.structuresExtender.SetAttributeName(this.label4, null);
            this.structuresExtender.SetAttributeTypeName(this.label4, null);
            this.label4.AutoSize = true;
            this.structuresExtender.SetBindPropertyName(this.label4, null);
            this.label4.Location = new System.Drawing.Point(134, 193);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 13);
            this.label4.TabIndex = 41;
            this.label4.Text = "Issue Number";
            // 
            // label1
            // 
            this.structuresExtender.SetAttributeName(this.label1, null);
            this.structuresExtender.SetAttributeTypeName(this.label1, null);
            this.label1.AutoSize = true;
            this.structuresExtender.SetBindPropertyName(this.label1, null);
            this.label1.Location = new System.Drawing.Point(103, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 40;
            this.label1.Text = "Start Number";
            // 
            // statusStrip4
            // 
            this.structuresExtender.SetAttributeName(this.statusStrip4, null);
            this.structuresExtender.SetAttributeTypeName(this.statusStrip4, null);
            this.structuresExtender.SetBindPropertyName(this.statusStrip4, null);
            this.statusStrip4.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip4.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MaterialStatusLabel});
            this.statusStrip4.Location = new System.Drawing.Point(0, 254);
            this.statusStrip4.Name = "statusStrip4";
            this.statusStrip4.Size = new System.Drawing.Size(255, 22);
            this.statusStrip4.TabIndex = 38;
            this.statusStrip4.Text = "statusStrip4";
            // 
            // MaterialStatusLabel
            // 
            this.MaterialStatusLabel.Name = "MaterialStatusLabel";
            this.MaterialStatusLabel.Size = new System.Drawing.Size(39, 17);
            this.MaterialStatusLabel.Text = "Status";
            // 
            // txt_StartNumber
            // 
            this.structuresExtender.SetAttributeName(this.txt_StartNumber, null);
            this.structuresExtender.SetAttributeTypeName(this.txt_StartNumber, null);
            this.txt_StartNumber.BackColor = System.Drawing.Color.LightCoral;
            this.structuresExtender.SetBindPropertyName(this.txt_StartNumber, null);
            this.txt_StartNumber.Location = new System.Drawing.Point(106, 88);
            this.txt_StartNumber.Name = "txt_StartNumber";
            this.txt_StartNumber.Size = new System.Drawing.Size(100, 20);
            this.txt_StartNumber.TabIndex = 37;
            this.txt_StartNumber.TextChanged += new System.EventHandler(this.txt_StartNumber_TextChanged_1);
            this.txt_StartNumber.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txt_StartNumber_KeyPress_1);
            // 
            // cmb_OrderMaterial
            // 
            this.structuresExtender.SetAttributeName(this.cmb_OrderMaterial, null);
            this.structuresExtender.SetAttributeTypeName(this.cmb_OrderMaterial, null);
            this.structuresExtender.SetBindPropertyName(this.cmb_OrderMaterial, null);
            this.cmb_OrderMaterial.FormattingEnabled = true;
            this.cmb_OrderMaterial.Items.AddRange(new object[] {
            "Order Material",
            "Add Material",
            "Omit Material"});
            this.cmb_OrderMaterial.Location = new System.Drawing.Point(106, 150);
            this.cmb_OrderMaterial.Name = "cmb_OrderMaterial";
            this.cmb_OrderMaterial.Size = new System.Drawing.Size(100, 21);
            this.cmb_OrderMaterial.TabIndex = 36;
            this.cmb_OrderMaterial.Text = "Order Material";
            // 
            // label3
            // 
            this.structuresExtender.SetAttributeName(this.label3, null);
            this.structuresExtender.SetAttributeTypeName(this.label3, null);
            this.label3.AutoSize = true;
            this.structuresExtender.SetBindPropertyName(this.label3, null);
            this.label3.Location = new System.Drawing.Point(12, 193);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 35;
            this.label3.Text = "Phase Number";
            // 
            // txt_MaterialIssueNumber
            // 
            this.structuresExtender.SetAttributeName(this.txt_MaterialIssueNumber, null);
            this.structuresExtender.SetAttributeTypeName(this.txt_MaterialIssueNumber, null);
            this.txt_MaterialIssueNumber.BackColor = System.Drawing.Color.LightCoral;
            this.structuresExtender.SetBindPropertyName(this.txt_MaterialIssueNumber, null);
            this.txt_MaterialIssueNumber.Location = new System.Drawing.Point(137, 216);
            this.txt_MaterialIssueNumber.Name = "txt_MaterialIssueNumber";
            this.txt_MaterialIssueNumber.Size = new System.Drawing.Size(100, 20);
            this.txt_MaterialIssueNumber.TabIndex = 32;
            this.txt_MaterialIssueNumber.TextChanged += new System.EventHandler(this.txt_MaterialIssueNumber_TextChanged_1);
            this.txt_MaterialIssueNumber.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txt_MaterialIssueNumber_KeyPress_1);
            // 
            // txt_MaterialPhaseNumber
            // 
            this.structuresExtender.SetAttributeName(this.txt_MaterialPhaseNumber, null);
            this.structuresExtender.SetAttributeTypeName(this.txt_MaterialPhaseNumber, null);
            this.txt_MaterialPhaseNumber.BackColor = System.Drawing.Color.LightCoral;
            this.structuresExtender.SetBindPropertyName(this.txt_MaterialPhaseNumber, null);
            this.txt_MaterialPhaseNumber.Location = new System.Drawing.Point(15, 216);
            this.txt_MaterialPhaseNumber.Name = "txt_MaterialPhaseNumber";
            this.txt_MaterialPhaseNumber.Size = new System.Drawing.Size(100, 20);
            this.txt_MaterialPhaseNumber.TabIndex = 33;
            this.txt_MaterialPhaseNumber.TextChanged += new System.EventHandler(this.txt_MaterialPhaseNumber_TextChanged_1);
            // 
            // pnl_Detail
            // 
            this.structuresExtender.SetAttributeName(this.pnl_Detail, null);
            this.structuresExtender.SetAttributeTypeName(this.pnl_Detail, null);
            this.pnl_Detail.BackColor = System.Drawing.Color.White;
            this.structuresExtender.SetBindPropertyName(this.pnl_Detail, null);
            this.pnl_Detail.Controls.Add(this.btn_HomeDetail);
            this.pnl_Detail.Controls.Add(this.statusStrip5);
            this.pnl_Detail.Controls.Add(this.btn_Detail2);
            this.pnl_Detail.Controls.Add(this.btn_Detail3);
            this.pnl_Detail.Controls.Add(this.btn_Detail1);
            this.pnl_Detail.Location = new System.Drawing.Point(519, 3);
            this.pnl_Detail.Name = "pnl_Detail";
            this.pnl_Detail.Size = new System.Drawing.Size(191, 236);
            this.pnl_Detail.TabIndex = 2;
            this.pnl_Detail.Visible = false;
            // 
            // statusStrip5
            // 
            this.structuresExtender.SetAttributeName(this.statusStrip5, null);
            this.structuresExtender.SetAttributeTypeName(this.statusStrip5, null);
            this.structuresExtender.SetBindPropertyName(this.statusStrip5, null);
            this.statusStrip5.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip5.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DetailingStatusLabel});
            this.statusStrip5.Location = new System.Drawing.Point(0, 214);
            this.statusStrip5.Name = "statusStrip5";
            this.statusStrip5.Size = new System.Drawing.Size(191, 22);
            this.statusStrip5.TabIndex = 23;
            this.statusStrip5.Text = "statusStrip5";
            // 
            // DetailingStatusLabel
            // 
            this.DetailingStatusLabel.Name = "DetailingStatusLabel";
            this.DetailingStatusLabel.Size = new System.Drawing.Size(39, 17);
            this.DetailingStatusLabel.Text = "Status";
            // 
            // pnl_Package
            // 
            this.structuresExtender.SetAttributeName(this.pnl_Package, null);
            this.structuresExtender.SetAttributeTypeName(this.pnl_Package, null);
            this.pnl_Package.BackColor = System.Drawing.Color.White;
            this.structuresExtender.SetBindPropertyName(this.pnl_Package, null);
            this.pnl_Package.Controls.Add(this.btn_HomePackage);
            this.pnl_Package.Controls.Add(this.statusStrip6);
            this.pnl_Package.Controls.Add(this.txt_SiteDate);
            this.pnl_Package.Controls.Add(this.btn_BoltOrder1);
            this.pnl_Package.Controls.Add(this.btnCreatePackage1);
            this.pnl_Package.Controls.Add(this.label16);
            this.pnl_Package.Controls.Add(this.label17);
            this.pnl_Package.Controls.Add(this.cmbPackageLocation);
            this.pnl_Package.Controls.Add(this.label18);
            this.pnl_Package.Controls.Add(this.label19);
            this.pnl_Package.Controls.Add(this.phaseNumber);
            this.pnl_Package.Controls.Add(this.issueNumber);
            this.pnl_Package.Location = new System.Drawing.Point(716, 3);
            this.pnl_Package.Name = "pnl_Package";
            this.pnl_Package.Size = new System.Drawing.Size(258, 227);
            this.pnl_Package.TabIndex = 3;
            this.pnl_Package.Visible = false;
            // 
            // statusStrip6
            // 
            this.structuresExtender.SetAttributeName(this.statusStrip6, null);
            this.structuresExtender.SetAttributeTypeName(this.statusStrip6, null);
            this.structuresExtender.SetBindPropertyName(this.statusStrip6, null);
            this.statusStrip6.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip6.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            this.statusStrip6.Location = new System.Drawing.Point(0, 205);
            this.statusStrip6.Name = "statusStrip6";
            this.statusStrip6.Size = new System.Drawing.Size(258, 22);
            this.statusStrip6.TabIndex = 36;
            this.statusStrip6.Text = "statusStrip6";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(39, 17);
            this.StatusLabel.Text = "Status";
            // 
            // txt_SiteDate
            // 
            this.structuresExtender.SetAttributeName(this.txt_SiteDate, null);
            this.structuresExtender.SetAttributeTypeName(this.txt_SiteDate, null);
            this.txt_SiteDate.BackColor = System.Drawing.Color.Moccasin;
            this.structuresExtender.SetBindPropertyName(this.txt_SiteDate, null);
            this.txt_SiteDate.Location = new System.Drawing.Point(15, 176);
            this.txt_SiteDate.Name = "txt_SiteDate";
            this.txt_SiteDate.Size = new System.Drawing.Size(124, 20);
            this.txt_SiteDate.TabIndex = 35;
            this.txt_SiteDate.TextChanged += new System.EventHandler(this.txt_SiteDate_TextChanged_1);
            // 
            // label16
            // 
            this.structuresExtender.SetAttributeName(this.label16, null);
            this.structuresExtender.SetAttributeTypeName(this.label16, null);
            this.label16.AutoSize = true;
            this.structuresExtender.SetBindPropertyName(this.label16, null);
            this.label16.Location = new System.Drawing.Point(12, 159);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(99, 13);
            this.label16.TabIndex = 31;
            this.label16.Text = "Site Date (Optional)";
            // 
            // label17
            // 
            this.structuresExtender.SetAttributeName(this.label17, null);
            this.structuresExtender.SetAttributeTypeName(this.label17, null);
            this.label17.AutoSize = true;
            this.structuresExtender.SetBindPropertyName(this.label17, null);
            this.label17.Location = new System.Drawing.Point(12, 108);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(94, 13);
            this.label17.TabIndex = 32;
            this.label17.Text = "Package Location";
            // 
            // cmbPackageLocation
            // 
            this.structuresExtender.SetAttributeName(this.cmbPackageLocation, null);
            this.structuresExtender.SetAttributeTypeName(this.cmbPackageLocation, null);
            this.cmbPackageLocation.BackColor = System.Drawing.Color.LightCoral;
            this.structuresExtender.SetBindPropertyName(this.cmbPackageLocation, null);
            this.cmbPackageLocation.FormattingEnabled = true;
            this.cmbPackageLocation.Items.AddRange(new object[] {
            "SNI",
            "SUK",
            "SDB",
            "Harry Peers",
            "DAM Structures"});
            this.cmbPackageLocation.Location = new System.Drawing.Point(15, 125);
            this.cmbPackageLocation.Name = "cmbPackageLocation";
            this.cmbPackageLocation.Size = new System.Drawing.Size(121, 21);
            this.cmbPackageLocation.TabIndex = 30;
            this.cmbPackageLocation.SelectedIndexChanged += new System.EventHandler(this.cmbPackageLocation_SelectedIndexChanged_1);
            // 
            // label18
            // 
            this.structuresExtender.SetAttributeName(this.label18, null);
            this.structuresExtender.SetAttributeTypeName(this.label18, null);
            this.label18.AutoSize = true;
            this.structuresExtender.SetBindPropertyName(this.label18, null);
            this.label18.Location = new System.Drawing.Point(12, 56);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(118, 13);
            this.label18.TabIndex = 29;
            this.label18.Text = "Package Issue Number";
            // 
            // label19
            // 
            this.structuresExtender.SetAttributeName(this.label19, null);
            this.structuresExtender.SetAttributeTypeName(this.label19, null);
            this.label19.AutoSize = true;
            this.structuresExtender.SetBindPropertyName(this.label19, null);
            this.label19.Location = new System.Drawing.Point(12, 5);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(103, 13);
            this.label19.TabIndex = 28;
            this.label19.Text = "Package Reference";
            // 
            // issueNumber
            // 
            this.structuresExtender.SetAttributeName(this.issueNumber, null);
            this.structuresExtender.SetAttributeTypeName(this.issueNumber, null);
            this.issueNumber.BackColor = System.Drawing.Color.LightCoral;
            this.structuresExtender.SetBindPropertyName(this.issueNumber, null);
            this.issueNumber.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.issueNumber.Location = new System.Drawing.Point(15, 73);
            this.issueNumber.Name = "issueNumber";
            this.issueNumber.Size = new System.Drawing.Size(124, 20);
            this.issueNumber.TabIndex = 27;
            this.issueNumber.TextChanged += new System.EventHandler(this.issueNumber_TextChanged_1);
            this.issueNumber.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.issueNumber_KeyPress_1);
            // 
            // PrismForm
            // 
            this.structuresExtender.SetAttributeName(this, null);
            this.structuresExtender.SetAttributeTypeName(this, null);
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.structuresExtender.SetBindPropertyName(this, null);
            this.ClientSize = new System.Drawing.Size(982, 288);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(2000, 2000);
            this.MinimumSize = new System.Drawing.Size(100, 100);
            this.Name = "PrismForm";
            this.Text = "Prism";
            this.toolTip1.SetToolTip(this, "Phase or VO no.");
            this.flowLayoutPanel1.ResumeLayout(false);
            this.pnl_Home.ResumeLayout(false);
            this.pnl_Material.ResumeLayout(false);
            this.pnl_Material.PerformLayout();
            this.statusStrip4.ResumeLayout(false);
            this.statusStrip4.PerformLayout();
            this.pnl_Detail.ResumeLayout(false);
            this.pnl_Detail.PerformLayout();
            this.statusStrip5.ResumeLayout(false);
            this.statusStrip5.PerformLayout();
            this.pnl_Package.ResumeLayout(false);
            this.pnl_Package.PerformLayout();
            this.statusStrip6.ResumeLayout(false);
            this.statusStrip6.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel pnl_Home;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button btn_MainPackageCreation;
        private System.Windows.Forms.Button btn_MainDetailCheck;
        private System.Windows.Forms.Button btn_MainMaterialCheck;
        private System.Windows.Forms.Panel pnl_Material;
        public System.Windows.Forms.TextBox txt_StartNumber;
        private System.Windows.Forms.ComboBox cmb_OrderMaterial;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox txt_MaterialIssueNumber;
        public System.Windows.Forms.TextBox txt_MaterialPhaseNumber;
        private System.Windows.Forms.Button btn_Material2;
        private System.Windows.Forms.Button btn_Material3;
        private System.Windows.Forms.Button btn_Material1;
        private System.Windows.Forms.Panel pnl_Detail;
        private System.Windows.Forms.Button btn_Detail2;
        private System.Windows.Forms.Button btn_Detail3;
        private System.Windows.Forms.Button btn_Detail1;
        private System.Windows.Forms.Panel pnl_Package;
        private System.Windows.Forms.TextBox txt_SiteDate;
        private System.Windows.Forms.Button btn_BoltOrder1;
        private System.Windows.Forms.Button btnCreatePackage1;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.ComboBox cmbPackageLocation;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        public System.Windows.Forms.TextBox phaseNumber;
        public System.Windows.Forms.TextBox issueNumber;
        private System.Windows.Forms.StatusStrip statusStrip4;
        private System.Windows.Forms.ToolStripStatusLabel MaterialStatusLabel;
        private System.Windows.Forms.StatusStrip statusStrip5;
        private System.Windows.Forms.ToolStripStatusLabel DetailingStatusLabel;
        private System.Windows.Forms.StatusStrip statusStrip6;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.Button btn_HomeMaterial;
        private System.Windows.Forms.Button btn_HomeDetail;
        private System.Windows.Forms.Button btn_HomePackage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
    }
}