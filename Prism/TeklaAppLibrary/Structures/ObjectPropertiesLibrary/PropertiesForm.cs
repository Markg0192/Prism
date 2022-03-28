using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Tekla.Structures.Dialog;

namespace Tekla.Structures.ObjectPropertiesLibrary
{
	public class PropertiesForm : ApplicationFormBase
	{
		private PresentedPropertiesXml ownAllPresentedPropertiesXmlInstance;

		private PresentedPropertiesXml ownShownPresentedPropertiesXmlInstance;

		private IContainer components = null;

		private AllPropertiesDialog allPropertiesDialog1;

		private ShownPropertiesDialog shownPropertiesDialog1;

		private Button button1;

		private Button button2;

		private Button OkButton;

		private Button CancelButton;

		private SplitContainer splitContainer1;

		private Panel panel1;

		public PropertiesForm()
			: this()
		{
			InitializeComponent();
		}

		public PropertiesForm(Form parentForm, ref PresentedPropertiesXml allPresentedPropertiesXmlInstance, ref PresentedPropertiesXml shownPresentedPropertiesXmlInstance, LocalisationDelegate localizationMethod, bool showShownProperties)
			: this()
		{
			InitializeComponent();
			((Form)this).StartPosition = FormStartPosition.Manual;
			if (parentForm != null)
			{
				((Form)this).TopMost = parentForm.TopMost;
				((Form)this).Location = GetLocation(parentForm.Location);
			}
			else
			{
				((Form)this).Location = GetLocation(Screen.PrimaryScreen.WorkingArea.Location);
			}
			splitContainer1.Panel1Collapsed = !showShownProperties;
			allPropertiesDialog1.ShowIncludedColumn(!showShownProperties);
			ownAllPresentedPropertiesXmlInstance = allPresentedPropertiesXmlInstance;
			ownShownPresentedPropertiesXmlInstance = shownPresentedPropertiesXmlInstance;
			allPropertiesDialog1.InitializeAllPropertiesDialog(parentForm, ref allPresentedPropertiesXmlInstance, ref shownPresentedPropertiesXmlInstance, localizationMethod);
			shownPropertiesDialog1.InitializeShownPropertiesDialog(parentForm, ref allPresentedPropertiesXmlInstance, ref shownPresentedPropertiesXmlInstance, localizationMethod);
			localizationMethod?.Invoke((Control)(object)this);
			((Form)this).FormClosing += PropertiesFormFormClosing;
		}

		private void Button1Click(object sender, EventArgs e)
		{
			allPropertiesDialog1.ShowSelected();
			shownPropertiesDialog1.UpdateShownProperties();
		}

		private void Button2Click(object sender, EventArgs e)
		{
			shownPropertiesDialog1.RemovePropertiesFromShown();
			allPropertiesDialog1.UpdateAllProperties();
		}

		private Point GetLocation(Point parentLocation)
		{
			Point result = new Point(parentLocation.X, parentLocation.Y + 24);
			int num = parentLocation.X + ((Control)this).Width;
			int num2 = parentLocation.Y + ((Control)this).Height;
			Rectangle workingArea = Screen.GetWorkingArea(parentLocation);
			if (num > workingArea.Width)
			{
				result.X -= num - workingArea.Width;
			}
			if (num2 > workingArea.Height)
			{
				result.Y -= num2 - workingArea.Height;
			}
			return result;
		}

		private void OkButtonClick(object sender, EventArgs e)
		{
			ownAllPresentedPropertiesXmlInstance.XmlWriteProperties(ownAllPresentedPropertiesXmlInstance.PropertiesList);
			PresentedPropertiesManage.MakeFileFromOtherFilesHiddenProperties(ref ownShownPresentedPropertiesXmlInstance, ref ownAllPresentedPropertiesXmlInstance);
		}

		private void PropertiesFormFormClosing(object sender, FormClosingEventArgs e)
		{
			ownAllPresentedPropertiesXmlInstance.ForceReadFile();
			ownShownPresentedPropertiesXmlInstance.ForceReadFile();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			((FormBase)this).Dispose(disposing);
		}

		private void InitializeComponent()
		{
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(PropertiesForm));
			button1 = new Button();
			button2 = new Button();
			OkButton = new Button();
			CancelButton = new Button();
			splitContainer1 = new SplitContainer();
			panel1 = new Panel();
			shownPropertiesDialog1 = new ShownPropertiesDialog();
			allPropertiesDialog1 = new AllPropertiesDialog();
			((ISupportInitialize)splitContainer1).BeginInit();
			splitContainer1.Panel1.SuspendLayout();
			splitContainer1.Panel2.SuspendLayout();
			splitContainer1.SuspendLayout();
			panel1.SuspendLayout();
			((Control)this).SuspendLayout();
			((FormBase)this).structuresExtender.SetAttributeName((Control)button1, (string)null);
			((FormBase)this).structuresExtender.SetAttributeTypeName((Control)button1, (string)null);
			((FormBase)this).structuresExtender.SetBindPropertyName((Control)button1, (string)null);
			button1.Location = new Point(7, 98);
			button1.Name = "button1";
			button1.Size = new Size(48, 25);
			button1.TabIndex = 3;
			button1.Text = "<-";
			button1.UseVisualStyleBackColor = true;
			button1.Click += Button1Click;
			((FormBase)this).structuresExtender.SetAttributeName((Control)button2, (string)null);
			((FormBase)this).structuresExtender.SetAttributeTypeName((Control)button2, (string)null);
			((FormBase)this).structuresExtender.SetBindPropertyName((Control)button2, (string)null);
			button2.Location = new Point(7, 141);
			button2.Name = "button2";
			button2.Size = new Size(48, 25);
			button2.TabIndex = 4;
			button2.Text = "->";
			button2.UseVisualStyleBackColor = true;
			button2.Click += Button2Click;
			OkButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			((FormBase)this).structuresExtender.SetAttributeName((Control)OkButton, (string)null);
			((FormBase)this).structuresExtender.SetAttributeTypeName((Control)OkButton, (string)null);
			((FormBase)this).structuresExtender.SetBindPropertyName((Control)OkButton, (string)null);
			OkButton.DialogResult = DialogResult.OK;
			OkButton.Location = new Point(872, 382);
			OkButton.Name = "OkButton";
			OkButton.Size = new Size(108, 21);
			OkButton.TabIndex = 5;
			OkButton.Text = "albl_OK";
			OkButton.UseVisualStyleBackColor = true;
			OkButton.Click += OkButtonClick;
			CancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			((FormBase)this).structuresExtender.SetAttributeName((Control)CancelButton, (string)null);
			((FormBase)this).structuresExtender.SetAttributeTypeName((Control)CancelButton, (string)null);
			((FormBase)this).structuresExtender.SetBindPropertyName((Control)CancelButton, (string)null);
			CancelButton.DialogResult = DialogResult.Cancel;
			CancelButton.Location = new Point(763, 382);
			CancelButton.Name = "CancelButton";
			CancelButton.Size = new Size(103, 21);
			CancelButton.TabIndex = 6;
			CancelButton.Text = "albl_Cancel";
			CancelButton.UseVisualStyleBackColor = true;
			splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			((FormBase)this).structuresExtender.SetAttributeName((Control)splitContainer1, (string)null);
			((FormBase)this).structuresExtender.SetAttributeTypeName((Control)splitContainer1, (string)null);
			((FormBase)this).structuresExtender.SetBindPropertyName((Control)splitContainer1, (string)null);
			splitContainer1.Location = new Point(1, -1);
			splitContainer1.Name = "splitContainer1";
			((FormBase)this).structuresExtender.SetAttributeName((Control)splitContainer1.Panel1, (string)null);
			((FormBase)this).structuresExtender.SetAttributeTypeName((Control)splitContainer1.Panel1, (string)null);
			((FormBase)this).structuresExtender.SetBindPropertyName((Control)splitContainer1.Panel1, (string)null);
			splitContainer1.Panel1.Controls.Add(panel1);
			splitContainer1.Panel1.Controls.Add(shownPropertiesDialog1);
			((FormBase)this).structuresExtender.SetAttributeName((Control)splitContainer1.Panel2, (string)null);
			((FormBase)this).structuresExtender.SetAttributeTypeName((Control)splitContainer1.Panel2, (string)null);
			((FormBase)this).structuresExtender.SetBindPropertyName((Control)splitContainer1.Panel2, (string)null);
			splitContainer1.Panel2.Controls.Add(allPropertiesDialog1);
			splitContainer1.Size = new Size(994, 377);
			splitContainer1.SplitterDistance = 321;
			splitContainer1.TabIndex = 7;
			panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
			((FormBase)this).structuresExtender.SetAttributeName((Control)panel1, (string)null);
			((FormBase)this).structuresExtender.SetAttributeTypeName((Control)panel1, (string)null);
			((FormBase)this).structuresExtender.SetBindPropertyName((Control)panel1, (string)null);
			panel1.Controls.Add(button1);
			panel1.Controls.Add(button2);
			panel1.Location = new Point(259, 0);
			panel1.Name = "panel1";
			panel1.Size = new Size(62, 377);
			panel1.TabIndex = 0;
			((FormBase)this).structuresExtender.SetAttributeName((Control)shownPropertiesDialog1, (string)null);
			((FormBase)this).structuresExtender.SetAttributeTypeName((Control)shownPropertiesDialog1, (string)null);
			((FormBase)this).structuresExtender.SetBindPropertyName((Control)shownPropertiesDialog1, (string)null);
			shownPropertiesDialog1.Dock = DockStyle.Fill;
			shownPropertiesDialog1.Location = new Point(0, 0);
			shownPropertiesDialog1.Name = "shownPropertiesDialog1";
			shownPropertiesDialog1.Size = new Size(321, 377);
			shownPropertiesDialog1.TabIndex = 2;
			((FormBase)this).structuresExtender.SetAttributeName((Control)allPropertiesDialog1, (string)null);
			((FormBase)this).structuresExtender.SetAttributeTypeName((Control)allPropertiesDialog1, (string)null);
			((FormBase)this).structuresExtender.SetBindPropertyName((Control)allPropertiesDialog1, (string)null);
			allPropertiesDialog1.Dock = DockStyle.Fill;
			allPropertiesDialog1.Location = new Point(0, 0);
			allPropertiesDialog1.Name = "allPropertiesDialog1";
			allPropertiesDialog1.Size = new Size(669, 377);
			allPropertiesDialog1.TabIndex = 1;
			((FormBase)this).structuresExtender.SetAttributeName((Control)(object)this, (string)null);
			((FormBase)this).structuresExtender.SetAttributeTypeName((Control)(object)this, (string)null);
			((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
			((ContainerControl)this).AutoScaleMode = AutoScaleMode.Font;
			((FormBase)this).structuresExtender.SetBindPropertyName((Control)(object)this, (string)null);
			((Form)this).ClientSize = new Size(996, 406);
			((Control)this).Controls.Add(CancelButton);
			((Control)this).Controls.Add(OkButton);
			((Control)this).Controls.Add(splitContainer1);
			((Form)this).Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
			((Control)this).Name = "PropertiesForm";
			((Control)(object)this).Text = "albl_Properties";
			splitContainer1.Panel1.ResumeLayout(performLayout: false);
			splitContainer1.Panel2.ResumeLayout(performLayout: false);
			((ISupportInitialize)splitContainer1).EndInit();
			splitContainer1.ResumeLayout(performLayout: false);
			panel1.ResumeLayout(performLayout: false);
			((Control)this).ResumeLayout(performLayout: false);
		}
	}
}
