#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Tekla.Structures.ObjectPropertiesLibrary.Properties;

namespace Tekla.Structures.ObjectPropertiesLibrary
{
	public class QuickSearchForDGW : UserControl
	{
		private const int DelayBeforeSearch = 1000;

		private readonly Image clearButtonImage;

		private readonly Timer quickSearchTimer = new Timer();

		private DataGridView dgwToSearch;

		private int hiddenRows;

		private string previousSearchString = string.Empty;

		private List<bool> rowIsSelected;

		private string searchString = string.Empty;

		private int visibleRows;

		private IContainer components = null;

		private TextBox QuickSearchTB;

		private Button ClearButton;

		public event EventHandler UpdateRowsCount;

		public QuickSearchForDGW()
		{
			InitializeComponent();
			quickSearchTimer.Interval = 1000;
			quickSearchTimer.Tick += OnQuickSearchTimerTick;
			clearButtonImage = ClearButton.Image;
			ClearButton.Image = null;
		}

		public QuickSearchForDGW(ref DataGridView dgw)
		{
			InitializeComponent();
			quickSearchTimer.Interval = 1000;
			quickSearchTimer.Tick += OnQuickSearchTimerTick;
			clearButtonImage = ClearButton.Image;
			ClearButton.Image = null;
			dgwToSearch = dgw;
		}

		public void EmptySearchBox()
		{
			QuickSearchTB.Text = (searchString = string.Empty);
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
			visibleRows = 0;
			hiddenRows = 0;
			previousSearchString = searchString;
			string[] array = new string[dgwToSearch.Rows.Count];
			int num = 0;
			bool flag = true;
			rowIsSelected = new List<bool>();
			foreach (DataGridViewRow item in (IEnumerable)dgwToSearch.Rows)
			{
				foreach (DataGridViewCell cell in item.Cells)
				{
					if (dgwToSearch.Columns[cell.ColumnIndex].Visible && cell.Value != null)
					{
						ref string reference = ref array[num];
						reference = reference + cell.Value.ToString().ToLower() + " ";
					}
				}
				rowIsSelected.Add(item.Selected);
				num++;
			}
			if (dgwToSearch.CurrentCell != null)
			{
				dgwToSearch.CurrentCell = null;
			}
			dgwToSearch.SuspendLayout();
			DrawingControl.SuspendDrawing(dgwToSearch);
			dgwToSearch.DataBindings.DefaultDataSourceUpdateMode = DataSourceUpdateMode.Never;
			List<string[]> orSearchTerms = GetOrSearchTerms(searchString);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == null || QuickSearchIsTrue(array[i], orSearchTerms))
				{
					dgwToSearch.Rows[i].Visible = true;
					visibleRows++;
				}
				else
				{
					try
					{
						dgwToSearch.Rows[i].Visible = false;
						hiddenRows++;
					}
					catch (Exception)
					{
					}
				}
				if (!dgwToSearch.Rows[i].Visible || !rowIsSelected[i])
				{
					continue;
				}
				dgwToSearch.Rows[i].Selected = true;
				if (flag)
				{
					try
					{
						dgwToSearch.CurrentCell = dgwToSearch.Rows[i].Cells[0];
					}
					catch (Exception value)
					{
						Debug.WriteLine(value);
					}
					flag = false;
				}
			}
			dgwToSearch.DataBindings.DefaultDataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
			DrawingControl.ResumeDrawing(dgwToSearch);
			dgwToSearch.ResumeLayout();
			UpdateRows();
		}

		public void SetReferenceProperties(ref DataGridView dgw)
		{
			dgwToSearch = dgw;
			if (dgwToSearch != null && dgwToSearch.Rows != null)
			{
				visibleRows = dgwToSearch.Rows.Count;
			}
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
			QuickSearchTB = new System.Windows.Forms.TextBox();
			ClearButton = new System.Windows.Forms.Button();
			SuspendLayout();
			QuickSearchTB.Location = new System.Drawing.Point(3, 3);
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
			ClearButton.Location = new System.Drawing.Point(195, 2);
			ClearButton.Name = "ClearButton";
			ClearButton.Size = new System.Drawing.Size(20, 20);
			ClearButton.TabIndex = 2;
			ClearButton.UseVisualStyleBackColor = true;
			ClearButton.Click += new System.EventHandler(ClearButtonClick);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(ClearButton);
			base.Controls.Add(QuickSearchTB);
			base.Name = "QuickSearchForDGW";
			base.Size = new System.Drawing.Size(218, 27);
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
