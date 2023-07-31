using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Prism.CustomDialogs
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            CenterToScreen();
            TopMost = true;
            // Get the build version of the application.
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            // Set the label text to display the build version.
            lbl_AboutVersionNo.Text = "Version: " + version.ToString();
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_Contact_Click(object sender, EventArgs e)
        {
            EmailWriter.WriteHelpEmail();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string websiteUrl = "https://www.severfield.com";

            // Open the link in the default web browser.
            Process.Start(websiteUrl);
        }
    }
}