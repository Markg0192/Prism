namespace Prism.CustomDialogs
{
    partial class ChangeMessage
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
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.btn_StopAndReport = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.btn_ConfirmAndContinue = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btn_Cancel.Location = new System.Drawing.Point(0, 424);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(584, 23);
            this.btn_Cancel.TabIndex = 1;
            this.btn_Cancel.Text = "Cancel";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_StopAndReport
            // 
            this.btn_StopAndReport.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btn_StopAndReport.Location = new System.Drawing.Point(0, 401);
            this.btn_StopAndReport.Name = "btn_StopAndReport";
            this.btn_StopAndReport.Size = new System.Drawing.Size(584, 23);
            this.btn_StopAndReport.TabIndex = 2;
            this.btn_StopAndReport.Text = "Stop and Produce Report";
            this.btn_StopAndReport.UseVisualStyleBackColor = true;
            this.btn_StopAndReport.Click += new System.EventHandler(this.btn_StopAndReport_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(0, 0);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(584, 346);
            this.richTextBox1.TabIndex = 3;
            this.richTextBox1.Text = "";
            // 
            // btn_ConfirmAndContinue
            // 
            this.btn_ConfirmAndContinue.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btn_ConfirmAndContinue.Location = new System.Drawing.Point(0, 378);
            this.btn_ConfirmAndContinue.Name = "btn_ConfirmAndContinue";
            this.btn_ConfirmAndContinue.Size = new System.Drawing.Size(584, 23);
            this.btn_ConfirmAndContinue.TabIndex = 4;
            this.btn_ConfirmAndContinue.Text = "Confirm and Continue";
            this.btn_ConfirmAndContinue.UseVisualStyleBackColor = true;
            this.btn_ConfirmAndContinue.Click += new System.EventHandler(this.btn_ConfirmAndContinue_Click_1);
            // 
            // ChangeMessage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(584, 447);
            this.Controls.Add(this.btn_ConfirmAndContinue);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.btn_StopAndReport);
            this.Controls.Add(this.btn_Cancel);
            this.Name = "ChangeMessage";
            this.Text = "ChangeMessage";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_StopAndReport;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btn_ConfirmAndContinue;
    }
}