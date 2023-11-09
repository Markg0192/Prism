namespace Prism.CustomDialogs
{
    partial class ProjectControllers
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectControllers));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txt_ProjectManagement = new System.Windows.Forms.TextBox();
            this.txt_DOManager = new System.Windows.Forms.TextBox();
            this.txt_DocumentControl = new System.Windows.Forms.TextBox();
            this.txt_Others = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btn_Apply = new System.Windows.Forms.Button();
            this.btn_Close = new System.Windows.Forms.Button();
            this.lbl_ApplyStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(371, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Add your project users here, these people will be cc\'d into all correspondance";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Project Management";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Location = new System.Drawing.Point(12, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(142, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Drawing Office Management";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 122);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Others";
            // 
            // txt_ProjectManagement
            // 
            this.txt_ProjectManagement.Location = new System.Drawing.Point(160, 43);
            this.txt_ProjectManagement.Name = "txt_ProjectManagement";
            this.txt_ProjectManagement.Size = new System.Drawing.Size(223, 20);
            this.txt_ProjectManagement.TabIndex = 2;
            // 
            // txt_DOManager
            // 
            this.txt_DOManager.Location = new System.Drawing.Point(160, 69);
            this.txt_DOManager.Name = "txt_DOManager";
            this.txt_DOManager.Size = new System.Drawing.Size(223, 20);
            this.txt_DOManager.TabIndex = 2;
            // 
            // txt_DocumentControl
            // 
            this.txt_DocumentControl.Location = new System.Drawing.Point(160, 95);
            this.txt_DocumentControl.Name = "txt_DocumentControl";
            this.txt_DocumentControl.Size = new System.Drawing.Size(223, 20);
            this.txt_DocumentControl.TabIndex = 2;
            // 
            // txt_Others
            // 
            this.txt_Others.Location = new System.Drawing.Point(160, 119);
            this.txt_Others.Name = "txt_Others";
            this.txt_Others.Size = new System.Drawing.Size(223, 20);
            this.txt_Others.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Location = new System.Drawing.Point(12, 98);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(92, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Document Control";
            // 
            // btn_Apply
            // 
            this.btn_Apply.Location = new System.Drawing.Point(228, 146);
            this.btn_Apply.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Apply.Name = "btn_Apply";
            this.btn_Apply.Size = new System.Drawing.Size(76, 23);
            this.btn_Apply.TabIndex = 3;
            this.btn_Apply.Text = "Apply";
            this.btn_Apply.UseVisualStyleBackColor = true;
            this.btn_Apply.Click += new System.EventHandler(this.btn_Apply_Click);
            // 
            // btn_Close
            // 
            this.btn_Close.Location = new System.Drawing.Point(308, 146);
            this.btn_Close.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Close.Name = "btn_Close";
            this.btn_Close.Size = new System.Drawing.Size(74, 23);
            this.btn_Close.TabIndex = 3;
            this.btn_Close.Text = "Close";
            this.btn_Close.UseVisualStyleBackColor = true;
            this.btn_Close.Click += new System.EventHandler(this.btn_Close_Click);
            // 
            // lbl_ApplyStatus
            // 
            this.lbl_ApplyStatus.AutoSize = true;
            this.lbl_ApplyStatus.BackColor = System.Drawing.Color.Transparent;
            this.lbl_ApplyStatus.Location = new System.Drawing.Point(12, 151);
            this.lbl_ApplyStatus.Name = "lbl_ApplyStatus";
            this.lbl_ApplyStatus.Size = new System.Drawing.Size(133, 13);
            this.lbl_ApplyStatus.TabIndex = 4;
            this.lbl_ApplyStatus.Text = "Press apply to store values";
            // 
            // ProjectControllers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImage = global::Prism.Properties.Resources.watereddownlogo;
            this.ClientSize = new System.Drawing.Size(392, 180);
            this.Controls.Add(this.lbl_ApplyStatus);
            this.Controls.Add(this.btn_Close);
            this.Controls.Add(this.btn_Apply);
            this.Controls.Add(this.txt_Others);
            this.Controls.Add(this.txt_DocumentControl);
            this.Controls.Add(this.txt_DOManager);
            this.Controls.Add(this.txt_ProjectManagement);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(408, 219);
            this.MinimumSize = new System.Drawing.Size(408, 219);
            this.Name = "ProjectControllers";
            this.Text = "Project Controllers";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txt_ProjectManagement;
        private System.Windows.Forms.TextBox txt_DOManager;
        private System.Windows.Forms.TextBox txt_DocumentControl;
        private System.Windows.Forms.TextBox txt_Others;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btn_Apply;
        private System.Windows.Forms.Button btn_Close;
        private System.Windows.Forms.Label lbl_ApplyStatus;
    }
}