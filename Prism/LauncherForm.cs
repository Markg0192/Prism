using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Prism
{
    public partial class LauncherForm : Form
    {
        #region public fields

        public string SelectedVersion;

        #endregion

        public LauncherForm(List<string> listVersions)
        {
            InitializeComponent();

            TeklaVersionComboBox.Items.Clear();
            foreach (var listVersion in listVersions)
            {
                TeklaVersionComboBox.Items.Add(listVersion);
            }

            if (TeklaVersionComboBox.Items.Count > 0)
                TeklaVersionComboBox.SelectedIndex = TeklaVersionComboBox.Items.Count - 1;
        }

        #region private methods

        private void OkButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            SelectedVersion = TeklaVersionComboBox.Text;
            Close();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion
    }
}
