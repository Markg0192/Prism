
namespace Prism
{
    partial class FactoryLocation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FactoryLocation));
            this.txt_SUK = new System.Windows.Forms.Button();
            this.txt_SNI = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_Unknown = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // txt_SUK
            // 
            this.txt_SUK.Location = new System.Drawing.Point(17, 68);
            this.txt_SUK.Name = "txt_SUK";
            this.txt_SUK.Size = new System.Drawing.Size(75, 23);
            this.txt_SUK.TabIndex = 0;
            this.txt_SUK.Text = "SUK";
            this.txt_SUK.UseVisualStyleBackColor = true;
            this.txt_SUK.Click += new System.EventHandler(this.txt_SUK_Click);
            // 
            // txt_SNI
            // 
            this.txt_SNI.Location = new System.Drawing.Point(98, 68);
            this.txt_SNI.Name = "txt_SNI";
            this.txt_SNI.Size = new System.Drawing.Size(75, 23);
            this.txt_SNI.TabIndex = 0;
            this.txt_SNI.Text = "SNI";
            this.txt_SNI.UseVisualStyleBackColor = true;
            this.txt_SNI.Click += new System.EventHandler(this.txt_SNI_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(57, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(198, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Where is the fabricating factory located?";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // txt_Unknown
            // 
            this.txt_Unknown.Location = new System.Drawing.Point(179, 68);
            this.txt_Unknown.Name = "txt_Unknown";
            this.txt_Unknown.Size = new System.Drawing.Size(75, 23);
            this.txt_Unknown.TabIndex = 0;
            this.txt_Unknown.Text = "I don\'t know";
            this.txt_Unknown.UseVisualStyleBackColor = true;
            this.txt_Unknown.Click += new System.EventHandler(this.txt_Unknown_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Prism.Properties.Resources.Question;
            this.pictureBox1.Location = new System.Drawing.Point(2, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(49, 48);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // WhereAreYou
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.ClientSize = new System.Drawing.Size(270, 105);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txt_Unknown);
            this.Controls.Add(this.txt_SNI);
            this.Controls.Add(this.txt_SUK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WhereAreYou";
            this.Text = "Where is it going?";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button txt_SUK;
        private System.Windows.Forms.Button txt_SNI;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button txt_Unknown;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}