using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Tekla.Structures.ObjectPropertiesLibrary.Properties;

namespace Tekla.Structures.ObjectPropertiesLibrary
{
	public class QuickSearchForDataTable : UserControl
	{
		private const int DelayBeforeSearch = 1000;

		private readonly Image clearButtonImage;

		private readonly Timer quickSearchTimer = new Timer();

		private DataTable dataTableToSearch;

		private SearchEndedDelegate doSearchEnded;

		private SearchStartDelegate doSearchStarted;

		private int hiddenRows;

		private string previousSearchString = string.Empty;

		private DataTable resultDataTable;

		private List<bool> rowIsSelected;

		private string searchString = string.Empty;

		private int visibleRows;

		private IContainer components = null;

		private TextBox QuickSearchTB;

		private Button ClearButton;

		private ImageList imageList1;

		public event EventHandler UpdateRowsCount;

		public QuickSearchForDataTable()
		{
			InitializeComponent();
			quickSearchTimer.Interval = 1000;
			quickSearchTimer.Tick += OnQuickSearchTimerTick;
			clearButtonImage = ClearButton.Image;
			resultDataTable = new DataTable();
		}

		public QuickSearchForDataTable(ref DataTable newDataTableToSearch, SearchStartDelegate searchStarted, SearchEndedDelegate searchEnded)
		{
			InitializeComponent();
			quickSearchTimer.Interval = 1000;
			quickSearchTimer.Tick += OnQuickSearchTimerTick;
			clearButtonImage = ClearButton.Image;
			dataTableToSearch = newDataTableToSearch;
			doSearchStarted = searchStarted;
			doSearchEnded = searchEnded;
			resultDataTable = new DataTable();
		}

		public int GetHiddenRows()
		{
			return hiddenRows;
		}

		public int GetVisibleRows()
		{
			return visibleRows;
		}

		public void QuickSearchExecute()
		{
			if (doSearchStarted != null)
			{
				doSearchStarted();
			}
			visibleRows = 0;
			hiddenRows = 0;
			previousSearchString = searchString;
			resultDataTable.Columns.Clear();
			resultDataTable.Clear();
			resultDataTable = dataTableToSearch.Clone();
			string[] array = new string[dataTableToSearch.Rows.Count];
			int num = 0;
			rowIsSelected = new List<bool>();
			foreach (DataRow row in dataTableToSearch.Rows)
			{
				for (int i = 1; i < dataTableToSearch.Columns.Count; i++)
				{
					ref string reference = ref array[num];
					reference = reference + row[i].ToString().ToLower() + " ";
				}
				num++;
			}
			List<string[]> orSearchTerms = GetOrSearchTerms(searchString);
			for (int j = 0; j < array.Length; j++)
			{
				if (QuickSearchIsTrue(array[j], orSearchTerms))
				{
					resultDataTable.ImportRow(dataTableToSearch.Rows[j]);
					visibleRows++;
				}
				else
				{
					hiddenRows++;
				}
			}
			UpdateRows();
			if (doSearchEnded != null)
			{
				doSearchEnded(resultDataTable);
			}
		}

		public void SetReferenceProperties(ref DataTable newDataTableToSearch, SearchStartDelegate searchStarted, SearchEndedDelegate searchEnded)
		{
			dataTableToSearch = newDataTableToSearch;
			doSearchStarted = searchStarted;
			doSearchEnded = searchEnded;
			if (dataTableToSearch != null && dataTableToSearch.Rows != null)
			{
				visibleRows = dataTableToSearch.Rows.Count;
			}
		}

		public void SilentlyEmptySearchBox()
		{
			QuickSearchTB.TextChanged -= QuickSearchTbTextChanged;
			QuickSearchTB.Text = (searchString = string.Empty);
			HideClearButton();
			QuickSearchTB.TextChanged += QuickSearchTbTextChanged;
		}

		protected virtual void OnUpdateRowsCount(EventArgs e)
		{
			if (this.UpdateRowsCount != null)
			{
				this.UpdateRowsCount(this, e);
			}
		}

		private void ClearButtonClick(object sender, EventArgs e)
		{
			QuickSearchTB.Text = (searchString = string.Empty);
			HideClearButton();
		}

		private List<string[]> GetOrSearchTerms(string rawSearchString)
		{
			string text = string.Empty;
			List<string[]> list = new List<string[]>();
			for (int i = 0; i < rawSearchString.Length; i++)
			{
				text = rawSearchString.Replace("+ ", "+");
				if (!text.Contains("+ "))
				{
					break;
				}
			}
			for (int j = 0; j < rawSearchString.Length; j++)
			{
				text = text.Replace(" +", "+");
				if (!text.Contains(" +"))
				{
					break;
				}
			}
			char[] separator = new char[1] { ' ' };
			string[] array = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				list.Add(text2.Split('+'));
			}
			return list;
		}

		private void HideClearButton()
		{
			if (ClearButton.Image != null)
			{
				ClearButton.Image = null;
			}
		}

		private void OnQuickSearchTimerTick(object sender, EventArgs e)
		{
			quickSearchTimer.Stop();
			searchString = QuickSearchTB.Text.ToLower();
			if (previousSearchString != searchString)
			{
				QuickSearchExecute();
			}
		}

		private bool QuickSearchIsTrue(string rowString, List<string[]> searchTermsOr)
		{
			bool flag = false;
			if (searchTermsOr.Count == 0)
			{
				flag = true;
			}
			else
			{
				foreach (string[] item in searchTermsOr)
				{
					string[] array = item;
					foreach (string text in array)
					{
						string value = text;
						bool flag2 = false;
						if (text.StartsWith("!"))
						{
							value = text.Substring(1);
							flag2 = true;
						}
						if ((rowString.Contains(value) && !flag2) || (!rowString.Contains(value) && flag2))
						{
							flag = true;
							continue;
						}
						flag = false;
						break;
					}
					if (flag)
					{
						break;
					}
				}
			}
			return flag;
		}

		private void QuickSearchTbTextChanged(object sender, EventArgs e)
		{
			searchString = QuickSearchTB.Text.ToLower();
			if (string.IsNullOrEmpty(searchString))
			{
				HideClearButton();
			}
			else
			{
				ShowClearButton();
			}
			quickSearchTimer.Stop();
			quickSearchTimer.Start();
		}

		private void ShowClearButton()
		{
			if (ClearButton.Image == null)
			{
				ClearButton.Image = clearButtonImage;
			}
		}

		private void UpdateRows()
		{
			OnUpdateRowsCount(null);
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Tekla.Structures.ObjectPropertiesLibrary.QuickSearchForDataTable));
			QuickSearchTB = new System.Windows.Forms.TextBox();
			ClearButton = new System.Windows.Forms.Button();
			imageList1 = new System.Windows.Forms.ImageList(components);
			SuspendLayout();
			QuickSearchTB.Location = new System.Drawing.Point(3, 4);
			QuickSearchTB.Name = "QuickSearchTB";
			QuickSearchTB.Size = new System.Drawing.Size(190, 20);
			QuickSearchTB.TabIndex = 0;
			QuickSearchTB.TextChanged += new System.EventHandler(QuickSearchTbTextChanged);
			ClearButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
			ClearButton.FlatAppearance.BorderSize = 0;
			ClearButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
			ClearButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
			ClearButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			ClearButton.Image = Tekla.Structures.ObjectPropertiesLibrary.Properties.Resources.Cancel;
			ClearButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			ClearButton.Location = new System.Drawing.Point(197, 3);
			ClearButton.Name = "ClearButton";
			ClearButton.Size = new System.Drawing.Size(20, 20);
			ClearButton.TabIndex = 1;
			ClearButton.UseVisualStyleBackColor = true;
			ClearButton.Click += new System.EventHandler(ClearButtonClick);
			imageList1.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList1.ImageStream");
			imageList1.TransparentColor = System.Drawing.Color.Transparent;
			imageList1.Images.SetKeyName(0, "Cancel.png");
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(ClearButton);
			base.Controls.Add(QuickSearchTB);
			base.Name = "QuickSearchForDataTable";
			base.Size = new System.Drawing.Size(220, 34);
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
