namespace Prism.CustomDialogs
{
    partial class Division
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Division));
            this.btn_DivCI = new System.Windows.Forms.Button();
            this.btn_DivNI = new System.Windows.Forms.Button();
            this.btn_DivPP = new System.Windows.Forms.Button();
            this.btn_DivOther = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btn_DivCI
            // 
            this.btn_DivCI.BackColor = System.Drawing.Color.Transparent;
            this.btn_DivCI.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_DivCI.Location = new System.Drawing.Point(12, 52);
            this.btn_DivCI.Name = "btn_DivCI";
            this.btn_DivCI.Size = new System.Drawing.Size(102, 55);
            this.btn_DivCI.TabIndex = 0;
            this.btn_DivCI.Text = "Commercial and Industrial";
            this.btn_DivCI.UseVisualStyleBackColor = false;
            this.btn_DivCI.Click += new System.EventHandler(this.btn_DivCI_Click);
            // 
            // btn_DivNI
            // 
            this.btn_DivNI.BackColor = System.Drawing.Color.Transparent;
            this.btn_DivNI.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_DivNI.Location = new System.Drawing.Point(120, 52);
            this.btn_DivNI.Name = "btn_DivNI";
            this.btn_DivNI.Size = new System.Drawing.Size(102, 55);
            this.btn_DivNI.TabIndex = 0;
            this.btn_DivNI.Text = "Nuclear and Infrastructure";
            this.btn_DivNI.UseVisualStyleBackColor = false;
            this.btn_DivNI.Click += new System.EventHandler(this.btn_DivNI_Click);
            // 
            // btn_DivPP
            // 
            this.btn_DivPP.BackColor = System.Drawing.Color.Transparent;
            this.btn_DivPP.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_DivPP.Location = new System.Drawing.Point(12, 113);
            this.btn_DivPP.Name = "btn_DivPP";
            this.btn_DivPP.Size = new System.Drawing.Size(102, 55);
            this.btn_DivPP.TabIndex = 0;
            this.btn_DivPP.Text = "Parts and Processing";
            this.btn_DivPP.UseVisualStyleBackColor = false;
            this.btn_DivPP.Click += new System.EventHandler(this.btn_DivPP_Click);
            // 
            // btn_DivOther
            // 
            this.btn_DivOther.BackColor = System.Drawing.Color.Transparent;
            this.btn_DivOther.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_DivOther.Location = new System.Drawing.Point(120, 113);
            this.btn_DivOther.Name = "btn_DivOther";
            this.btn_DivOther.Size = new System.Drawing.Size(102, 55);
            this.btn_DivOther.TabIndex = 0;
            this.btn_DivOther.Text = "Other";
            this.btn_DivOther.UseVisualStyleBackColor = false;
            this.btn_DivOther.Click += new System.EventHandler(this.btn_DivOther_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.BackColor = System.Drawing.Color.Transparent;
            this.btn_Cancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_Cancel.Location = new System.Drawing.Point(12, 174);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(210, 23);
            this.btn_Cancel.TabIndex = 1;
            this.btn_Cancel.Text = "Cancel Seversafe Order";
            this.btn_Cancel.UseVisualStyleBackColor = false;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(15, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(207, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Which Severfield division is your order for?";
            // 
            // Division
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::Prism.Properties.Resources.watereddownlogo;
            this.ClientSize = new System.Drawing.Size(234, 208);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_DivOther);
            this.Controls.Add(this.btn_DivPP);
            this.Controls.Add(this.btn_DivNI);
            this.Controls.Add(this.btn_DivCI);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Division";
            this.Text = "Division";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_DivCI;
        private System.Windows.Forms.Button btn_DivNI;
        private System.Windows.Forms.Button btn_DivPP;
        private System.Windows.Forms.Button btn_DivOther;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Label label1;
    }
}