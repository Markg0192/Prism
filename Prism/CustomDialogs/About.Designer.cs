namespace Prism.CustomDialogs
{
    partial class About
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btn_Close = new System.Windows.Forms.Button();
            this.lbl_AboutVersionNo = new System.Windows.Forms.Label();
            this.btn_Contact = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Prism.Properties.Resources.Severfield_blue_RGB_PNG;
            this.pictureBox1.Location = new System.Drawing.Point(60, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(311, 97);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(47, 193);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(338, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Prism is an app developed by the Severfield Central Engineering Team";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(35, 216);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(365, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "It\'s goal is to aid Tekla detailers with model checks and all package creation";
            // 
            // btn_Close
            // 
            this.btn_Close.Location = new System.Drawing.Point(325, 258);
            this.btn_Close.Name = "btn_Close";
            this.btn_Close.Size = new System.Drawing.Size(93, 36);
            this.btn_Close.TabIndex = 3;
            this.btn_Close.Text = "Close";
            this.btn_Close.UseVisualStyleBackColor = true;
            this.btn_Close.Click += new System.EventHandler(this.btn_Close_Click);
            // 
            // lbl_AboutVersionNo
            // 
            this.lbl_AboutVersionNo.AutoSize = true;
            this.lbl_AboutVersionNo.Location = new System.Drawing.Point(185, 149);
            this.lbl_AboutVersionNo.Name = "lbl_AboutVersionNo";
            this.lbl_AboutVersionNo.Size = new System.Drawing.Size(66, 13);
            this.lbl_AboutVersionNo.TabIndex = 1;
            this.lbl_AboutVersionNo.Text = "Version: ???";
            // 
            // btn_Contact
            // 
            this.btn_Contact.Location = new System.Drawing.Point(226, 258);
            this.btn_Contact.Name = "btn_Contact";
            this.btn_Contact.Size = new System.Drawing.Size(93, 36);
            this.btn_Contact.TabIndex = 3;
            this.btn_Contact.Text = "Contact";
            this.btn_Contact.UseVisualStyleBackColor = true;
            this.btn_Contact.Click += new System.EventHandler(this.btn_Contact_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabel1.Location = new System.Drawing.Point(150, 112);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(143, 13);
            this.linkLabel1.TabIndex = 4;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "https://www.severfield.com/";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(426, 307);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.btn_Contact);
            this.Controls.Add(this.btn_Close);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lbl_AboutVersionNo);
            this.Controls.Add(this.pictureBox1);
            this.MaximumSize = new System.Drawing.Size(442, 346);
            this.MinimumSize = new System.Drawing.Size(442, 346);
            this.Name = "About";
            this.Text = "About";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btn_Close;
        private System.Windows.Forms.Label lbl_AboutVersionNo;
        private System.Windows.Forms.Button btn_Contact;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}