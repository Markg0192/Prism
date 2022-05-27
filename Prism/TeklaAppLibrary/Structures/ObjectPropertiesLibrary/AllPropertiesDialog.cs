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
	public class AllPropertiesDialog : UserControl
	{
		private readonly ArrayList prevSelection = new ArrayList();

		private readonly Label validationErrorLabel = new Label();

		private PresentedPropertiesXml allShownPresentedPropertiesXmlInstance;

		private PresentedPropertiesXml newShownPresentedPropertiesXmlInstance;

		private QuickSearchForDGW quickSearchUserCntrl;

		private IContainer components = null;

		private DataGridView AllPropertiesDGW;

		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;

		private ToolStrip AllPropertiesToolStrip;

		private ToolStripButton toolStripButton1;

		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;

		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;

		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;

		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;

		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;

		private ToolStripButton PasteNormalToolStripButton;

		private ToolStripButton PasteExteralToolStripButton;

		private ToolStripButton LoadProperties;

		private ToolStripButton SaveProperties;

		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;

		private ContextMenuStrip contextMenuStrip1;

		private ToolStripMenuItem alblIncludeselectedToolStripMenuItem;

		private ToolStripMenuItem alblExcludeselectedToolStripMenuItem;

		private ToolStripSeparator toolStripSeparator1;

		private ToolStripMenuItem alblRemoveselectedToolStripMenuItem;

		private ToolStripButton toolStripButton2;

		private ToolStripButton toolStripButton3;

		private ToolStripSeparator toolStripSeparator2;

		private ToolStripMenuItem toolStripMenuItem1;

		private ToolStripMenuItem toolStripMenuItem2;

		private DataGridViewCheckBoxColumn PropertyShown;

		private DataGridViewTextBoxColumn Property;

		private DataGridViewTextBoxColumn ReportProperty;

		private DataGridViewTextBoxColumn UdaProperty;

		private DataGridViewTextBoxColumn Type;

		private DataGridViewTextBoxColumn DisplayType;

		private DataGridViewTextBoxColumn Decimals;

		private DataGridViewTextBoxColumn ColWidth;

		public PresentedPropertiesXml AllShownPresentedPropertiesXmlInstance
		{
			get
			{
				return allShownPresentedPropertiesXmlInstance;
			}
			set
			{
				allShownPresentedPropertiesXmlInstance = value;
			}
		}

		public AllPropertiesDialog()
		{
			InitializeComponent();
		}

		public AllPropertiesDialog(Form parentForm, ref PresentedPropertiesXml allPresentedPropertiesXmlInstance, ref PresentedPropertiesXml shownPresentedPropertiesXmlInstance, LocalisationDelegate methodToLocalize)
		{
			InitializeComponent();
			InitializeAllPropertiesDialog(parentForm, ref allPresentedPropertiesXmlInstance, ref shownPresentedPropertiesXmlInstance, methodToLocalize);
		}

		public void InitializeAllPropertiesDialog(Form newParentForm, ref PresentedPropertiesXml allPresentedPropertiesXmlInstance, ref PresentedPropertiesXml shownPresentedPropertiesXmlInstance, LocalisationDelegate methodToLocalize)
		{
			quickSearchUserCntrl = new QuickSearchForDGW();
			quickSearchUserCntrl.SetReferenceProperties(ref AllPropertiesDGW);
			ToolStripControlHost value = new ToolStripControlHost(quickSearchUserCntrl)
			{
				Alignment = ToolStripItemAlignment.Right
			};
			quickSearchUserCntrl.Width = 140;
			AllPropertiesToolStrip.SuspendLayout();
			AllPropertiesToolStrip.Items.Add(value);
			AllPropertiesToolStrip.ResumeLayout();
			if (methodToLocalize != null)
			{
				methodToLocalize(this);
				methodToLocalize(contextMenuStrip1);
				validationErrorLabel.Text = "albl_Change_default_property_name";
				methodToLocalize(validationErrorLabel);
			}
			AllPropertiesDGW.AutoGenerateColumns = false;
			AllPropertiesDGW.SelectionChanged += ShownPropertiesDGWSelectionChanged;
			AllPropertiesDGW.KeyDown += AllPropertiesDGWKeyDown;
			AllPropertiesDGW.Sorted += AllPropertiesDGWSorted;
			PresentedPropertiesManage.ChangeHiddenValuesByOtherFile(ref allPresentedPropertiesXmlInstance, ref shownPresentedPropertiesXmlInstance);
			allPresentedPropertiesXmlInstance.ForceReadFile();
			newShownPresentedPropertiesXmlInstance = shownPresentedPropertiesXmlInstance;
			allShownPresentedPropertiesXmlInstance = allPresentedPropertiesXmlInstance;
			AllPropertiesDGW.DataSource = allShownPresentedPropertiesXmlInstance.PropertiesList;
			AllPropertiesDGW.ReadOnly = false;
			AllPropertiesDGW.AllowUserToAddRows = true;
			AllPropertiesDGW.AllowUserToDeleteRows = true;
			AllPropertiesDGW.SelectionMode = DataGridViewSelectionMode.CellSelect;
			AllPropertiesDGW.Columns[0].ReadOnly = false;
		}

		public void ShowIncludedColumn(bool show)
		{
			foreach (DataGridViewColumn column in AllPropertiesDGW.Columns)
			{
				if (column.Name == "PropertyShown")
				{
					column.Visible = show;
				}
			}
		}

		public void ShowSelected()
		{
			foreach (PresentedProperties item in prevSelection)
			{
				if (item != null)
				{
					item.Visible = true;
				}
			}
		}

		public void UpdateAllProperties()
		{
			PresentedPropertiesManage.ChangeHiddenValuesByOtherFile(ref allShownPresentedPropertiesXmlInstance, ref newShownPresentedPropertiesXmlInstance);
			AllPropertiesDGW.DataSource = allShownPresentedPropertiesXmlInstance.PropertiesList;
			Refresh();
		}

		private void AddProperties(bool external)
		{
			string text = null;
			char[] separator = new char[1] { '\n' };
			try
			{
				if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text))
				{
					text = Clipboard.GetDataObject().GetData(DataFormats.Text).ToString();
				}
				if (AllPropertiesDGW.Rows.Count != 0)
				{
					allShownPresentedPropertiesXmlInstance.PropertiesList.CancelNew(AllPropertiesDGW.Rows.Count - 1);
				}
				if (text == null)
				{
					return;
				}
				AllPropertiesDGW.SuspendLayout();
				string[] array = text.Split(separator);
				string[] array2 = array;
				foreach (string text2 in array2)
				{
					int num = text2.IndexOf(':');
					string text3 = text2.Substring(0, num);
					string valueString = null;
					if (text2.Length > num)
					{
						valueString = text2.Substring(num + 1);
					}
					text3 = text3.Trim();
					if (external)
					{
						text3 = "EXTERNAL." + text3;
					}
					allShownPresentedPropertiesXmlInstance.AddPropertyWithReportName(text3, valueString);
				}
				AllPropertiesDGW.ResumeLayout();
				AllPropertiesDGW.Refresh();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
				AllPropertiesDGW.ResumeLayout();
				AllPropertiesDGW.Refresh();
			}
		}

		private void AllPropertiesDGWCellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (AllPropertiesDGW.Columns[e.ColumnIndex].Name == "PropertyShown")
			{
				CancelEditIfPropertyIsDefault(AllPropertiesDGW.Rows.Count - 1, null);
			}
		}

		private void AllPropertiesDGWCellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (AllPropertiesDGW.Columns[e.ColumnIndex].Name == "PropertyShown")
			{
				CancelEditIfPropertyIsDefault(AllPropertiesDGW.Rows.Count - 1, null);
			}
		}

		private void AllPropertiesDGWRowValidating(object sender, DataGridViewCellCancelEventArgs e)
		{
			CancelEditIfPropertyIsDefault(e.RowIndex, e);
		}

		private void AllPropertiesDGWCellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			if (e.ColumnIndex == 1 && e.FormattedValue.ToString() == "New Property")
			{
				if (base.ParentForm != null)
				{
					MessageBox.Show(validationErrorLabel.Text, base.ParentForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
				}
				e.Cancel = true;
			}
		}

		private void AllPropertiesDGWKeyDown(object sender, KeyEventArgs e)
		{
			Keys keyCode = e.KeyCode;
			Keys keys = keyCode;
			if (keys != Keys.V || !e.Control)
			{
				return;
			}
			string value = Clipboard.GetText();
			foreach (DataGridViewCell selectedCell in AllPropertiesDGW.SelectedCells)
			{
				if (AllPropertiesDGW.Rows[AllPropertiesDGW.Rows.Count - 1].Cells[1] == selectedCell)
				{
					if (AllPropertiesDGW.Rows.Count != 0)
					{
						allShownPresentedPropertiesXmlInstance.PropertiesList.CancelNew(AllPropertiesDGW.Rows.Count - 1);
					}
					allShownPresentedPropertiesXmlInstance.PropertiesList.AddNew();
				}
				selectedCell.Value = value;
			}
		}

		private void AllPropertiesDGWMouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				contextMenuStrip1.Show();
			}
		}

		private void AllPropertiesDGWSorted(object sender, EventArgs e)
		{
			quickSearchUserCntrl.QuickSearchExecute();
		}

		private void CancelEditIfPropertyIsDefault(int rowNumber, DataGridViewCellCancelEventArgs e)
		{
			if (AllPropertiesDGW.Rows.Count > 0)
			{
				object value = AllPropertiesDGW.Rows[rowNumber].Cells["Property"].Value;
				if (AllPropertiesDGW.Rows[rowNumber].Cells["Property"].Value != null && AllPropertiesDGW.Rows[rowNumber].Cells["Property"].Value.ToString() == "New Property" && e == null)
				{
					AllPropertiesDGW.CancelEdit();
				}
			}
		}

		private void GridClicked(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				if (e.Location.Y < AllPropertiesDGW.ColumnHeadersHeight)
				{
					ShowHideColumns(e.Location);
				}
				else
				{
					contextMenuStrip1.Show(this, e.Location);
				}
			}
		}

		private void LoadPropertiesClick(object sender, EventArgs e)
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
					allShownPresentedPropertiesXmlInstance.PropertiesList = searchableSortableBindingList;
					Settings.Default.FileFolder = Path.GetDirectoryName(openFileDialog.FileName);
					Settings.Default.Save();
				}
				AllPropertiesDGW.DataSource = allShownPresentedPropertiesXmlInstance.PropertiesList;
				AllPropertiesDGW.Refresh();
				PresentedPropertiesManage.MakeFileFromOtherFilesHiddenProperties(ref newShownPresentedPropertiesXmlInstance, ref allShownPresentedPropertiesXmlInstance);
				newShownPresentedPropertiesXmlInstance.ForceReadFile();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
			}
		}

		private void RemoveSelectedRows()
		{
			ArrayList arrayList = new ArrayList(prevSelection);
			foreach (PresentedProperties item in arrayList)
			{
				allShownPresentedPropertiesXmlInstance.RemoveProperty(item);
			}
			AllPropertiesDGW.Refresh();
		}

		private void SavePropertiesClick(object sender, EventArgs e)
		{
			AllPropertiesDGW.EndEdit();
			allShownPresentedPropertiesXmlInstance.XmlWriteProperties(allShownPresentedPropertiesXmlInstance.PropertiesList);
			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				InitialDirectory = (string.IsNullOrEmpty(Settings.Default.FileFolder) ? allShownPresentedPropertiesXmlInstance.LoadSaveDirectory : Settings.Default.FileFolder)
			};
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					PresentedPropertiesXml.XmlWriteProperties(allShownPresentedPropertiesXmlInstance.PropertiesList, saveFileDialog.FileName);
					Settings.Default.FileFolder = Path.GetDirectoryName(saveFileDialog.FileName);
					Settings.Default.Save();
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.ToString());
				}
			}
		}

		private void ShowHideColumns(Point showLocation)
		{
			if (AllPropertiesDGW == null)
			{
				return;
			}
			MenuItem[] array = new MenuItem[AllPropertiesDGW.Columns.Count];
			int num = 0;
			foreach (DataGridViewColumn column in AllPropertiesDGW.Columns)
			{
				MenuItem menuItem = new MenuItem(column.HeaderText, ShowHideOnClick);
				if (column.Visible)
				{
					menuItem.Checked = true;
				}
				array[num] = menuItem;
				num++;
			}
			ContextMenu contextMenu = new ContextMenu(array);
			contextMenu.Show(this, showLocation);
		}

		private void ShowHideOnClick(object sender, EventArgs e)
		{
			MenuItem menuItem = sender as MenuItem;
			if (menuItem == null)
			{
				return;
			}
			foreach (DataGridViewColumn column in AllPropertiesDGW.Columns)
			{
				if (column.HeaderText == menuItem.Text)
				{
					column.Visible = !menuItem.Checked;
					menuItem.Checked = !menuItem.Checked;
				}
			}
		}

		private void ShownPropertiesDGWSelectionChanged(object sender, EventArgs e)
		{
			prevSelection.Clear();
			foreach (DataGridViewCell selectedCell in AllPropertiesDGW.SelectedCells)
			{
				PresentedProperties presentedProperties = AllPropertiesDGW.Rows[selectedCell.RowIndex].DataBoundItem as PresentedProperties;
				if (presentedProperties != null && !prevSelection.Contains(presentedProperties))
				{
					prevSelection.Add(presentedProperties);
				}
			}
		}

		private void AlblExcludeselectedToolStripMenuItemClick(object sender, EventArgs e)
		{
			foreach (PresentedProperties item in prevSelection)
			{
				item.Visible = false;
			}
			AllPropertiesDGW.Refresh();
		}

		private void AlblIncludeselectedToolStripMenuItemClick(object sender, EventArgs e)
		{
			foreach (PresentedProperties item in prevSelection)
			{
				item.Visible = true;
			}
			AllPropertiesDGW.Refresh();
		}

		private void AlblRemoveselectedToolStripMenuItemClick(object sender, EventArgs e)
		{
			RemoveSelectedRows();
		}

		private void ToolStripButton1Click(object sender, EventArgs e)
		{
			if (!toolStripButton1.Checked)
			{
				AllPropertiesDGW.ReadOnly = true;
				AllPropertiesDGW.AllowUserToAddRows = false;
				AllPropertiesDGW.AllowUserToDeleteRows = true;
				AllPropertiesDGW.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			}
			else
			{
				AllPropertiesDGW.ReadOnly = false;
				AllPropertiesDGW.AllowUserToAddRows = true;
				AllPropertiesDGW.AllowUserToDeleteRows = false;
				AllPropertiesDGW.SelectionMode = DataGridViewSelectionMode.CellSelect;
				AllPropertiesDGW.Columns[0].ReadOnly = true;
			}
		}

		private void ToolStripButton2Click(object sender, EventArgs e)
		{
			if (AllPropertiesDGW.CurrentCell == null || !AllPropertiesDGW.CurrentCell.IsInEditMode)
			{
				AllPropertiesDGW.CurrentCell = AllPropertiesDGW.Rows[AllPropertiesDGW.Rows.Count - 1].Cells[1];
			}
		}

		private void ToolStripButton3Click(object sender, EventArgs e)
		{
			if (AllPropertiesDGW.CurrentCell == null || !AllPropertiesDGW.CurrentCell.IsInEditMode)
			{
				RemoveSelectedRows();
			}
		}

		private void ToolStripMenuItem1Click(object sender, EventArgs e)
		{
			AddProperties(external: true);
		}

		private void ToolStripMenuItem2Click(object sender, EventArgs e)
		{
			AddProperties(external: false);
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
			AllPropertiesDGW = new System.Windows.Forms.DataGridView();
			PropertyShown = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			Property = new System.Windows.Forms.DataGridViewTextBoxColumn();
			ReportProperty = new System.Windows.Forms.DataGridViewTextBoxColumn();
			UdaProperty = new System.Windows.Forms.DataGridViewTextBoxColumn();
			Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
			DisplayType = new System.Windows.Forms.DataGridViewTextBoxColumn();
			Decimals = new System.Windows.Forms.DataGridViewTextBoxColumn();
			ColWidth = new System.Windows.Forms.DataGridViewTextBoxColumn();
			contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
			alblIncludeselectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			alblExcludeselectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			alblRemoveselectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			AllPropertiesToolStrip = new System.Windows.Forms.ToolStrip();
			toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			PasteNormalToolStripButton = new System.Windows.Forms.ToolStripButton();
			PasteExteralToolStripButton = new System.Windows.Forms.ToolStripButton();
			LoadProperties = new System.Windows.Forms.ToolStripButton();
			SaveProperties = new System.Windows.Forms.ToolStripButton();
			toolStripButton2 = new System.Windows.Forms.ToolStripButton();
			toolStripButton3 = new System.Windows.Forms.ToolStripButton();
			dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)AllPropertiesDGW).BeginInit();
			contextMenuStrip1.SuspendLayout();
			AllPropertiesToolStrip.SuspendLayout();
			SuspendLayout();
			AllPropertiesDGW.AllowUserToAddRows = false;
			AllPropertiesDGW.AllowUserToDeleteRows = false;
			AllPropertiesDGW.AllowUserToResizeRows = false;
			AllPropertiesDGW.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			AllPropertiesDGW.BackgroundColor = System.Drawing.SystemColors.Window;
			AllPropertiesDGW.BorderStyle = System.Windows.Forms.BorderStyle.None;
			AllPropertiesDGW.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			AllPropertiesDGW.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			AllPropertiesDGW.Columns.AddRange(PropertyShown, Property, ReportProperty, UdaProperty, Type, DisplayType, Decimals, ColWidth);
			AllPropertiesDGW.Location = new System.Drawing.Point(2, 36);
			AllPropertiesDGW.Name = "AllPropertiesDGW";
			AllPropertiesDGW.ReadOnly = true;
			AllPropertiesDGW.RowHeadersVisible = false;
			AllPropertiesDGW.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			AllPropertiesDGW.Size = new System.Drawing.Size(665, 431);
			AllPropertiesDGW.TabIndex = 0;
			AllPropertiesDGW.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(AllPropertiesDGWCellClick);
			AllPropertiesDGW.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(AllPropertiesDGWCellDoubleClick);
			AllPropertiesDGW.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(AllPropertiesDGWCellValidating);
			AllPropertiesDGW.RowValidating += new System.Windows.Forms.DataGridViewCellCancelEventHandler(AllPropertiesDGWRowValidating);
			AllPropertiesDGW.MouseClick += new System.Windows.Forms.MouseEventHandler(GridClicked);
			PropertyShown.DataPropertyName = "Visible";
			dataGridViewCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle.BackColor = System.Drawing.Color.White;
			dataGridViewCellStyle.NullValue = false;
			PropertyShown.DefaultCellStyle = dataGridViewCellStyle;
			PropertyShown.FalseValue = "false";
			PropertyShown.HeaderText = "albl_Included";
			PropertyShown.Name = "PropertyShown";
			PropertyShown.ReadOnly = true;
			PropertyShown.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			PropertyShown.TrueValue = "true";
			PropertyShown.Width = 66;
			Property.DataPropertyName = "Name";
			Property.HeaderText = "albl_Name";
			Property.Name = "Property";
			Property.ReadOnly = true;
			Property.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			Property.Width = 156;
			ReportProperty.DataPropertyName = "ReportPropertyName";
			ReportProperty.HeaderText = "albl_Report_property";
			ReportProperty.Name = "ReportProperty";
			ReportProperty.ReadOnly = true;
			UdaProperty.DataPropertyName = "UdaPropertyName";
			UdaProperty.HeaderText = "albl_UDA_name";
			UdaProperty.Name = "UdaProperty";
			UdaProperty.ReadOnly = true;
			Type.DataPropertyName = "PropertyType";
			Type.HeaderText = "albl_Type";
			Type.Name = "Type";
			Type.ReadOnly = true;
			DisplayType.HeaderText = "albl_Displayed_type";
			DisplayType.Name = "DisplayType";
			DisplayType.ReadOnly = true;
			DisplayType.Visible = false;
			Decimals.DataPropertyName = "Decimals";
			Decimals.HeaderText = "albl_Decimals";
			Decimals.Name = "Decimals";
			Decimals.ReadOnly = true;
			Decimals.Width = 70;
			ColWidth.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			ColWidth.DataPropertyName = "Width";
			ColWidth.HeaderText = "albl_Width";
			ColWidth.Name = "ColWidth";
			ColWidth.ReadOnly = true;
			contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[7] { alblIncludeselectedToolStripMenuItem, alblExcludeselectedToolStripMenuItem, toolStripSeparator2, toolStripMenuItem1, toolStripMenuItem2, toolStripSeparator1, alblRemoveselectedToolStripMenuItem });
			contextMenuStrip1.Name = "contextMenuStrip1";
			contextMenuStrip1.Size = new System.Drawing.Size(231, 126);
			alblIncludeselectedToolStripMenuItem.Name = "alblIncludeselectedToolStripMenuItem";
			alblIncludeselectedToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
			alblIncludeselectedToolStripMenuItem.Text = "albl_Include_selected";
			alblIncludeselectedToolStripMenuItem.Click += new System.EventHandler(AlblIncludeselectedToolStripMenuItemClick);
			alblExcludeselectedToolStripMenuItem.Name = "alblExcludeselectedToolStripMenuItem";
			alblExcludeselectedToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
			alblExcludeselectedToolStripMenuItem.Text = "albl_Exclude_selected";
			alblExcludeselectedToolStripMenuItem.Click += new System.EventHandler(AlblExcludeselectedToolStripMenuItemClick);
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new System.Drawing.Size(227, 6);
			toolStripMenuItem1.Name = "toolStripMenuItem1";
			toolStripMenuItem1.Size = new System.Drawing.Size(230, 22);
			toolStripMenuItem1.Text = "albl_Paste_external_properties";
			toolStripMenuItem1.Click += new System.EventHandler(ToolStripMenuItem1Click);
			toolStripMenuItem2.Name = "toolStripMenuItem2";
			toolStripMenuItem2.Size = new System.Drawing.Size(230, 22);
			toolStripMenuItem2.Text = "albl_Paste_model_properties";
			toolStripMenuItem2.Click += new System.EventHandler(ToolStripMenuItem2Click);
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new System.Drawing.Size(227, 6);
			alblRemoveselectedToolStripMenuItem.Name = "alblRemoveselectedToolStripMenuItem";
			alblRemoveselectedToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
			alblRemoveselectedToolStripMenuItem.Text = "albl_Remove_selected";
			alblRemoveselectedToolStripMenuItem.Click += new System.EventHandler(AlblRemoveselectedToolStripMenuItemClick);
			AllPropertiesToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[7] { toolStripButton1, PasteNormalToolStripButton, PasteExteralToolStripButton, LoadProperties, SaveProperties, toolStripButton2, toolStripButton3 });
			AllPropertiesToolStrip.Location = new System.Drawing.Point(0, 0);
			AllPropertiesToolStrip.Name = "AllPropertiesToolStrip";
			AllPropertiesToolStrip.Size = new System.Drawing.Size(670, 25);
			AllPropertiesToolStrip.TabIndex = 1;
			AllPropertiesToolStrip.Text = "toolStrip1";
			toolStripButton1.CheckOnClick = true;
			toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolStripButton1.Image = Tekla.Structures.ObjectPropertiesLibrary.Properties.Resources.EditTableHS1;
			toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolStripButton1.Name = "toolStripButton1";
			toolStripButton1.Size = new System.Drawing.Size(23, 22);
			toolStripButton1.ToolTipText = "albl_Edit";
			toolStripButton1.Visible = false;
			toolStripButton1.Click += new System.EventHandler(ToolStripButton1Click);
			PasteNormalToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			PasteNormalToolStripButton.Image = Tekla.Structures.ObjectPropertiesLibrary.Properties.Resources.PasteHS;
			PasteNormalToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			PasteNormalToolStripButton.Name = "PasteNormalToolStripButton";
			PasteNormalToolStripButton.Size = new System.Drawing.Size(23, 22);
			PasteNormalToolStripButton.ToolTipText = "albl_Paste_model_properties";
			PasteNormalToolStripButton.Visible = false;
			PasteExteralToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			PasteExteralToolStripButton.Image = Tekla.Structures.ObjectPropertiesLibrary.Properties.Resources.PasteHS;
			PasteExteralToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			PasteExteralToolStripButton.Name = "PasteExteralToolStripButton";
			PasteExteralToolStripButton.Size = new System.Drawing.Size(23, 22);
			PasteExteralToolStripButton.ToolTipText = "albl_Paste_external_properties";
			PasteExteralToolStripButton.Visible = false;
			LoadProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			LoadProperties.Image = Tekla.Structures.ObjectPropertiesLibrary.Properties.Resources.open_big;
			LoadProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
			LoadProperties.Name = "LoadProperties";
			LoadProperties.Size = new System.Drawing.Size(23, 22);
			LoadProperties.ToolTipText = "albl_Load";
			LoadProperties.Click += new System.EventHandler(LoadPropertiesClick);
			SaveProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			SaveProperties.Image = Tekla.Structures.ObjectPropertiesLibrary.Properties.Resources.save_as_big;
			SaveProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
			SaveProperties.Name = "SaveProperties";
			SaveProperties.Size = new System.Drawing.Size(23, 22);
			SaveProperties.ToolTipText = "albl_Save";
			SaveProperties.Click += new System.EventHandler(SavePropertiesClick);
			toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolStripButton2.Image = Tekla.Structures.ObjectPropertiesLibrary.Properties.Resources.AddTable;
			toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolStripButton2.Name = "toolStripButton2";
			toolStripButton2.Size = new System.Drawing.Size(23, 22);
			toolStripButton2.Text = "albl_Add";
			toolStripButton2.Click += new System.EventHandler(ToolStripButton2Click);
			toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolStripButton3.Image = Tekla.Structures.ObjectPropertiesLibrary.Properties.Resources.delete_big;
			toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolStripButton3.Name = "toolStripButton3";
			toolStripButton3.Size = new System.Drawing.Size(23, 22);
			toolStripButton3.Text = "albl_Remove";
			toolStripButton3.Click += new System.EventHandler(ToolStripButton3Click);
			dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewTextBoxColumn1.DataPropertyName = "Name";
			dataGridViewTextBoxColumn1.HeaderText = "";
			dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			dataGridViewTextBoxColumn1.ReadOnly = true;
			dataGridViewTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			dataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			dataGridViewTextBoxColumn2.DataPropertyName = "ReportPropertyName";
			dataGridViewTextBoxColumn2.HeaderText = "albl_Report_property";
			dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			dataGridViewTextBoxColumn2.ReadOnly = true;
			dataGridViewTextBoxColumn3.DataPropertyName = "UdaPropertyName";
			dataGridViewTextBoxColumn3.HeaderText = "albl_UDA_name";
			dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
			dataGridViewTextBoxColumn3.ReadOnly = true;
			dataGridViewTextBoxColumn4.DataPropertyName = "PropertyType";
			dataGridViewTextBoxColumn4.HeaderText = "albl_Type";
			dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
			dataGridViewTextBoxColumn4.ReadOnly = true;
			dataGridViewTextBoxColumn5.DataPropertyName = "DisplayType";
			dataGridViewTextBoxColumn5.HeaderText = "albl_Displayed_type";
			dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
			dataGridViewTextBoxColumn5.ReadOnly = true;
			dataGridViewTextBoxColumn5.Visible = false;
			dataGridViewTextBoxColumn6.DataPropertyName = "Decimals";
			dataGridViewTextBoxColumn6.HeaderText = "albl_Decimals";
			dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
			dataGridViewTextBoxColumn6.ReadOnly = true;
			dataGridViewTextBoxColumn6.Width = 70;
			dataGridViewTextBoxColumn7.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewTextBoxColumn7.DataPropertyName = "Width";
			dataGridViewTextBoxColumn7.HeaderText = "albl_Width";
			dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
			dataGridViewTextBoxColumn7.ReadOnly = true;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(AllPropertiesToolStrip);
			base.Controls.Add(AllPropertiesDGW);
			base.Name = "AllPropertiesDialog";
			base.Size = new System.Drawing.Size(670, 470);
			((System.ComponentModel.ISupportInitialize)AllPropertiesDGW).EndInit();
			contextMenuStrip1.ResumeLayout(false);
			AllPropertiesToolStrip.ResumeLayout(false);
			AllPropertiesToolStrip.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
