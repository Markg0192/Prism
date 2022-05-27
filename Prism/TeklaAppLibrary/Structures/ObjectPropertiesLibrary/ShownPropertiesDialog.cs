#define DEBUG
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Tekla.Structures.ObjectPropertiesLibrary.Properties;
using Tekla.Structures.Properties;

namespace Tekla.Structures.ObjectPropertiesLibrary
{
	public class ShownPropertiesDialog : UserControl
	{
		private readonly ArrayList prevSelection = new ArrayList();

		private PresentedPropertiesXml allShownPresentedPropertiesXmlInstance;

		private PresentedPropertiesXml newShownPresentedPropertiesXmlInstance;

		private IContainer components = null;

		private DataGridView ShownPropertiesDGW;

		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;

		private ToolStrip toolStrip1;

		private ToolStripButton SaveTSB;

		private ToolStripButton LoadTSB;

		private DataGridViewCheckBoxColumn PropertyShown;

		private DataGridViewTextBoxColumn Property;

		public ShownPropertiesDialog()
		{
			InitializeComponent();
		}

		public ShownPropertiesDialog(Form parentForm, ref PresentedPropertiesXml allPresentedPropertiesXmlInstance, ref PresentedPropertiesXml shownPresentedPropertiesXmlInstance, LocalisationDelegate methodToLocalize)
		{
			InitializeComponent();
			methodToLocalize?.Invoke(this);
			ShownPropertiesDGW.AutoGenerateColumns = false;
			ShownPropertiesDGW.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			ShownPropertiesDGW.CellMouseClick += ShownPropertiesDGWCellMouseClick;
			ShownPropertiesDGW.SelectionChanged += AllPropertiesDGWSelectionChanged;
			newShownPresentedPropertiesXmlInstance = shownPresentedPropertiesXmlInstance;
			allShownPresentedPropertiesXmlInstance = allPresentedPropertiesXmlInstance;
			ShownPropertiesDGW.DataSource = newShownPresentedPropertiesXmlInstance.PropertiesList;
		}

		public void InitializeShownPropertiesDialog(Form newParentForm, ref PresentedPropertiesXml allPresentedPropertiesXmlInstance, ref PresentedPropertiesXml shownPresentedPropertiesXmlInstance, LocalisationDelegate methodToLocalize)
		{
			methodToLocalize?.Invoke(this);
			ShownPropertiesDGW.AutoGenerateColumns = false;
			ShownPropertiesDGW.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			ShownPropertiesDGW.CellMouseClick += ShownPropertiesDGWCellMouseClick;
			ShownPropertiesDGW.SelectionChanged += AllPropertiesDGWSelectionChanged;
			newShownPresentedPropertiesXmlInstance = shownPresentedPropertiesXmlInstance;
			allShownPresentedPropertiesXmlInstance = allPresentedPropertiesXmlInstance;
			ShownPropertiesDGW.DataSource = newShownPresentedPropertiesXmlInstance.PropertiesList;
		}

		public void RefreshDGW(SearchableSortableBindingList<PresentedProperties> reallyRefreshList)
		{
			SearchableSortableBindingList<PresentedProperties> searchableSortableBindingList2 = (SearchableSortableBindingList<PresentedProperties>)(ShownPropertiesDGW.DataSource = (newShownPresentedPropertiesXmlInstance.PropertiesList = reallyRefreshList));
			ShownPropertiesDGW.Refresh();
		}

		public void RemovePropertiesFromShown()
		{
			foreach (DataGridViewRow selectedRow in ShownPropertiesDGW.SelectedRows)
			{
				ShownPropertiesDGW.Rows.Remove(selectedRow);
			}
		}

		public void SaveShown()
		{
			newShownPresentedPropertiesXmlInstance.XmlWriteProperties(newShownPresentedPropertiesXmlInstance.PropertiesList);
			newShownPresentedPropertiesXmlInstance.ForceReadFile();
		}

		public void UpdateShownProperties()
		{
			PresentedPropertiesManage.MakeFileFromOtherFilesHiddenProperties(ref newShownPresentedPropertiesXmlInstance, ref allShownPresentedPropertiesXmlInstance);
			ShownPropertiesDGW.DataSource = newShownPresentedPropertiesXmlInstance.PropertiesList;
			Refresh();
		}

		private void AllPropertiesDGWSelectionChanged(object sender, EventArgs e)
		{
			prevSelection.Clear();
			foreach (DataGridViewRow selectedRow in ShownPropertiesDGW.SelectedRows)
			{
				PresentedProperties presentedProperties = selectedRow.DataBoundItem as PresentedProperties;
				if (presentedProperties != null)
				{
					prevSelection.Add(presentedProperties);
				}
			}
		}

		private void LoadTsbClick(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				InitialDirectory = (string.IsNullOrEmpty(Settings.Default.FileFolder) ? allShownPresentedPropertiesXmlInstance.LoadSaveDirectory : Settings.Default.FileFolder)
			};
			if (openFileDialog.ShowDialog() != DialogResult.OK)
			{
				return;
			}
			try
			{
				SearchableSortableBindingList<PresentedProperties> searchableSortableBindingList = PresentedPropertiesXml.ReadPropertiesListFromFile(openFileDialog.FileName);
				if (searchableSortableBindingList.Count > 0)
				{
					newShownPresentedPropertiesXmlInstance.PropertiesList = searchableSortableBindingList;
					Settings.Default.FileFolder = Path.GetDirectoryName(openFileDialog.FileName);
					Settings.Default.Save();
				}
				ShownPropertiesDGW.DataSource = newShownPresentedPropertiesXmlInstance.PropertiesList;
				ShownPropertiesDGW.Refresh();
				PresentedPropertiesManage.ChangeHiddenValuesByOtherFile(ref allShownPresentedPropertiesXmlInstance, ref newShownPresentedPropertiesXmlInstance);
				allShownPresentedPropertiesXmlInstance.ForceReadFile();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
			}
		}

		private void SaveTsbClick(object sender, EventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				InitialDirectory = (string.IsNullOrEmpty(Settings.Default.FileFolder) ? allShownPresentedPropertiesXmlInstance.LoadSaveDirectory : Settings.Default.FileFolder)
			};
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					PresentedPropertiesXml.XmlWriteProperties(newShownPresentedPropertiesXmlInstance.PropertiesList, saveFileDialog.FileName);
					Settings.Default.FileFolder = Path.GetDirectoryName(saveFileDialog.FileName);
					Settings.Default.Save();
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.ToString());
				}
			}
		}

		private void ShownPropertiesDGWCellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (!(ShownPropertiesDGW.Columns[e.ColumnIndex].Name == "PropertyShown") || ShownPropertiesDGW.CurrentRow == null)
			{
				return;
			}
			bool flag = !(bool)ShownPropertiesDGW.CurrentCell.Value;
			PresentedProperties presentedProperties = ShownPropertiesDGW.CurrentRow.DataBoundItem as PresentedProperties;
			if (presentedProperties != null)
			{
				presentedProperties.Hidden = !flag;
			}
			foreach (PresentedProperties item in prevSelection)
			{
				if (item != null)
				{
					item.Hidden = !flag;
				}
			}
			ShownPropertiesDGW.Refresh();
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			ShownPropertiesDGW = new System.Windows.Forms.DataGridView();
			PropertyShown = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			toolStrip1 = new System.Windows.Forms.ToolStrip();
			SaveTSB = new System.Windows.Forms.ToolStripButton();
			LoadTSB = new System.Windows.Forms.ToolStripButton();
			dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			Property = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)ShownPropertiesDGW).BeginInit();
			toolStrip1.SuspendLayout();
			SuspendLayout();
			ShownPropertiesDGW.AllowUserToAddRows = false;
			ShownPropertiesDGW.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			ShownPropertiesDGW.BackgroundColor = System.Drawing.SystemColors.Window;
			ShownPropertiesDGW.BorderStyle = System.Windows.Forms.BorderStyle.None;
			ShownPropertiesDGW.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			dataGridViewCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			dataGridViewCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			ShownPropertiesDGW.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle;
			ShownPropertiesDGW.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			ShownPropertiesDGW.Columns.AddRange(PropertyShown, Property);
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			ShownPropertiesDGW.DefaultCellStyle = dataGridViewCellStyle2;
			ShownPropertiesDGW.Location = new System.Drawing.Point(-1, 35);
			ShownPropertiesDGW.Name = "ShownPropertiesDGW";
			ShownPropertiesDGW.ReadOnly = true;
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			ShownPropertiesDGW.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
			ShownPropertiesDGW.RowHeadersVisible = false;
			ShownPropertiesDGW.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			ShownPropertiesDGW.Size = new System.Drawing.Size(240, 436);
			ShownPropertiesDGW.TabIndex = 0;
			PropertyShown.DataPropertyName = "Visible";
			PropertyShown.FalseValue = "";
			PropertyShown.HeaderText = "albl_Visible";
			PropertyShown.Name = "PropertyShown";
			PropertyShown.ReadOnly = true;
			PropertyShown.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			PropertyShown.TrueValue = "";
			PropertyShown.Width = 40;
			toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { SaveTSB, LoadTSB });
			toolStrip1.Location = new System.Drawing.Point(0, 0);
			toolStrip1.Name = "toolStrip1";
			toolStrip1.Size = new System.Drawing.Size(239, 25);
			toolStrip1.TabIndex = 1;
			toolStrip1.Text = "toolStrip1";
			SaveTSB.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			SaveTSB.Image = Tekla.Structures.ObjectPropertiesLibrary.Properties.Resources.save_as_big;
			SaveTSB.ImageTransparentColor = System.Drawing.Color.Magenta;
			SaveTSB.Name = "SaveTSB";
			SaveTSB.Size = new System.Drawing.Size(23, 22);
			SaveTSB.ToolTipText = "albl_Save_view";
			SaveTSB.Click += new System.EventHandler(SaveTsbClick);
			LoadTSB.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			LoadTSB.Image = Tekla.Structures.ObjectPropertiesLibrary.Properties.Resources.open_big;
			LoadTSB.ImageTransparentColor = System.Drawing.Color.Magenta;
			LoadTSB.Name = "LoadTSB";
			LoadTSB.Size = new System.Drawing.Size(23, 22);
			LoadTSB.ToolTipText = "albl_Load_view";
			LoadTSB.Click += new System.EventHandler(LoadTsbClick);
			dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewTextBoxColumn1.DataPropertyName = "Name";
			dataGridViewTextBoxColumn1.HeaderText = "";
			dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			dataGridViewTextBoxColumn1.ReadOnly = true;
			dataGridViewTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			Property.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			Property.DataPropertyName = "Name";
			Property.HeaderText = "albl_Name";
			Property.Name = "Property";
			Property.ReadOnly = true;
			Property.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(toolStrip1);
			base.Controls.Add(ShownPropertiesDGW);
			base.Name = "ShownPropertiesDialog";
			base.Size = new System.Drawing.Size(239, 472);
			((System.ComponentModel.ISupportInitialize)ShownPropertiesDGW).EndInit();
			toolStrip1.ResumeLayout(false);
			toolStrip1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
