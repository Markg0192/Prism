namespace Prism.CustomDialogs
{
    partial class ResetPrelim
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResetPrelim));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lbl_NextPrelim = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txt_PrelimReset = new System.Windows.Forms.TextBox();
            this.btn_PrelimReset = new System.Windows.Forms.Button();
            this.btn_Refresh = new System.Windows.Forms.Button();
            this.btn_Close = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(12, 92);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Current start point:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(12, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(314, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Click the button below to set the prelim number system start point,";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Location = new System.Drawing.Point(12, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(331, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "when applying new prelim numbers Prism will now start from this here.";
            // 
            // lbl_NextPrelim
            // 
            this.lbl_NextPrelim.AutoSize = true;
            this.lbl_NextPrelim.BackColor = System.Drawing.Color.Transparent;
            this.lbl_NextPrelim.Location = new System.Drawing.Point(104, 92);
            this.lbl_NextPrelim.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_NextPrelim.Name = "lbl_NextPrelim";
            this.lbl_NextPrelim.Size = new System.Drawing.Size(35, 13);
            this.lbl_NextPrelim.TabIndex = 47;
            this.lbl_NextPrelim.Text = "label5";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Location = new System.Drawing.Point(12, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "WARNING:";
            // 
            // txt_PrelimReset
            // 
            this.txt_PrelimReset.Location = new System.Drawing.Point(165, 126);
            this.txt_PrelimReset.Name = "txt_PrelimReset";
            this.txt_PrelimReset.Size = new System.Drawing.Size(100, 20);
            this.txt_PrelimReset.TabIndex = 48;
            this.txt_PrelimReset.Text = "0";
            // 
            // btn_PrelimReset
            // 
            this.btn_PrelimReset.Location = new System.Drawing.Point(15, 124);
            this.btn_PrelimReset.Name = "btn_PrelimReset";
            this.btn_PrelimReset.Size = new System.Drawing.Size(144, 23);
            this.btn_PrelimReset.TabIndex = 49;
            this.btn_PrelimReset.Text = "Set New Prelim Start Point";
            this.btn_PrelimReset.UseVisualStyleBackColor = true;
            this.btn_PrelimReset.Click += new System.EventHandler(this.btn_PrelimReset_Click);
            // 
            // btn_Refresh
            // 
            this.btn_Refresh.Location = new System.Drawing.Point(165, 87);
            this.btn_Refresh.Name = "btn_Refresh";
            this.btn_Refresh.Size = new System.Drawing.Size(62, 23);
            this.btn_Refresh.TabIndex = 50;
            this.btn_Refresh.Text = "Refresh";
            this.btn_Refresh.UseVisualStyleBackColor = true;
            this.btn_Refresh.Click += new System.EventHandler(this.btn_Refresh_Click);
            // 
            // btn_Close
            // 
            this.btn_Close.Location = new System.Drawing.Point(349, 152);
            this.btn_Close.Name = "btn_Close";
            this.btn_Close.Size = new System.Drawing.Size(62, 23);
            this.btn_Close.TabIndex = 50;
            this.btn_Close.Text = "Close";
            this.btn_Close.UseVisualStyleBackColor = true;
            this.btn_Close.Click += new System.EventHandler(this.btn_Close_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Location = new System.Drawing.Point(12, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(402, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "This manual operation may cause prelim number overlapping and cannot be undone";
            // 
            // ResetPrelim
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::Prism.Properties.Resources.watereddownlogo;
            this.ClientSize = new System.Drawing.Size(423, 187);
            this.Controls.Add(this.btn_Close);
            this.Controls.Add(this.btn_Refresh);
            this.Controls.Add(this.btn_PrelimReset);
            this.Controls.Add(this.txt_PrelimReset);
            this.Controls.Add(this.lbl_NextPrelim);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ResetPrelim";
            this.Text = "Reset Prelim Start Number";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbl_NextPrelim;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txt_PrelimReset;
        private System.Windows.Forms.Button btn_PrelimReset;
        private System.Windows.Forms.Button btn_Refresh;
        private System.Windows.Forms.Button btn_Close;
        private System.Windows.Forms.Label label5;
    }
}