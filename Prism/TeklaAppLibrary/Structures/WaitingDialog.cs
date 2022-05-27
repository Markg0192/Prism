using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Tekla.Structures
{
	public class WaitingDialog : Form
	{
		private readonly Predicate<WaitingDialog> completed;

		private IContainer components = null;

		private ProgressBar progressBar1;

		private Button cancelButton;

		private Timer completionTimer;

		public WaitingDialog(Predicate<WaitingDialog> completed)
		{
			this.completed = completed;
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!base.DesignMode && TeklaStructures.Connection.IsActive)
			{
				TeklaStructures.Environment.Localization.Localize((Control)this);
			}
			else
			{
				cancelButton.Text = "Cancel";
			}
		}

		private void OnTick(object sender, EventArgs e)
		{
			if (completed(this))
			{
				base.DialogResult = DialogResult.OK;
				Close();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			progressBar1 = new System.Windows.Forms.ProgressBar();
			cancelButton = new System.Windows.Forms.Button();
			completionTimer = new System.Windows.Forms.Timer(components);
			SuspendLayout();
			progressBar1.Location = new System.Drawing.Point(12, 12);
			progressBar1.Name = "progressBar1";
			progressBar1.Size = new System.Drawing.Size(260, 23);
			progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			progressBar1.TabIndex = 0;
			cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			cancelButton.Location = new System.Drawing.Point(105, 41);
			cancelButton.Name = "cancelButton";
			cancelButton.Size = new System.Drawing.Size(75, 23);
			cancelButton.TabIndex = 1;
			cancelButton.Text = "albl_Cancel";
			cancelButton.UseVisualStyleBackColor = true;
			completionTimer.Enabled = true;
			completionTimer.Tick += new System.EventHandler(OnTick);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = cancelButton;
			base.ClientSize = new System.Drawing.Size(284, 73);
			base.ControlBox = false;
			base.Controls.Add(cancelButton);
			base.Controls.Add(progressBar1);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "WaitingDialog";
			base.ShowInTaskbar = false;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			ResumeLayout(false);
		}
	}
}
