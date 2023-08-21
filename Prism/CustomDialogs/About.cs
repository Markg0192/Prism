using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Prism.CustomDialogs
{
    public partial class About : Form
    {
        string version = "";

        public About()
        {
            InitializeComponent();
            CenterToScreen();
            TopMost = true;
            // Get the build version of the application.
            version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // Set the label text to display the build version.
            lbl_AboutVersionNo.Text = "Version: " + version;
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_Contact_Click(object sender, EventArgs e)
        {
            EmailWriter.WriteHelpEmail(version);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string websiteUrl = "https://www.severfield.com";

            // Open the link in the default web browser.
            Process.Start(websiteUrl);
        }
    }
}