
namespace Prism
{
    partial class IgnoreWarning
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IgnoreWarning));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_Stop = new System.Windows.Forms.Button();
            this.txt_Ignore = new System.Windows.Forms.Button();
            this.txt_AutoFix = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Prism.Properties.Resources.Question;
            this.pictureBox1.Location = new System.Drawing.Point(7, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(49, 48);
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(62, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(201, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "What would you like to do with this error?";
            // 
            // txt_Stop
            // 
            this.txt_Stop.Location = new System.Drawing.Point(178, 69);
            this.txt_Stop.Name = "txt_Stop";
            this.txt_Stop.Size = new System.Drawing.Size(75, 23);
            this.txt_Stop.TabIndex = 3;
            this.txt_Stop.Text = "Stop";
            this.txt_Stop.UseVisualStyleBackColor = true;
            this.txt_Stop.Click += new System.EventHandler(this.txt_Stop_Click);
            // 
            // txt_Ignore
            // 
            this.txt_Ignore.Location = new System.Drawing.Point(97, 69);
            this.txt_Ignore.Name = "txt_Ignore";
            this.txt_Ignore.Size = new System.Drawing.Size(75, 23);
            this.txt_Ignore.TabIndex = 4;
            this.txt_Ignore.Text = "Ignore";
            this.txt_Ignore.UseVisualStyleBackColor = true;
            this.txt_Ignore.Click += new System.EventHandler(this.txt_Ignore_Click);
            // 
            // txt_AutoFix
            // 
            this.txt_AutoFix.Location = new System.Drawing.Point(16, 69);
            this.txt_AutoFix.Name = "txt_AutoFix";
            this.txt_AutoFix.Size = new System.Drawing.Size(75, 23);
            this.txt_AutoFix.TabIndex = 5;
            this.txt_AutoFix.Text = "Auto-Fix";
            this.txt_AutoFix.UseVisualStyleBackColor = true;
            this.txt_AutoFix.Click += new System.EventHandler(this.txt_AutoFix_Click);
            // 
            // IgnoreWarning
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(274, 104);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txt_Stop);
            this.Controls.Add(this.txt_Ignore);
            this.Controls.Add(this.txt_AutoFix);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "IgnoreWarning";
            this.Text = "Warning Action";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button txt_Stop;
        private System.Windows.Forms.Button txt_Ignore;
        private System.Windows.Forms.Button txt_AutoFix;
    }
}