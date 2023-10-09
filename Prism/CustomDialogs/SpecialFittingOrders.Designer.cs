namespace Prism.CustomDialogs
{
    partial class SpecialFittingOrders
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpecialFittingOrders));
            this.btn_TagSpecial = new System.Windows.Forms.Button();
            this.btn_OrderTagged = new System.Windows.Forms.Button();
            this.btn_OrderSelected = new System.Windows.Forms.Button();
            this.btn_RemoveSpecialTag = new System.Windows.Forms.Button();
            this.btn_SpecialClose = new System.Windows.Forms.Button();
            this.btn_ShowTagged = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btn_CreateTaggedDrawings = new System.Windows.Forms.Button();
            this.btn_CreateSelectedDrawings = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_TagSpecial
            // 
            this.btn_TagSpecial.BackColor = System.Drawing.Color.Transparent;
            this.btn_TagSpecial.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_TagSpecial.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_TagSpecial.Location = new System.Drawing.Point(3, 3);
            this.btn_TagSpecial.Name = "btn_TagSpecial";
            this.btn_TagSpecial.Size = new System.Drawing.Size(100, 50);
            this.btn_TagSpecial.TabIndex = 0;
            this.btn_TagSpecial.Text = "Tag Special Fittings";
            this.toolTip1.SetToolTip(this.btn_TagSpecial, "Adds a UDA to the selected");
            this.btn_TagSpecial.UseVisualStyleBackColor = false;
            this.btn_TagSpecial.Click += new System.EventHandler(this.btn_TagSpecial_Click);
            // 
            // btn_OrderTagged
            // 
            this.btn_OrderTagged.BackColor = System.Drawing.Color.Transparent;
            this.btn_OrderTagged.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_OrderTagged.Location = new System.Drawing.Point(3, 145);
            this.btn_OrderTagged.Name = "btn_OrderTagged";
            this.btn_OrderTagged.Size = new System.Drawing.Size(100, 50);
            this.btn_OrderTagged.TabIndex = 1;
            this.btn_OrderTagged.Text = "Order Tagged";
            this.toolTip1.SetToolTip(this.btn_OrderTagged, "Orders selected items with the special UDA");
            this.btn_OrderTagged.UseVisualStyleBackColor = false;
            this.btn_OrderTagged.Click += new System.EventHandler(this.btn_OrderTagged_Click);
            // 
            // btn_OrderSelected
            // 
            this.btn_OrderSelected.BackColor = System.Drawing.Color.Transparent;
            this.btn_OrderSelected.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_OrderSelected.Location = new System.Drawing.Point(109, 145);
            this.btn_OrderSelected.Name = "btn_OrderSelected";
            this.btn_OrderSelected.Size = new System.Drawing.Size(100, 50);
            this.btn_OrderSelected.TabIndex = 1;
            this.btn_OrderSelected.Text = "Order Selected";
            this.toolTip1.SetToolTip(this.btn_OrderSelected, "Orders everything selected");
            this.btn_OrderSelected.UseVisualStyleBackColor = false;
            this.btn_OrderSelected.Click += new System.EventHandler(this.btn_OrderSelected_Click);
            // 
            // btn_RemoveSpecialTag
            // 
            this.btn_RemoveSpecialTag.BackColor = System.Drawing.Color.Transparent;
            this.btn_RemoveSpecialTag.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_RemoveSpecialTag.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_RemoveSpecialTag.Location = new System.Drawing.Point(109, 3);
            this.btn_RemoveSpecialTag.Name = "btn_RemoveSpecialTag";
            this.btn_RemoveSpecialTag.Size = new System.Drawing.Size(100, 50);
            this.btn_RemoveSpecialTag.TabIndex = 0;
            this.btn_RemoveSpecialTag.Text = "Remove Special Tag";
            this.toolTip1.SetToolTip(this.btn_RemoveSpecialTag, "Removes the UDA from the selected");
            this.btn_RemoveSpecialTag.UseVisualStyleBackColor = false;
            this.btn_RemoveSpecialTag.Click += new System.EventHandler(this.btn_RemoveSpecialTag_Click);
            // 
            // btn_SpecialClose
            // 
            this.btn_SpecialClose.BackColor = System.Drawing.Color.Transparent;
            this.btn_SpecialClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_SpecialClose.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_SpecialClose.Location = new System.Drawing.Point(3, 202);
            this.btn_SpecialClose.Name = "btn_SpecialClose";
            this.btn_SpecialClose.Size = new System.Drawing.Size(206, 23);
            this.btn_SpecialClose.TabIndex = 2;
            this.btn_SpecialClose.Text = "Close";
            this.btn_SpecialClose.UseVisualStyleBackColor = false;
            this.btn_SpecialClose.Click += new System.EventHandler(this.btn_SpecialClose_Click);
            // 
            // btn_ShowTagged
            // 
            this.btn_ShowTagged.BackColor = System.Drawing.Color.Transparent;
            this.btn_ShowTagged.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_ShowTagged.Location = new System.Drawing.Point(3, 59);
            this.btn_ShowTagged.Name = "btn_ShowTagged";
            this.btn_ShowTagged.Size = new System.Drawing.Size(206, 23);
            this.btn_ShowTagged.TabIndex = 3;
            this.btn_ShowTagged.Text = "Show Tagged in Selection";
            this.toolTip1.SetToolTip(this.btn_ShowTagged, "Colours selected items with the special UDA");
            this.btn_ShowTagged.UseVisualStyleBackColor = false;
            this.btn_ShowTagged.Click += new System.EventHandler(this.btn_ShowTagged_Click);
            // 
            // btn_CreateTaggedDrawings
            // 
            this.btn_CreateTaggedDrawings.BackColor = System.Drawing.Color.Transparent;
            this.btn_CreateTaggedDrawings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_CreateTaggedDrawings.Location = new System.Drawing.Point(3, 89);
            this.btn_CreateTaggedDrawings.Name = "btn_CreateTaggedDrawings";
            this.btn_CreateTaggedDrawings.Size = new System.Drawing.Size(100, 50);
            this.btn_CreateTaggedDrawings.TabIndex = 1;
            this.btn_CreateTaggedDrawings.Text = "Create Drawings for Tagged";
            this.toolTip1.SetToolTip(this.btn_CreateTaggedDrawings, "Creates drawings for selected items with the special tag");
            this.btn_CreateTaggedDrawings.UseVisualStyleBackColor = false;
            this.btn_CreateTaggedDrawings.Click += new System.EventHandler(this.btn_CreateTaggedDrawings_Click);
            // 
            // btn_CreateSelectedDrawings
            // 
            this.btn_CreateSelectedDrawings.BackColor = System.Drawing.Color.Transparent;
            this.btn_CreateSelectedDrawings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_CreateSelectedDrawings.Location = new System.Drawing.Point(109, 89);
            this.btn_CreateSelectedDrawings.Name = "btn_CreateSelectedDrawings";
            this.btn_CreateSelectedDrawings.Size = new System.Drawing.Size(100, 50);
            this.btn_CreateSelectedDrawings.TabIndex = 1;
            this.btn_CreateSelectedDrawings.Text = "Create Drawings for Selected";
            this.toolTip1.SetToolTip(this.btn_CreateSelectedDrawings, "Creates drawings for everything selected");
            this.btn_CreateSelectedDrawings.UseVisualStyleBackColor = false;
            this.btn_CreateSelectedDrawings.Click += new System.EventHandler(this.btn_CreateSelectedDrawings_Click);
            // 
            // SpecialFittingOrders
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.BackgroundImage = global::Prism.Properties.Resources.watereddownlogo;
            this.ClientSize = new System.Drawing.Size(213, 233);
            this.Controls.Add(this.btn_ShowTagged);
            this.Controls.Add(this.btn_SpecialClose);
            this.Controls.Add(this.btn_CreateSelectedDrawings);
            this.Controls.Add(this.btn_OrderSelected);
            this.Controls.Add(this.btn_CreateTaggedDrawings);
            this.Controls.Add(this.btn_OrderTagged);
            this.Controls.Add(this.btn_RemoveSpecialTag);
            this.Controls.Add(this.btn_TagSpecial);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SpecialFittingOrders";
            this.Text = "Special Fitting Handler";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_TagSpecial;
        private System.Windows.Forms.Button btn_OrderTagged;
        private System.Windows.Forms.Button btn_OrderSelected;
        private System.Windows.Forms.Button btn_RemoveSpecialTag;
        private System.Windows.Forms.Button btn_SpecialClose;
        private System.Windows.Forms.Button btn_ShowTagged;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btn_CreateTaggedDrawings;
        private System.Windows.Forms.Button btn_CreateSelectedDrawings;
    }
}