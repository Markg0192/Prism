namespace Prism.CustomDialogs
{
    partial class AdvancedSettings
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdvancedSettings));
			this.txt_PrelimPrefix = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btn_Apply = new System.Windows.Forms.Button();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.label2 = new System.Windows.Forms.Label();
			this.cmb_FaPackType = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.txt_DirectoryFab = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.txt_DirectoryMaterial = new System.Windows.Forms.TextBox();
			this.txt_DirectoryCarcass = new System.Windows.Forms.TextBox();
			this.txt_DirectoryBolts = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.txt_DirectorySeversafe = new System.Windows.Forms.TextBox();
			this.btn_Close = new System.Windows.Forms.Button();
			this.txt_DirectoryVariation = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.txt_FabsecGreen = new System.Windows.Forms.TextBox();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// txt_PrelimPrefix
			// 
			this.txt_PrelimPrefix.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txt_PrelimPrefix.Location = new System.Drawing.Point(142, 27);
			this.txt_PrelimPrefix.Name = "txt_PrelimPrefix";
			this.txt_PrelimPrefix.Size = new System.Drawing.Size(100, 20);
			this.txt_PrelimPrefix.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Location = new System.Drawing.Point(12, 30);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(91, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Prelim Mark Prefix";
			// 
			// btn_Apply
			// 
			this.btn_Apply.Location = new System.Drawing.Point(13, 296);
			this.btn_Apply.Name = "btn_Apply";
			this.btn_Apply.Size = new System.Drawing.Size(226, 30);
			this.btn_Apply.TabIndex = 2;
			this.btn_Apply.Text = "Apply Settings";
			this.btn_Apply.UseVisualStyleBackColor = true;
			this.btn_Apply.Click += new System.EventHandler(this.btn_Apply_Click);
			// 
			// statusStrip1
			// 
			this.statusStrip1.BackColor = System.Drawing.Color.Transparent;
			this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
			this.statusStrip1.Location = new System.Drawing.Point(0, 330);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(430, 22);
			this.statusStrip1.TabIndex = 3;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// StatusLabel
			// 
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = new System.Drawing.Size(118, 17);
			this.StatusLabel.Text = "toolStripStatusLabel1";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Enabled = false;
			this.label2.Location = new System.Drawing.Point(12, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(98, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Fab Package Type";
			// 
			// cmb_FaPackType
			// 
			this.cmb_FaPackType.Enabled = false;
			this.cmb_FaPackType.FormattingEnabled = true;
			this.cmb_FaPackType.Items.AddRange(new object[] {
            "By Phase",
            "By Lot"});
			this.cmb_FaPackType.Location = new System.Drawing.Point(142, 53);
			this.cmb_FaPackType.Name = "cmb_FaPackType";
			this.cmb_FaPackType.Size = new System.Drawing.Size(100, 21);
			this.cmb_FaPackType.TabIndex = 5;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.BackColor = System.Drawing.Color.Transparent;
			this.label3.Location = new System.Drawing.Point(13, 247);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(102, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Fabication Package";
			// 
			// txt_DirectoryFab
			// 
			this.txt_DirectoryFab.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txt_DirectoryFab.Location = new System.Drawing.Point(142, 244);
			this.txt_DirectoryFab.Name = "txt_DirectoryFab";
			this.txt_DirectoryFab.Size = new System.Drawing.Size(275, 20);
			this.txt_DirectoryFab.TabIndex = 0;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.BackColor = System.Drawing.Color.Transparent;
			this.label4.Location = new System.Drawing.Point(13, 120);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(394, 13);
			this.label4.TabIndex = 4;
			this.label4.Text = " --  --  --  --  --  --  --  --  --  --  --  --  Package Directories --  --  --  " +
    "--  --  --  --  --  --  --  --  -- ";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.BackColor = System.Drawing.Color.Transparent;
			this.label5.Location = new System.Drawing.Point(13, 143);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(78, 13);
			this.label5.TabIndex = 4;
			this.label5.Text = "Material Orders";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.BackColor = System.Drawing.Color.Transparent;
			this.label6.Location = new System.Drawing.Point(12, 169);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(94, 13);
			this.label6.TabIndex = 4;
			this.label6.Text = "Fabsec Carcasses";
			// 
			// txt_DirectoryMaterial
			// 
			this.txt_DirectoryMaterial.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txt_DirectoryMaterial.Location = new System.Drawing.Point(142, 140);
			this.txt_DirectoryMaterial.Name = "txt_DirectoryMaterial";
			this.txt_DirectoryMaterial.Size = new System.Drawing.Size(275, 20);
			this.txt_DirectoryMaterial.TabIndex = 0;
			// 
			// txt_DirectoryCarcass
			// 
			this.txt_DirectoryCarcass.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txt_DirectoryCarcass.Location = new System.Drawing.Point(142, 166);
			this.txt_DirectoryCarcass.Name = "txt_DirectoryCarcass";
			this.txt_DirectoryCarcass.Size = new System.Drawing.Size(275, 20);
			this.txt_DirectoryCarcass.TabIndex = 0;
			// 
			// txt_DirectoryBolts
			// 
			this.txt_DirectoryBolts.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txt_DirectoryBolts.Location = new System.Drawing.Point(142, 192);
			this.txt_DirectoryBolts.Name = "txt_DirectoryBolts";
			this.txt_DirectoryBolts.Size = new System.Drawing.Size(275, 20);
			this.txt_DirectoryBolts.TabIndex = 0;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.BackColor = System.Drawing.Color.Transparent;
			this.label7.Location = new System.Drawing.Point(13, 195);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(59, 13);
			this.label7.TabIndex = 4;
			this.label7.Text = "Bolt Orders";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.BackColor = System.Drawing.Color.Transparent;
			this.label8.Location = new System.Drawing.Point(12, 221);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(89, 13);
			this.label8.TabIndex = 4;
			this.label8.Text = "Seversafe Orders";
			// 
			// txt_DirectorySeversafe
			// 
			this.txt_DirectorySeversafe.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txt_DirectorySeversafe.Location = new System.Drawing.Point(142, 218);
			this.txt_DirectorySeversafe.Name = "txt_DirectorySeversafe";
			this.txt_DirectorySeversafe.Size = new System.Drawing.Size(275, 20);
			this.txt_DirectorySeversafe.TabIndex = 0;
			// 
			// btn_Close
			// 
			this.btn_Close.Location = new System.Drawing.Point(245, 296);
			this.btn_Close.Name = "btn_Close";
			this.btn_Close.Size = new System.Drawing.Size(173, 30);
			this.btn_Close.TabIndex = 2;
			this.btn_Close.Text = "Close";
			this.btn_Close.UseVisualStyleBackColor = true;
			this.btn_Close.Click += new System.EventHandler(this.btn_Close_Click);
			// 
			// txt_DirectoryVariation
			// 
			this.txt_DirectoryVariation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txt_DirectoryVariation.Location = new System.Drawing.Point(142, 270);
			this.txt_DirectoryVariation.Name = "txt_DirectoryVariation";
			this.txt_DirectoryVariation.Size = new System.Drawing.Size(275, 20);
			this.txt_DirectoryVariation.TabIndex = 0;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.BackColor = System.Drawing.Color.Transparent;
			this.label9.Location = new System.Drawing.Point(13, 273);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(53, 13);
			this.label9.TabIndex = 4;
			this.label9.Text = "Variations";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.BackColor = System.Drawing.Color.Transparent;
			this.label10.Location = new System.Drawing.Point(12, 82);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(74, 13);
			this.label10.TabIndex = 7;
			this.label10.Text = "Fabsec Green";
			// 
			// txt_FabsecGreen
			// 
			this.txt_FabsecGreen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txt_FabsecGreen.Location = new System.Drawing.Point(142, 80);
			this.txt_FabsecGreen.Name = "txt_FabsecGreen";
			this.txt_FabsecGreen.Size = new System.Drawing.Size(100, 20);
			this.txt_FabsecGreen.TabIndex = 6;
			this.txt_FabsecGreen.Text = "100";
			// 
			// AdvancedSettings
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = global::Prism.Properties.Resources.watereddownlogo;
			this.ClientSize = new System.Drawing.Size(430, 352);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.txt_FabsecGreen);
			this.Controls.Add(this.cmb_FaPackType);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.btn_Close);
			this.Controls.Add(this.btn_Apply);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txt_DirectoryCarcass);
			this.Controls.Add(this.txt_DirectoryMaterial);
			this.Controls.Add(this.txt_DirectoryBolts);
			this.Controls.Add(this.txt_DirectorySeversafe);
			this.Controls.Add(this.txt_DirectoryVariation);
			this.Controls.Add(this.txt_DirectoryFab);
			this.Controls.Add(this.txt_PrelimPrefix);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "AdvancedSettings";
			this.Text = "Advanced Settings";
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_PrelimPrefix;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_Apply;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmb_FaPackType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txt_DirectoryFab;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txt_DirectoryMaterial;
        private System.Windows.Forms.TextBox txt_DirectoryCarcass;
        private System.Windows.Forms.TextBox txt_DirectoryBolts;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txt_DirectorySeversafe;
        private System.Windows.Forms.Button btn_Close;
        private System.Windows.Forms.TextBox txt_DirectoryVariation;
        private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.TextBox txt_FabsecGreen;
	}
}