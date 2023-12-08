namespace Prism.CustomDialogs
{
    partial class FabsecCarcassOrder
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FabsecCarcassOrder));
            this.btn_CreateCarcass = new System.Windows.Forms.Button();
            this.btn_OrderCarcass = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_CreateCarcass
            // 
            this.btn_CreateCarcass.Location = new System.Drawing.Point(12, 12);
            this.btn_CreateCarcass.Name = "btn_CreateCarcass";
            this.btn_CreateCarcass.Size = new System.Drawing.Size(117, 43);
            this.btn_CreateCarcass.TabIndex = 0;
            this.btn_CreateCarcass.Text = "Create Carcass Drawings";
            this.btn_CreateCarcass.UseVisualStyleBackColor = true;
            this.btn_CreateCarcass.Click += new System.EventHandler(this.btn_CreateCarcass_Click);
            // 
            // btn_OrderCarcass
            // 
            this.btn_OrderCarcass.Location = new System.Drawing.Point(167, 12);
            this.btn_OrderCarcass.Name = "btn_OrderCarcass";
            this.btn_OrderCarcass.Size = new System.Drawing.Size(117, 43);
            this.btn_OrderCarcass.TabIndex = 0;
            this.btn_OrderCarcass.Text = "Order Carcasses";
            this.btn_OrderCarcass.UseVisualStyleBackColor = true;
            this.btn_OrderCarcass.Click += new System.EventHandler(this.btn_OrderCarcass_Click);
            // 
            // FabsecCarcassOrder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::Prism.Properties.Resources.watereddownlogo;
            this.ClientSize = new System.Drawing.Size(297, 70);
            this.Controls.Add(this.btn_OrderCarcass);
            this.Controls.Add(this.btn_CreateCarcass);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(313, 109);
            this.MinimumSize = new System.Drawing.Size(313, 109);
            this.Name = "FabsecCarcassOrder";
            this.Text = "Process Fabsec Carcasses";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_CreateCarcass;
        private System.Windows.Forms.Button btn_OrderCarcass;
    }
}