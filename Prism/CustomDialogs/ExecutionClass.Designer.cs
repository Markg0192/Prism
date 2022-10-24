
namespace Prism
{
    partial class ExecutionClass
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExecutionClass));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_Go = new System.Windows.Forms.Button();
            this.cmb_ExecutionClass = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Prism.Properties.Resources.Question;
            this.pictureBox1.Location = new System.Drawing.Point(3, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(49, 48);
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(58, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(225, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "What execution class would you like to apply?";
            // 
            // txt_Go
            // 
            this.txt_Go.Location = new System.Drawing.Point(181, 63);
            this.txt_Go.Name = "txt_Go";
            this.txt_Go.Size = new System.Drawing.Size(75, 23);
            this.txt_Go.TabIndex = 3;
            this.txt_Go.Text = "Go";
            this.txt_Go.UseVisualStyleBackColor = true;
            this.txt_Go.Click += new System.EventHandler(this.txt_Go_Click);
            // 
            // cmb_ExecutionClass
            // 
            this.cmb_ExecutionClass.FormattingEnabled = true;
            this.cmb_ExecutionClass.Items.AddRange(new object[] {
            "EXC1",
            "EXC2",
            "EXC3",
            "EXC4"});
            this.cmb_ExecutionClass.Location = new System.Drawing.Point(36, 65);
            this.cmb_ExecutionClass.Name = "cmb_ExecutionClass";
            this.cmb_ExecutionClass.Size = new System.Drawing.Size(121, 21);
            this.cmb_ExecutionClass.TabIndex = 8;
            // 
            // ExecutionClass
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(295, 101);
            this.Controls.Add(this.cmb_ExecutionClass);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txt_Go);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExecutionClass";
            this.Text = "Execution Class";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button txt_Go;
        private System.Windows.Forms.ComboBox cmb_ExecutionClass;
    }
}