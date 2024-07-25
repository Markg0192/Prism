using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Tekla.Structures.Model;

namespace Prism.CustomDialogs
{
    public partial class OkWithReportForm : Form
    {
        private List<PrismPart> _failedPrismParts;
        private List<ModelObject> _failedParts;
        private string _standardPartMessage;
        private List<string> _failedNcStrings;

        public OkWithReportForm(string message, string title, List<PrismPart> failedParts, string standardPartMessage)
        {
            InitializeComponent();
            CenterToScreen();
            lbl_WarningText.AutoSize = true;
            lbl_WarningText.MaximumSize = new Size(300, 1000);
            this.AutoSize = true;
            lbl_WarningText.Text = message;
            Text = title;
            _failedPrismParts = failedParts ?? new List<PrismPart>(); ;
            _standardPartMessage = standardPartMessage;
        }

        public OkWithReportForm(string message, string title, List<ModelObject> failedParts, string standardPartMessage)
        {
            InitializeComponent();
            CenterToScreen();
            lbl_WarningText.AutoSize = true;
            lbl_WarningText.MaximumSize = new Size(300, 1000);
            this.AutoSize = true;
            lbl_WarningText.Text = message;
            Text = title;
            _failedParts = failedParts ?? new List<ModelObject>();
            _standardPartMessage = standardPartMessage;
        }

        public OkWithReportForm(string message, string title, List<string> failedStrings, string standardPartMessage)
        {
            InitializeComponent();
            CenterToScreen();
            lbl_WarningText.AutoSize = true;
            lbl_WarningText.MaximumSize = new Size(300, 1000);
            this.AutoSize = true;
            lbl_WarningText.Text = message;
            Text = title;
            _failedNcStrings = failedStrings ?? new List<string>();
            _standardPartMessage = standardPartMessage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_ShowReport_Click(object sender, EventArgs e)
        {
            ShowFailedPartsReport();
        }

        private void ShowFailedPartsReport()
        {
            StringBuilder report = new StringBuilder();

            if (_failedNcStrings != null)
            {
                report.AppendLine("The following NC files are missing");
                foreach (string str in _failedNcStrings)
                {                    
                    report.AppendLine(str);
                }
            }
            else
            {
                report.AppendLine("The following parts do not meet the current criteria.");
                report.AppendLine("");
                if (_failedPrismParts != null)
                {
                    foreach (PrismPart part in _failedPrismParts)
                    {
                        Part p = part.ModelObject as Part;
                        string partMark = p != null ? p.GetPartMark() : "*Failed to get*";
                        string errorMessage = _standardPartMessage == "" ? part.ErrorString : _standardPartMessage;

                        report.AppendLine($"Part: {partMark} - Prelim {part.Prelim} - Part Guid: {part.Guid}"); // Assuming Part has a property called Name
                        report.AppendLine($"Error: {errorMessage}");
                        report.AppendLine($"---------------------------------------------------");
                    }
                }
                else
                {
                    foreach (Part part in _failedParts)
                    {
                        string partMark = part != null ? part.GetPartMark() : "*Failed to get*";
                        string errorMessage = _standardPartMessage == "" ? "*Failed to get*" : _standardPartMessage;
                        string prelimMark = part.GetPrelimMark() == "" ? "*Failed to get*" : part.GetPrelimMark();

                        report.AppendLine($"Part: {partMark} - Prelim {prelimMark} - Part Guid: {part.Identifier.GUID.ToString()}"); // Assuming Part has a property called Name
                        report.AppendLine($"Error: {errorMessage}");
                        report.AppendLine($"---------------------------------------------------");
                    }
                }
            }

            CreateAndOpenTempFile(report.ToString());
        }

        private void CreateAndOpenTempFile(string reportContent)
        {
            string tempFilePath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFilePath, reportContent);

                Process notepad = new Process();
                notepad.StartInfo.FileName = "notepad.exe";
                notepad.StartInfo.Arguments = tempFilePath;
                notepad.EnableRaisingEvents = true;
                notepad.Exited += (sender, e) => File.Delete(tempFilePath);
                notepad.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while creating or opening the temporary file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
