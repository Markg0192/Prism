namespace Prism.CustomDialogs
{
    partial class BoltOrderType
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BoltOrderType));
            this.btn_FromBolts = new System.Windows.Forms.Button();
            this.btn_FromAss = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_FromBolts
            // 
            this.btn_FromBolts.Location = new System.Drawing.Point(61, 60);
            this.btn_FromBolts.Name = "btn_FromBolts";
            this.btn_FromBolts.Size = new System.Drawing.Size(90, 41);
            this.btn_FromBolts.TabIndex = 0;
            this.btn_FromBolts.Text = "From Selected Bolts";
            this.btn_FromBolts.UseVisualStyleBackColor = true;
            this.btn_FromBolts.Click += new System.EventHandler(this.btn_FromBolts_Click);
            // 
            // btn_FromAss
            // 
            this.btn_FromAss.Location = new System.Drawing.Point(169, 60);
            this.btn_FromAss.Name = "btn_FromAss";
            this.btn_FromAss.Size = new System.Drawing.Size(87, 41);
            this.btn_FromAss.TabIndex = 0;
            this.btn_FromAss.Text = "From Selected Assemblies";
            this.btn_FromAss.UseVisualStyleBackColor = true;
            this.btn_FromAss.Click += new System.EventHandler(this.btn_FromAss_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(58, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(198, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "How would you like to order these bolts?";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Prism.Properties.Resources.Question;
            this.pictureBox1.Location = new System.Drawing.Point(3, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(49, 48);
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // BoltOrderType
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(285, 113);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_FromAss);
            this.Controls.Add(this.btn_FromBolts);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BoltOrderType";
            this.Text = "Bolt Order Type";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_FromBolts;
        private System.Windows.Forms.Button btn_FromAss;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}